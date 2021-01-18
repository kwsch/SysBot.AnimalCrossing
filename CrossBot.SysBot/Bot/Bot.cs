using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrossBot.Core;
using NHSE.Core;
using SysBot.Base;

namespace CrossBot.SysBot
{
    /// <summary>
    /// Animal Crossing Drop Bot
    /// </summary>
    public sealed class Bot : SwitchRoutineExecutor<BotConfig>
    {
        public bool CleanRequested { private get; set; }
        public bool ValidateRequested { private get; set; }
        public readonly IslandState Island = new();

        public Bot(BotConfig cfg) : base(cfg)
        {
            DropState = new DropBotState(cfg.DropConfig);
            FieldItemState = new FieldItemState(cfg.FieldItemConfig);
        }

        public readonly DropBotState DropState;
        public readonly FieldItemState FieldItemState;

        public override void SoftStop() => Config.AcceptingCommands = false;

        public override async Task MainLoop(CancellationToken token)
        {
            // Disconnect our virtual controller; will reconnect once we send a button command after a request.
            Log("Detaching controller on startup as first interaction.");
            await Connection.SendAsync(SwitchCommand.DetachController(UseCRLF), token).ConfigureAwait(false);
            await Task.Delay(200, token).ConfigureAwait(false);

            // Validate inventory offset.
            Log("Checking inventory offset for validity.");
            var valid = await GetIsPlayerInventoryValid(Config.Offset, token).ConfigureAwait(false);
            if (!valid)
            {
                Log($"Inventory read from {Config.Offset} (0x{Config.Offset:X8}) does not appear to be valid.");
                if (Config.RequireValidInventoryMetadata)
                {
                    Log("Exiting!");
                    return;
                }
            }

            Log("Successfully connected to bot. Starting main loop!");
            while (!token.IsCancellationRequested)
                await DropLoop(token).ConfigureAwait(false);
        }

        private async Task DropLoop(CancellationToken token)
        {
            if (ValidateRequested)
            {
                Log("Checking inventory offset for validity.");
                var valid = await GetIsPlayerInventoryValid(Config.Offset, token).ConfigureAwait(false);
                if (!valid)
                {
                    Connection.LogError($"Inventory read from {Config.Offset} (0x{Config.Offset:X8}) does not appear to be valid.");
                    if (Config.RequireValidInventoryMetadata)
                    {
                        Connection.LogError("Turning off command processing!");
                        Config.AcceptingCommands = false;
                    }
                }
                ValidateRequested = false;
            }

            if (!Config.AcceptingCommands)
            {
                await Task.Delay(1_000, token).ConfigureAwait(false);
                return;
            }

            if (DropState.Injections.TryDequeue(out var item))
            {
                var count = await DropItems(item, token).ConfigureAwait(false);
                DropState.AfterDrop(count);
            }
            else if ((DropState.CleanRequired && DropState.Config.AutoClean) || CleanRequested)
            {
                await CleanUp(DropState.Config.PickupCount, token).ConfigureAwait(false);
                DropState.AfterClean();
                CleanRequested = false;
            }
            else if (FieldItemState.FullRefreshRequired)
            {
                await Connection.WriteBytesAsync(FieldItemState.FieldItemLayer, FieldItemState.Config.FieldItemOffset, token).ConfigureAwait(false);
                FieldItemState.AfterFullRefresh();
            }
            else if (FieldItemState.Injections.TryDequeue(out var itemSet))
            {
                await InjectDroppedItems(itemSet, FieldItemState.Config.FieldItemOffset, token).ConfigureAwait(false);
            }
            else
            {
                DropState.StillIdle();
                await Task.Delay(1_000, token).ConfigureAwait(false);
            }
        }

        private async Task<bool> GetIsPlayerInventoryValid(uint playerOfs, CancellationToken token)
        {
            PlayerItemSet.GetOffsetLength(playerOfs, out var ofs, out var len);
            var inventory = await Connection.ReadBytesAsync(ofs, len, token).ConfigureAwait(false);

            return PlayerItemSet.ValidateItemBinary(inventory);
        }

        private async Task<int> DropItems(ItemRequest drop, CancellationToken token)
        {
            int dropped = 0;
            bool first = true;
            foreach (var item in drop.Items)
            {
                await DropItem(item, first, token).ConfigureAwait(false);
                first = false;
                dropped++;
            }
            return dropped;
        }

        private async Task DropItem(Item item, bool first, CancellationToken token)
        {
            // Exit out of any menus.
            if (first)
            {
                for (int i = 0; i < 3; i++)
                    await Click(SwitchButton.B, 0_400, token).ConfigureAwait(false);
            }

            var itemName = GameInfo.Strings.GetItemName(item);
            Log($"Injecting Item: {item.DisplayItemId:X4} ({itemName}).");

            // Inject item.
            var data = item.ToBytesClass();
            var poke = SwitchCommand.Poke(Config.Offset, data, UseCRLF);
            await Connection.SendAsync(poke, token).ConfigureAwait(false);
            await Task.Delay(0_300, token).ConfigureAwait(false);

            // Open player inventory and open the currently selected item slot -- assumed to be the config offset.
            await Click(SwitchButton.X, 1_100, token).ConfigureAwait(false);
            await Click(SwitchButton.A, 0_500, token).ConfigureAwait(false);

            // Navigate down to the "drop item" option.
            var downCount = item.GetItemDropOption();
            for (int i = 0; i < downCount; i++)
                await Click(SwitchButton.DDOWN, 0_400, token).ConfigureAwait(false);

            // Drop item, close menu.
            await Click(SwitchButton.A, 0_400, token).ConfigureAwait(false);
            await Click(SwitchButton.X, 0_400, token).ConfigureAwait(false);

            // Exit out of any menus (fail-safe)
            for (int i = 0; i < 2; i++)
                await Click(SwitchButton.B, 0_400, token).ConfigureAwait(false);
        }

        private async Task CleanUp(int count, CancellationToken token)
        {
            Log("Picking up leftover items during idle time.");

            // Exit out of any menus.
            for (int i = 0; i < 3; i++)
                await Click(SwitchButton.B, 0_400, token).ConfigureAwait(false);

            // Pick up and delete.
            for (int i = 0; i < count; i++)
            {
                await Click(SwitchButton.Y, 2_000, token).ConfigureAwait(false);
                var poke = SwitchCommand.Poke(Config.Offset, Item.NONE.ToBytes(), UseCRLF);
                await Connection.SendAsync(poke, token).ConfigureAwait(false);
                await Task.Delay(1_000, token).ConfigureAwait(false);
            }
        }

        private async Task InjectDroppedItems(IReadOnlyList<FieldItemColumn> itemSet, uint fiOffset, CancellationToken token)
        {
            foreach (var column in itemSet)
            {
                await Connection.WriteBytesAsync(column.Data, fiOffset + (uint)column.Offset, token).ConfigureAwait(false);
                Log($"Wrote {column.Data.Length / Item.SIZE} tiles to field item map @ ({column.X},{column.Y}).");
            }
        }
    }
}

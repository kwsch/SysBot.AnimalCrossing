﻿using System;
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
        public readonly IslandState Island = new();

        public Bot(BotConfig cfg) : base(cfg)
        {
            DropState = new DropBotState(cfg.DropConfig);
            FieldItemState = new FieldItemState(cfg.FieldItemConfig);
            ViewState = new AdvancedViewState(this);
            VillagerState = new VillagerState(this, cfg.VillagerConfig);
        }

        public readonly DropBotState DropState;
        public readonly FieldItemState FieldItemState;
        public readonly AdvancedViewState ViewState;
        public readonly VillagerState VillagerState;

        public override void SoftStop() => Config.AcceptingCommands = false;

        public override async Task MainLoop(CancellationToken token)
        {
            if (!await BotStartup.ValidateStartup(this, token).ConfigureAwait(false))
            {
                Log("Exiting!");
                return;
            }

            Log("Successfully connected to bot. Starting main loop!");
            while (!token.IsCancellationRequested)
            {
                var result = await DropLoop(token).ConfigureAwait(false);
                if (result)
                    continue;
                Log("Exiting!");
                break;
            }
        }

        private async Task<bool> DropLoop(CancellationToken token)
        {
            // Check if our session is still active.
            if (!Config.ViewConfig.SkipSessionCheck && !await ViewState.IsLinkSessionActive(token).ConfigureAwait(false))
            {
                Log("Link Session appears to have ended. Attempting to re-open gates.");
                if (!await ViewState.StartupOpenGates(this, token).ConfigureAwait(false))
                {
                    Log("Opening gates has failed. Stopping bot loop.");
                    return false;
                }

                if (!await ViewState.StartupGetDodoCode(this, token).ConfigureAwait(false))
                {
                    Log("Unable to retrieve new dodo code. Stopping bot loop.");
                    return false;
                }
            }

            if (DropState.ValidateRequested)
            {
                Log("Checking inventory offset for validity.");
                var valid = await GetIsPlayerInventoryValid(Config.InventoryOffset, token).ConfigureAwait(false);
                if (!valid)
                {
                    Connection.LogError($"Inventory read from {Config.InventoryOffset} (0x{Config.InventoryOffset:X8}) does not appear to be valid.");
                    if (Config.RequireValidInventoryMetadata)
                    {
                        Connection.LogError("Turning off command processing!");
                        Config.AcceptingCommands = false;
                    }
                }
                DropState.ValidateRequested = false;
            }

            if (!Config.AcceptingCommands)
            {
                await Task.Delay(1_000, token).ConfigureAwait(false);
                return true;
            }

            if (DropState.Injections.TryDequeue(out var item))
            {
                var count = await DropItems(item, token).ConfigureAwait(false);
                item.Injected = count == item.Items.Count;
                Log($"Dropped {count}/{item.Items.Count} items for {item.User} ({item.UserID}).");
                DropState.AfterDrop(item, count);
            }
            else if ((DropState.CleanRequired && DropState.Config.AutoClean) || DropState.CleanRequested)
            {
                await CleanUp(DropState.Config.PickupCount, token).ConfigureAwait(false);
                DropState.AfterClean();
            }
            else if (FieldItemState.FullRefreshRequired)
            {
                const uint ofs = Offsets.FieldItemStart;
                {
                    var payload = FieldItemState.FieldItemLayer;
                    Log($"Writing Field Item Layer to 0x{ofs:X8}, size 0x{payload.Length:X} bytes.");
                    await Connection.WriteBytesAsync(payload, ofs, token).ConfigureAwait(false);
                }
                FieldItemState.AfterFullRefresh();
            }
            else if (FieldItemState.Injections.TryDequeue(out var fieldSpawn))
            {
                const uint ofs = Offsets.FieldItemStart;
                {
                    await InjectDroppedItems(fieldSpawn, ofs, token).ConfigureAwait(false);
                    Log($"Injected {fieldSpawn.Items.Count} tile columns ({fieldSpawn.RawItems.Count} items) for {fieldSpawn.User} ({fieldSpawn.UserID}).");
                    fieldSpawn.Injected = true;
                }
                FieldItemState.AfterSpawn(fieldSpawn);
            }
            else if (VillagerState.Injections.TryDequeue(out var villagerInject))
            {
                await VillagerState.InjectVillager(villagerInject, token).ConfigureAwait(false);
            }
            else
            {
                DropState.StillIdle();
                await Task.Delay(1_000, token).ConfigureAwait(false);
            }

            return true;
        }

        #region Player Inventory

        public async Task<bool> GetIsPlayerInventoryValid(uint playerOfs, CancellationToken token)
        {
            PlayerItemSet.GetOffsetLength(playerOfs, out var ofs, out var len);
            var inventory = await Connection.ReadBytesAsync(ofs, len, token).ConfigureAwait(false);

            return PlayerItemSet.ValidateItemBinary(inventory);
        }

        private async Task<int> DropItems(DropRequest drop, CancellationToken token)
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

            if (DropState.Config.Mode == DropMode.Legacy)
            {
                // Inject item.
                var data = item.ToBytesClass();
                var poke = SwitchCommand.Poke(Config.InventoryOffset, data, UseCRLF);
                await Connection.SendAsync(poke, token).ConfigureAwait(false);
                await Task.Delay(0_300, token).ConfigureAwait(false);

                // Open player inventory and open the currently selected item slot -- assumed to be the config offset.
                await Click(SwitchButton.X, 1_100, token).ConfigureAwait(false);
                await Click(SwitchButton.A, 0_500, token).ConfigureAwait(false);

                // Navigate down to the "drop item" option.
                var downCount = item.GetItemDropOption();
                for (int i = 0; i < downCount; i++)
                    await Click(SwitchButton.DDOWN, 0_400, token).ConfigureAwait(false);
            }
            else if (DropState.Config.Mode == DropMode.SingleDropOptionOverwrite)
            {
                // Inject fake item first.
                var spoof = BitConverter.GetBytes(0x9C9ul); // gold nugget
                var poke = SwitchCommand.Poke(Config.InventoryOffset, spoof, UseCRLF);
                await Connection.SendAsync(poke, token).ConfigureAwait(false);
                await Task.Delay(0_300, token).ConfigureAwait(false);

                // Open player inventory and open the currently selected item slot -- assumed to be the config offset.
                await Click(SwitchButton.X, 1_100, token).ConfigureAwait(false);
                await Click(SwitchButton.A, 0_500, token).ConfigureAwait(false);

                // Already at "drop item" option. Inject our actual item.
                var data = item.ToBytesClass();
                poke = SwitchCommand.Poke(Config.InventoryOffset, data, UseCRLF);
                await Connection.SendAsync(poke, token).ConfigureAwait(false);
                await Task.Delay(0_300, token).ConfigureAwait(false);
            }
            else
            {
                throw new IndexOutOfRangeException(nameof(DropState.Config.Mode) + " is not a known value.");
            }

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
                var poke = SwitchCommand.Poke(Config.InventoryOffset, Item.NONE.ToBytes(), UseCRLF);
                await Connection.SendAsync(poke, token).ConfigureAwait(false);
                await Task.Delay(1_000, token).ConfigureAwait(false);
            }
        }

        #endregion

        #region Field Item

        private async Task InjectDroppedItems(SpawnRequest itemSet, uint fiOffset, CancellationToken token)
        {
            foreach (var column in itemSet.Items)
            {
                await Connection.WriteBytesAsync(column.Data, fiOffset + (uint)column.Offset, token).ConfigureAwait(false);
                Log($"Wrote {column.Data.Length / Item.SIZE} tiles to field item map @ ({column.X},{column.Y}).");
            }
        }

        #endregion
    }
}

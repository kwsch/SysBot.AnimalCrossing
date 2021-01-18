using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using NHSE.Core;

namespace CrossBot.Discord
{
    // ReSharper disable once UnusedType.Global
    public class FieldItemModule : ModuleBase<SocketCommandContext>
    {
        private static int MaxRequestCount => Globals.Bot.Config.FieldItemConfig.MaxSpawnCount;

        private const string SpawnItemSummary =
            "Requests the bot spawn items with the user's provided input. " +
            "Hex Mode: Item IDs (in hex); request multiple by putting spaces between items. " +
            "Text Mode: Item names; request multiple by putting commas between items. To parse for another language, include the language code first and a comma, followed by the items.";

        [Command("spawnNHI")] [Alias("sn")]
        [Summary("Spawns a set of items.")]
        [RequireQueueRole(nameof(Globals.Self.Config.RoleUseBot))]
        public async Task RequestSpawnAsync()
        {
            if (Context.Message.Attachments.Count == 0)
            {
                await ReplyAsync("No items requested; silly goose. Attach an `nhi` file next time, or request specific items.").ConfigureAwait(false);
                return;
            }

            var att1 = Context.Message.Attachments.ElementAt(0);
            var fn = att1.Filename;
            if (!fn.EndsWith(".nhi"))
            {
                await ReplyAsync("I only accept `nhi` files.").ConfigureAwait(false);
                return;
            }

            var size = att1.Size;
            if (size % Item.SIZE != 0 || size == 0)
            {
                await ReplyAsync("That `nhi` does not appear to be a valid size.").ConfigureAwait(false);
                return;
            }

            var max = Globals.Bot.FieldItemState.Config.MaxSpawnCount * 10;
            if (size > Item.SIZE * max)
            {
                await ReplyAsync($"That `nhi` file is way too big. I only allow at most {max} items from an `nhi` file.").ConfigureAwait(false);
                return;
            }

            var data = await NetUtil.DownloadFromUrlAsync(att1.Url).ConfigureAwait(false);
            var items = Item.GetArray(data);
            await SpawnItems(items).ConfigureAwait(false);
        }

        [Command("spawnItems")] [Alias("si")]
        [Summary("Spawns a set of items.")]
        [RequireQueueRole(nameof(Globals.Self.Config.RoleUseBot))]
        public async Task RequestSpawnAsync([Summary(SpawnItemSummary)][Remainder] string request)
        {
            var bot = Globals.Bot;
            if (bot.Config.RequireJoin && bot.Island.GetVisitor(Context.User.Id) == null && !Globals.Self.Config.CanUseSudo(Context.User.Id))
            {
                await ReplyAsync($"You must `{IslandModule.cmdJoin}` the island before using this command.").ConfigureAwait(false);
                return;
            }
            var cfg = Globals.Bot.Config;
            var items = ItemParser.GetItemsFromUserInput(request, cfg.DropConfig);
            await SpawnItems(items.ToArray()).ConfigureAwait(false);
        }

        private async Task SpawnItems(IReadOnlyList<Item> items)
        {
            if (items.Count > MaxRequestCount)
            {
                var clamped = $"Users are limited to {MaxRequestCount} items per command. Please use this bot responsibly.";
                await ReplyAsync(clamped).ConfigureAwait(false);
                items = items.Take(MaxRequestCount).ToArray();
            }

            var bot = Globals.Bot;
            var fi = bot.FieldItemState;
            var cfg = fi.Config;
            var height = cfg.GetSpawnHeight(items.Count);
            (int x, int y) = fi.GetNextInjectCoordinates(items.Count, height);
            string atCoords = $"at coordinates ({x},{y}) (count:{items.Count}, height:{Math.Min(items.Count, height)})";

            bool canInject = FieldItemDropper.CanFitDropped(x, y, items.Count, height, cfg.SpawnMinX, cfg.SpawnMaxX, cfg.SpawnMinY, cfg.SpawnMaxY);
            if (!canInject)
            {
                await ReplyAsync($"Unable to inject {atCoords}. Please confirm the bot is configured correctly, and contact the owner.").ConfigureAwait(false);
                return;
            }

            var column = FieldItemDropper.InjectItemsAsDropped(x, y, items, height);
            fi.Injections.Enqueue(column);
            var msg = $"Item spawn request{(items.Count > 1 ? "s" : string.Empty)} {atCoords} will be executed momentarily.";
            await ReplyAsync(msg).ConfigureAwait(false);
        }
    }
}

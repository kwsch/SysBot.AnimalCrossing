using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossBot.Core;
using Discord.Commands;
using NHSE.Core;

namespace CrossBot.Discord
{
    // ReSharper disable once UnusedType.Global
    public class DropModule : ModuleBase<SocketCommandContext>
    {
        private static int MaxRequestCount => Globals.Bot.Config.DropConfig.MaxDropCount;

        [Command("clean")]
        [Summary("Picks up items around the bot.")]
        [RequireQueueRole(nameof(Globals.Self.Config.RoleUseBot))]
        public async Task RequestCleanAsync()
        {
            var bot = Globals.Bot;
            if (bot.Config.RequireJoin && bot.Island.GetVisitor(Context.User.Id) == null && !Globals.Self.Config.CanUseSudo(Context.User.Id))
            {
                await ReplyAsync($"You must `{IslandModule.cmdJoin}` the island before using this command.").ConfigureAwait(false);
                return;
            }
            if (!Globals.Bot.Config.AllowClean)
            {
                await ReplyAsync("Clean functionality is currently disabled.").ConfigureAwait(false);
                return;
            }
            Globals.Bot.CleanRequested = true;
            await ReplyAsync("A clean request will be executed momentarily.").ConfigureAwait(false);
        }

        private const string DropItemSummary =
            "Requests the bot drop an item with the user's provided input. " +
            "Hex Mode: Item IDs (in hex); request multiple by putting spaces between items. " +
            "Text Mode: Item names; request multiple by putting commas between items. To parse for another language, include the language code first and a comma, followed by the items.";

        [Command("dropItem")]
        [Alias("drop")]
        [Summary("Drops a custom item (or items).")]
        [RequireQueueRole(nameof(Globals.Self.Config.RoleUseBot))]
        public async Task RequestDropAsync([Summary(DropItemSummary)][Remainder]string request)
        {
            var bot = Globals.Bot;
            if (bot.Config.RequireJoin && bot.Island.GetVisitor(Context.User.Id) == null && !Globals.Self.Config.CanUseSudo(Context.User.Id))
            {
                await ReplyAsync($"You must `{IslandModule.cmdJoin}` the island before using this command.").ConfigureAwait(false);
                return;
            }
            var cfg = Globals.Bot.Config;
            var items = ItemParser.GetItemsFromUserInput(request, cfg.DropConfig);
            await DropItems(items).ConfigureAwait(false);
        }

        private const string DropDIYSummary =
            "Requests the bot drop a DIY recipe with the user's provided input. " +
            "Hex Mode: DIY Recipe IDs (in hex); request multiple by putting spaces between items. " +
            "Text Mode: DIY Recipe Item names; request multiple by putting commas between items. To parse for another language, include the language code first and a comma, followed by the items.";

        [Command("dropDIY")]
        [Alias("diy")]
        [Summary("Drops a DIY recipe with the requested recipe ID(s).")]
        [RequireQueueRole(nameof(Globals.Self.Config.RoleUseBot))]
        public async Task RequestDropDIYAsync([Summary(DropDIYSummary)][Remainder]string recipeIDs)
        {
            var bot = Globals.Bot;
            if (bot.Config.RequireJoin && bot.Island.GetVisitor(Context.User.Id) == null && !Globals.Self.Config.CanUseSudo(Context.User.Id))
            {
                await ReplyAsync($"You must `{IslandModule.cmdJoin}` the island before using this command.").ConfigureAwait(false);
                return;
            }
            var items = ItemParser.GetDIYsFromUserInput(recipeIDs);
            await DropItems(items).ConfigureAwait(false);
        }

        private async Task DropItems(IReadOnlyCollection<Item> items)
        {
            if (items.Count > MaxRequestCount)
            {
                var clamped = $"Users are limited to {MaxRequestCount} items per command. Please use this bot responsibly.";
                await ReplyAsync(clamped).ConfigureAwait(false);
                items = items.Take(MaxRequestCount).ToArray();
            }

            var user = Context.User;
            var requestInfo = new ItemRequest(user.Username, user.Id, items);
            Globals.Bot.DropState.Injections.Enqueue(requestInfo);

            var msg = $"Item drop request{(requestInfo.Items.Count > 1 ? "s" : string.Empty)} will be executed momentarily.";
            await ReplyAsync(msg).ConfigureAwait(false);
        }
    }
}

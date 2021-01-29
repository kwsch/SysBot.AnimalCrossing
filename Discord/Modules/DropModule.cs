﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using NHSE.Core;

namespace SysBot.AnimalCrossing {
    public class DropModule:ModuleBase<SocketCommandContext> {
        [Command("code")]
        [Alias("dodo")]
        [Summary("Prints the Dodo Code for the island.")]
        public async Task RequestDodoCodeAsync() {
            await Task.Delay(100).ConfigureAwait(false);
        }

        private const string DropItemSummary =
            "Requests the bot drop an item with the user's provided input. " +
            "Hex Mode: Item IDs (in hex); request multiple by putting spaces between items. " +
            "Text Mode: Item names; request multiple by putting commas between items. To parse for another language, include the language code first and a comma, followed by the items.";

        [Command("qview")]
        [Alias("queueview", "view", "queue", "q")]
        [Summary("Lets users know who is currently on the island!")]
        public async Task RequestQueueViewAsync() {
            if (Globals.Bot.IsQueueFull()) await ReplyAsync($"Queue is currently full. Please wait!~").ConfigureAwait(false);
            Globals.Bot.ShowQueue ^= true;
        }

        [Command("dropItem")]
        [Alias("drop")]
        [Summary("Drops a custom item (or items).")]
        public async Task RequestDropAsync([Summary(DropItemSummary)][Remainder] string request) {
            var items = DropUtil.GetItemsFromUserInput(request);
            await DropItems(items).ConfigureAwait(false);
        }

        private const string DropDIYSummary =
            "Requests the bot drop a DIY recipe with the user's provided input. " +
            "Hex Mode: DIY Recipe IDs (in hex); request multiple by putting spaces between items. " +
            "Text Mode: DIY Recipe Item names; request multiple by putting commas between items. To parse for another language, include the language code first and a comma, followed by the items.";

        [Command("dropDIY")]
        [Alias("diy")]
        [Summary("Drops a DIY recipe with the requested recipe ID(s).")]
        public async Task RequestDropDIYAsync([Summary(DropDIYSummary)][Remainder] string recipeIDs) {
            var items = DropUtil.GetDIYsFromUserInput(recipeIDs);
            await DropItems(items).ConfigureAwait(false);
        }

        private async Task DropItems(IReadOnlyCollection<Item> items) {

            if (Globals.Bot.Config.DISCORD_ACCEPTINGDROPS) {
                if (items.Count > Globals.Bot.MaxRequestCount) {
                    var clamped = $"Users are limited to { Globals.Bot.MaxRequestCount} items per command. Please use this bot responsibly.";
                    await ReplyAsync(clamped).ConfigureAwait(false);
                    items = items.Take(Globals.Bot.MaxRequestCount).ToArray();
                }

                var requestInfo = new ItemRequest(Context.User.Username, items);
                Globals.Bot.Injections.Enqueue(requestInfo);

                var msg = $"Item drop request{(requestInfo.Items.Count > 1 ? "s" : string.Empty)} will be executed momentarily.";
                await ReplyAsync(msg).ConfigureAwait(false);
            } else {
                await ReplyAsync("Requests are disabled at the moment!").ConfigureAwait(false);
            }
        }
    }
}

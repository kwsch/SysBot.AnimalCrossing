using System;
using System.Linq;
using System.Threading.Tasks;
using CrossBot.Core;
using CrossBot.SysBot;
using Discord.Commands;
using NHSE.Core;
using NHSE.Villagers;

namespace CrossBot.Discord
{
    // ReSharper disable once UnusedType.Global
    public class VillagerModule : ModuleBase<SocketCommandContext>
    {
        private static VillagerState State => Globals.Bot.VillagerState;
        private static SlotTracker Tracker => Globals.Bot.VillagerState.Tracker;
        private static VillagerConfig Config => Globals.Bot.Config.VillagerConfig;

        [Command("injectVillager"), Alias("iv")]
        [Summary("Injects a villager based on the internal name.")]
        [RequireQueueRole(nameof(Globals.Self.Config.RoleUseBot))]
        public async Task InjectVillagerAsync(string internalName)
        {
            var bot = Globals.Bot;
            if (bot.Config.RequireJoin && bot.Island.GetVisitor(Context.User.Id) == null && !Globals.Self.Config.CanUseSudo(Context.User.Id))
            {
                await ReplyAsync($"You must `{IslandModule.cmdJoin}` the island before using this command.").ConfigureAwait(false);
                return;
            }
            if (Globals.Bot.Config.VillagerConfig.AllowVillagerInjection)
            {
                await ReplyAsync("Villager functionality is currently disabled.").ConfigureAwait(false);
                return;
            }

            if (!VillagerResources.IsVillagerDataKnown(internalName))
                internalName = GameInfo.Strings.VillagerMap.First(z => string.Equals(z.Value, internalName, StringComparison.InvariantCultureIgnoreCase)).Key;

            if (!Tracker.CanAdd(Config))
            {
                await ReplyAsync("Cannot add villager at this time; other villagers have been added too recently.").ConfigureAwait(false);
                return;
            }

            int slot = Tracker.Add();
            var vd = State.Existing[slot + Config.MinVillagerIndex];
            if (vd == null)
            {
                await ReplyAsync("Villager data not initialized. Tell the bot owner to enable the correct setting.").ConfigureAwait(false);
                return;
            }

            var replace = VillagerSwap.GetReplacementVillager(vd, internalName, true);
            var user = Context.User;
            var mention = Context.User.Mention;
            var request = new VillagerRequest(user.Username, user.Id, new[] {replace})
            {
                Index = slot,
                OnFinish = success =>
                {
                    var reply = success
                        ? $"Villager has been injected by the bot at Index {slot}. Please go talk to them!"
                        : "Failed to inject villager. Please tell the bot owner to look at the logs!";
                    Task.Run(async () => await ReplyAsync($"{mention}: {reply}").ConfigureAwait(false));
                }
            };

            State.Injections.Enqueue(request);

            var msg = $"{mention}: Villager inject request{(request.Items.Count > 1 ? "s have" : "has")} been added to the queue and will be injected momentarily.";
            await ReplyAsync(msg).ConfigureAwait(false);
        }

        [Command("villagerName")]
        [Alias("vn", "nv", "name")]
        [Summary("Gets the internal name of a villager.")]
        [RequireQueueRole(nameof(Globals.Self.Config.RoleUseBot))]
        public async Task GetVillagerInternalNameAsync([Summary("Language code to search with")] string language, [Summary("Villager name")][Remainder] string villagerName)
        {
            var strings = GameInfo.GetStrings(language);
            await ReplyVillagerName(strings, villagerName).ConfigureAwait(false);
            await ReplyAsync($"Visitor count: {Globals.Bot.Island.Count}.").ConfigureAwait(false);
        }

        [Command("villagerName")]
        [Alias("vn", "nv", "name")]
        [Summary("Gets the internal name of a villager.")]
        [RequireQueueRole(nameof(Globals.Self.Config.RoleUseBot))]
        public async Task GetVillagerInternalNameAsync([Summary("Villager name")][Remainder] string villagerName)
        {
            var strings = GameInfo.Strings;
            await ReplyVillagerName(strings, villagerName).ConfigureAwait(false);
        }

        private async Task ReplyVillagerName(GameStrings strings, string villagerName)
        {
            var map = strings.VillagerMap;
            var result = map.FirstOrDefault(z => string.Equals(villagerName, z.Value, StringComparison.InvariantCultureIgnoreCase));
            if (string.IsNullOrWhiteSpace(result.Key))
            {
                await ReplyAsync($"No villager found of name {villagerName}.").ConfigureAwait(false);
                return;
            }
            await ReplyAsync($"{villagerName}={result.Key}").ConfigureAwait(false);
        }
    }
}

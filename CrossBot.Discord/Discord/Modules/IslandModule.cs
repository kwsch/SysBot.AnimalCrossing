using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace CrossBot.Discord
{
    // ReSharper disable once UnusedType.Global
    public class IslandModule : ModuleBase<SocketCommandContext>
    {
        public const string cmdJoin = "join";
        private const string cmdLeave = "leave";

        [Command("code")] [Alias("dodo", "dc")]
        [Summary("Prints the Dodo Code for the island.")]
        [RequireQueueRole(nameof(Globals.Self.Config.RoleUseBot))]
        public async Task RequestDodoCodeAsync()
        {
            await ReplyAsync($"Dodo Code: {Globals.Bot.Island.DodoCode}.").ConfigureAwait(false);
        }

        [Command("visitorCount")] [Alias("count", "cv", "vc")]
        [Summary("Prints the amount of visitors on the island.")]
        [RequireQueueRole(nameof(Globals.Self.Config.RoleUseBot))]
        public async Task RequestVisitorCountAsync()
        {
            await ReplyAsync($"Visitor count: {Globals.Bot.Island.Count}.").ConfigureAwait(false);
        }

        [Command("visitorList")] [Alias("listVisitors", "lv", "vl")]
        [Summary("Prints the amount of visitors on the island.")]
        [RequireQueueRole(nameof(Globals.Self.Config.RoleUseBot))]
        public async Task RequestVisitorListAsync()
        {
            var visitors = Globals.Bot.Island.CurrentVisitors.Select(z => z.ToString());
            await ReplyAsync($"Visitor list:\r\n{string.Join("\r\n", visitors)}").ConfigureAwait(false);
        }

        [Command(cmdJoin)] [Alias("j")]
        [Summary("Indicates the user is joining the island.")]
        [RequireQueueRole(nameof(Globals.Self.Config.RoleUseBot))]
        public async Task JoinIslandAsync()
        {
            var user = Context.User;
            if (Context.Message.MentionedUsers.Count > 0 && Globals.Self.Config.CanUseSudo(user.Id))
            {
                if (Context.Message.MentionedUsers.Count > 1)
                {
                    await ReplyAsync("Too many mentions. One user at a time please.").ConfigureAwait(false);
                    return;
                }
                user = Context.Message.MentionedUsers.ElementAt(0);
            }
            var island = Globals.Bot.Island;
            var result = island.Arrive(user.Username, user.Id);
            if (!result)
            {
                var detail = island.GetVisitor(user.Id);
                if (detail == null)
                    await ReplyAsync("Stop being sneaky.").ConfigureAwait(false);
                else
                    await ReplyAsync($"You have already joined the island -- at {detail.JoinTime:T} ({detail.Duration:g} ago).").ConfigureAwait(false);
                return;
            }

            await ReplyAsync($"{user.Username} has joined the island.\r\nWhen you are leaving please use the `{cmdLeave}` command.\r\nCurrent visitor count: {island.Count}.").ConfigureAwait(false);
        }

        [Command("leave")] [Alias("l")]
        [Summary("Indicates the user is leaving the island.")]
        [RequireQueueRole(nameof(Globals.Self.Config.RoleUseBot))]
        public async Task LeaveIslandAsync()
        {
            var user = Context.User;
            if (Context.Message.MentionedUsers.Count > 0 && Globals.Self.Config.CanUseSudo(user.Id))
            {
                if (Context.Message.MentionedUsers.Count > 1)
                {
                    await ReplyAsync("Too many mentions. One user at a time please.").ConfigureAwait(false);
                    return;
                }
                user = Context.Message.MentionedUsers.ElementAt(0);
            }
            var island = Globals.Bot.Island;
            var result = island.Depart(user.Id);
            if (result == null)
            {
                await ReplyAsync($"You must first be on the island via `{cmdJoin}`.").ConfigureAwait(false);
                return;
            }

            await ReplyAsync($"{user.Username} has left the island the island.\r\nVisit time: {result.Duration:g}\r\nCurrent visitor count: {island.Count}.").ConfigureAwait(false);
        }

        [Command("time")] [Alias("vt", "visitTime", "timeVisit", "duration")]
        [Summary("Prints the amount of time that a joined user has been on the island in their current visit session.")]
        [RequireQueueRole(nameof(Globals.Self.Config.RoleUseBot))]
        public async Task PrintVisitTimeAsync()
        {
            var user = Context.User;
            if (Context.Message.MentionedUsers.Count > 0 && Globals.Self.Config.CanUseSudo(user.Id))
            {
                if (Context.Message.MentionedUsers.Count > 1)
                {
                    await ReplyAsync("Too many mentions. One user at a time please.").ConfigureAwait(false);
                    return;
                }
                user = Context.Message.MentionedUsers.ElementAt(0);
            }
            var island = Globals.Bot.Island;
            var detail = island.GetVisitor(user.Id);
            if (detail == null)
            {
                await ReplyAsync($"Requested user is not currently on the island. They must join via `{cmdJoin}` prior to having a visit duration.").ConfigureAwait(false);
                return;
            }

            await ReplyAsync($"{user.Username} has been on the island since {detail.JoinTime:T} ({detail.Duration:g} ago).").ConfigureAwait(false);
        }
    }
}

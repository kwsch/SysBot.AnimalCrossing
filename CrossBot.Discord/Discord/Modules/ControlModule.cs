using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using SysBot.Base;

namespace CrossBot.Discord
{
    // ReSharper disable once UnusedType.Global
    public class ControlModule : ModuleBase<SocketCommandContext>
    {
        [Command("detach")]
        [Summary("Detaches the virtual controller so the operator can use their own handheld controller temporarily.")]
        [RequireSudo]
        public async Task DetachAsync()
        {
            await ReplyAsync("A controller detach request will be executed momentarily.").ConfigureAwait(false);
            var bot = Globals.Bot;
            await bot.Connection.SendAsync(SwitchCommand.DetachController(), CancellationToken.None).ConfigureAwait(false);
        }

        [Command("setCode")]
        [Summary("Sets a string to the Dodo Code property for users to call via the associated command.")]
        [RequireSudo]
        public async Task SetDodoCodeAsync([Summary("Current Dodo Code for the island.")][Remainder]string code)
        {
            var bot = Globals.Bot;
            bot.Island.DodoCode = code;
            await ReplyAsync($"The dodo code for the bot has been set to {code}.").ConfigureAwait(false);
        }

        [Command("toggleRequests")]
        [Summary("Toggles accepting drop requests.")]
        [RequireSudo]
        public async Task ToggleRequestsAsync()
        {
            bool value = (Globals.Bot.Config.AcceptingCommands ^= true);
            await ReplyAsync($"Accepting drop requests: {value}.").ConfigureAwait(false);
        }

        [Command("validate")]
        [Summary("Validates the bot inventory offset again.")]
        [RequireQueueRole(nameof(Globals.Self.Config.RoleUseBot))]
        public async Task RequestValidateAsync()
        {
            var bot = Globals.Bot;
            if (bot.Config.DropConfig.RequireJoin && bot.Island.GetVisitor(Context.User.Id) == null && !Globals.Self.Config.CanUseSudo(Context.User.Id))
            {
                await ReplyAsync($"You must `{IslandModule.cmdJoin}` the island before using this command.").ConfigureAwait(false);
                return;
            }
            if (!Globals.Bot.Config.AllowValidate)
            {
                await ReplyAsync("Validate functionality is currently disabled.").ConfigureAwait(false);
                return;
            }
            Globals.Bot.ValidateRequested = true;
            await ReplyAsync("A validate request will be executed momentarily. Check the logs for the result.").ConfigureAwait(false);
        }
    }
}

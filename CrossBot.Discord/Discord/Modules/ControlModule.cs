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

        [Command("toggleSpawns")]
        [Summary("Toggles accepting spawn requests.")]
        [RequireSudo]
        public async Task ToggleSpawnsAsync()
        {
            bool value = (Globals.Bot.FieldItemState.Config.InjectFieldItemRequest ^= true);
            await ReplyAsync($"Accepting spawn requests: {value}.").ConfigureAwait(false);
        }

        [Command("toggleLayer")]
        [Summary("Toggles layer refresh cycle.")]
        [RequireSudo]
        public async Task ToggleLayerLoadAsync()
        {
            bool value = (Globals.Bot.FieldItemState.Config.InjectFieldItemLayer ^= true);
            await ReplyAsync($"Refreshing field layer: {value}.").ConfigureAwait(false);
        }

        [Command("reloadLayer")]
        [Summary("Re-initializes the layer file. Next bot loop will reload if enabled.")]
        [RequireSudo]
        public async Task ReloadLayerAsync()
        {
            var fi = Globals.Bot.FieldItemState;
            bool value = fi.LoadFieldItemLayer(fi.Config.FieldItemLayerPath);
            if (value)
            {
                fi.ForceReload();
                await ReplyAsync("Reloaded from path. Sending to game soon.").ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync("File not found.").ConfigureAwait(false);
            }
        }

        [Command("validate")]
        [Summary("Validates the bot inventory offset again.")]
        [RequireQueueRole(nameof(Globals.Self.Config.RoleUseBot))]
        public async Task RequestValidateAsync()
        {
            var bot = Globals.Bot;
            if (bot.Config.RequireJoin && bot.Island.GetVisitor(Context.User.Id) == null && !Globals.Self.Config.CanUseSudo(Context.User.Id))
            {
                await ReplyAsync($"You must `{IslandModule.cmdJoin}` the island before using this command.").ConfigureAwait(false);
                return;
            }
            if (!Globals.Bot.Config.AllowValidate)
            {
                await ReplyAsync("Validate functionality is currently disabled.").ConfigureAwait(false);
                return;
            }
            Globals.Bot.DropState.ValidateRequested = true;
            await ReplyAsync("A validate request will be executed momentarily. Check the logs for the result.").ConfigureAwait(false);
        }
    }
}

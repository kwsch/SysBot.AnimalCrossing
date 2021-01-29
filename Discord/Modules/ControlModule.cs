using System.Threading;
using System;
using System.Threading.Tasks;
using Discord.Commands;
using SysBot.Base;
using System.Linq;

namespace SysBot.AnimalCrossing
{
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
        [Alias("setDodo")]
        [Summary("Sets a string to the Dodo Code property for users to call via the associated command.")]
        [RequireSudo]
        public async Task SetDodoCodeAsync([Summary("Current Dodo Code for the island.")][Remainder] string code)
        {
            Globals.Bot.DodoCode = code;
            await ReplyAsync($"The dodo code for the bot has been set to {code}.").ConfigureAwait(false);
        }

        [Command("toggleRequests")]
        [Alias("tr")]
        [Summary("Toggles accepting drop requests.")]
        [RequireSudo]
        public async Task ToggleRequestsAsync()
        {
            bool value = Globals.Bot.Config.DISCORD_ACCEPTINGDROPS ^= true;
            await ReplyAsync($"Accepting drop requests: {value}.").ConfigureAwait(false);
        }
        [Command("toggleLookup")]
        [Alias("tl")]
        [Summary("Toggles lookup command use.")]
        [RequireSudo]
        public async Task ToggleLookupAsync()
        {
            bool value = Globals.Bot.Config.DISCORD_ACCEPTINGLOOKUPS ^= true;
            await ReplyAsync($"Accepting lookups: {value}.").ConfigureAwait(false);
        }
        [Command("toggletwitchdodo")]
        [Alias("ttd", "twitchdodo")]
        [Summary("Toggles allowance for posting dodo in twitch chat!")]
        [RequireSudo]
        public async Task ToggleTwitchDodoAsync() {
            bool value = Globals.Bot.Config.TWITCH_SHARE_DODO ^= true;
            await ReplyAsync($"Share Dodo with twitch: {value}.").ConfigureAwait(false);
        }
        [Command("mutetwitch")]
        [Alias("mt", "toggletwitch")]
        [Summary("Toggles allowance for posting in twitch chat at all!")]
        [RequireSudo]
        public async Task ToggleTwitchMuteAsync() {
            bool value = Globals.Bot.MuteTwitch ^= true;
            await ReplyAsync($"Mute twitch: {value}.").ConfigureAwait(false);
        }
        [Command("droplimit")]
        [Summary("Set amount of drops a user can request at once! Default: 10")]
        [RequireSudo]
        public async Task DropLimit(int count)
        {
            Globals.Bot.MaxRequestCount = (count < 3) ? 3 : (count > 40) ? 40 : count;
            await ReplyAsync($"Drop-limit set to: {Globals.Bot.MaxRequestCount}.").ConfigureAwait(false);
        }
        /*
         * MOVEMENT COMMANDS
         */
        [Command("stopmove")]
        [Alias("stopclick", "stopwalk")]
        [Summary("Remotely control player's click.")]
        [RequireSudo]
        public async Task PlayerStop()
        {
            var bot = Globals.Bot;
            bot.MovementActive = false;
            await ReplyAsync("Player has stopped moving!").ConfigureAwait(false);
        }
        [Command("pulloffset")]
        [Alias("pull")]
        [Summary("hmmmm")]
        [RequireSudo]
        public async Task Pulloffsetval()
        {
            var bot = Globals.Bot;
            bot.PullValue = true;
            await ReplyAsync("testing beep beep boop!").ConfigureAwait(false);
        }
        [Command("airport")]
        [Summary("Once inside airport entrance hit this!")]
        [RequireSudo]
        public async Task AirportMacroAsync() {
            var bot = Globals.Bot;
            if (bot.RetrieveDodo != true) bot.RetrieveDodo = true;
            await ReplyAsync("Player should be retreiving code now!").ConfigureAwait(false);
        }
        [Command("click")]
        [Summary("Remotely control player's click.")]
        [RequireSudo]
        public async Task PlayerClick(string value)
        {
            var bot = Globals.Bot;
            if (value.ToLower() != "home") { 
                if (Enum.GetNames(typeof(SwitchButton)).Any(x => x.ToLower() == value.ToLower()) && !bot.MovementActive) {
                    bot.MovementButton = (SwitchButton)Enum.Parse(typeof(SwitchButton), value.ToUpper());
                    bot.MovementActive = true;
                    bot.MovementType = 0;
                    await ReplyAsync("Player is clicking now!").ConfigureAwait(false);
                }
                else if (bot.MovementActive)
                {
                    await ReplyAsync("Wait till click/movement is finished!").ConfigureAwait(false);
                }
                else
                {
                    await ReplyAsync("Invalid button!").ConfigureAwait(false);
                }
            }
        }
        [Command("walk")]
        [Alias("move")]
        [Summary("Remotely control player's movement (walk).")]
        [RequireSudo]
        public async Task PlayerWalk([Summary("Direction")] string direction, [Summary("Run duration (1000 = 1 second")] int duration = 500) {
            var bot = Globals.Bot;
            if (Enum.GetNames(typeof(Directions)).Any(x => x.ToLower() == direction.ToLower()) && !bot.MovementActive) {
                bot.MovementDirection = (int)(Directions)Enum.Parse(typeof(Directions), direction.ToUpper());
                bot.MovementActive = true;
                bot.MovementType = 1;
                bot.MovementState = false;
                bot.MovementDuration = duration;
                await ReplyAsync("Player is moving now!").ConfigureAwait(false);
            } else if (bot.MovementActive) {
                await ReplyAsync("Wait till click/movement is finished!").ConfigureAwait(false);
            } else {
                await ReplyAsync("Invalid direction!").ConfigureAwait(false);
            }
        }
        [Command("run")]
        [Alias("sprint")]
        [Summary("Remotely control player's movement (run).")]
        [RequireSudo]
        public async Task PlayerRun([Summary("Direction")] string direction, [Summary("Run duration (1000 = 1 second")] int duration = 500) {
            var bot = Globals.Bot;
            if (Enum.GetNames(typeof(Directions)).Any(x => x.ToLower() == direction.ToLower()) && !bot.MovementActive) {
                bot.MovementDirection = (int)(Directions)Enum.Parse(typeof(Directions), direction.ToUpper());
                bot.MovementActive = true;
                bot.MovementType = 1;
                bot.MovementState = true;
                bot.MovementDuration = duration;
                await ReplyAsync("Player is moving now!").ConfigureAwait(false);
            } else if (bot.MovementActive) {
                await ReplyAsync("Wait till click/movement is finished!").ConfigureAwait(false);
            } else {
                await ReplyAsync("Invalid direction!").ConfigureAwait(false);
            }
        }
        [Command("view")]
        [Alias("look")]
        [Summary("Remotely control player's view.")]
        [RequireSudo]
        public async Task PlayerView([Summary("Direction")] string direction, [Summary("Run duration (1000 = 1 second")] int duration = 500) {
            var bot = Globals.Bot;
            if (direction.ToLower() == "up" || direction.ToLower() == "down" && !bot.MovementActive) {
                bot.MovementState = (direction.ToLower() == "up") ? true : false;
                bot.MovementActive = true;
                bot.MovementType = 2;
                await ReplyAsync("Player is moving view now!").ConfigureAwait(false);
            } else if (bot.MovementActive) {
                await ReplyAsync("Wait till click/movement/view is finished!").ConfigureAwait(false);
            } else {
                await ReplyAsync("Invalid direction!").ConfigureAwait(false);
            }
        }
        public enum Directions
        {
            DOWN = 0,
            UP = 1,
            LEFT = 2,
            RIGHT = 3,
            UP_LEFT = 4,
            UP_RIGHT = 5,
            DOWN_LEFT = 6,
            DOWN_RIGHT = 7
        }
    }
}

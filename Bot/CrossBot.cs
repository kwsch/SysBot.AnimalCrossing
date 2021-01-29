using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System;
using NHSE.Core;
using SysBot.Base;
using System.Text;
using Discord.WebSocket;
using Discord;
using System.Collections.Generic;

namespace SysBot.AnimalCrossing {
    public sealed class CrossBot:SwitchRoutineExecutor<CrossBotConfig> {
        public readonly ConcurrentQueue<ItemRequest> Injections = new ConcurrentQueue<ItemRequest>();
        public SwitchButton MovementButton { get; set; } = SwitchButton.A;
        public int MovementDirection { get; set; } = 0;
        public int MovementType { get; set; } = 0;
        public bool RetrieveDodo { get; set; } = false;
        public bool MovementActive { get; set; } = false;
        public bool MovementState { get; set; } = false;
        public int MovementDuration { get; set; } = 500;  // .5 second
        public int WrapCounter { get; set; } = 0;
        public int MaxRequestCount { get; set; } = 10;
        public int Delay { get; set; } = 0;
        public int DelayTwitchDodoShare { get; set; } = 0;
        public bool MuteTwitch { get; set; } = false;
        public bool ShowQueue { get; set; } = false;
        public int LastUserCount { get; set; } = 0;
        public bool FirstRun { get; set; } = true;
        public string LastDodo { get; set; } = "No code set yet.";
        string[] OnlinePlayerList { get; set; } = { "", "", "", "", "", "", "", "" };
        public DateTime[] OnlinePlayerTime { get; set; } = new DateTime[8];

        public string DodoCode { get; set; } = "No code set yet.";
        public CrossBot(CrossBotConfig cfg) : base(cfg) => State = new DropBotState();
        public readonly DropBotState State;
        public bool PullValue { get; set; } = false;
        public TimeSpan TimeNowOnline;
        public List<IMessageChannel> Channel = new List<IMessageChannel>();

        public override void SoftStop() => Config.DISCORD_ACCEPTINGDROPS = false;
        protected override async Task MainLoop(CancellationToken token) {
            // Disconnect our virtual controller; will reconnect once we send a button command after a request.
            LogUtil.LogInfo("Detaching controller on startup as first interaction.", Config.IP);
            await Connection.SendAsync(SwitchCommand.DetachController(), token).ConfigureAwait(false);
            await Task.Delay(200, token).ConfigureAwait(false);

            LogUtil.LogInfo("Checking inventory offset for validity.", Config.IP);
            var valid = await GetIsPlayerInventoryValid(Config.INVENTORY_OFFSET, token).ConfigureAwait(false);
            if (!valid) {
                LogUtil.LogInfo($"Inventory read from {Config.INVENTORY_OFFSET} does not appear to be valid. Exiting!", Config.IP);
                return;
            }
            LogUtil.LogInfo("Successfully connected to bot. Starting main loop!", Config.IP);

            // RUN LOOP
            while (!token.IsCancellationRequested) {
                if (RetrieveDodo) {
                    await Task.Run(async () => {
                        await PlayerMove(1, 2_000, false, token);
                        await PlayerMove(3, 1_000, false, token);
                        await PlayerMove(1, 0_500, false, token);
                        await Connection.SendAsync(SwitchCommand.Hold(SwitchButton.L), token).ConfigureAwait(false);
                        await Task.Delay(0_500).ConfigureAwait(false);
                        // Initiate conversation
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.A), token).ConfigureAwait(false);
                        await Task.Delay(3_000, token).ConfigureAwait(false);
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.A), token).ConfigureAwait(false);
                        await Task.Delay(3_000, token).ConfigureAwait(false);

                        Console.WriteLine("First option selection - opting for visitors");
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.A), token).ConfigureAwait(false);
                        await Task.Delay(3_000, token).ConfigureAwait(false);
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.DDOWN), token).ConfigureAwait(false);
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                        Console.WriteLine("First option selection - opting for visitors");
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.A), token).ConfigureAwait(false);
                        await Task.Delay(3_000, token).ConfigureAwait(false);
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.A), token).ConfigureAwait(false);
                        await Task.Delay(3_000, token).ConfigureAwait(false);

                        Console.WriteLine("second option selection - opting for online play");
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.DDOWN), token).ConfigureAwait(false);
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.A), token).ConfigureAwait(false);
                        await Task.Delay(3_000, token).ConfigureAwait(false);
                        Console.WriteLine("third option selection - Ready!");
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.A), token).ConfigureAwait(false);
                        await Task.Delay(2_000, token).ConfigureAwait(false);
                        Console.WriteLine("Wait 20 seconds to connect, sometimes it can be slower than usual.");
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.A), token).ConfigureAwait(false);
                        await Task.Delay(16_000, token).ConfigureAwait(false);
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.A), token).ConfigureAwait(false);
                        await Task.Delay(1_500, token).ConfigureAwait(false);
                        Console.WriteLine("fourth option selection - Invite via dodo code!");
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.DUP), token).ConfigureAwait(false);
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.DUP), token).ConfigureAwait(false);
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.A), token).ConfigureAwait(false);
                        await Task.Delay(3_000, token).ConfigureAwait(false);
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.A), token).ConfigureAwait(false);
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.DUP), token).ConfigureAwait(false);
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.A), token).ConfigureAwait(false);
                        await Task.Delay(3_000, token).ConfigureAwait(false);
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.A), token).ConfigureAwait(false);
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.A), token).ConfigureAwait(false);
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                        await Connection.SendAsync(SwitchCommand.Click(SwitchButton.A), token).ConfigureAwait(false);
                        await Task.Delay(3_500, token).ConfigureAwait(false);
                        for (var i = 0; i < 14; i++) {
                            await Connection.SendAsync(SwitchCommand.Click(SwitchButton.B), token).ConfigureAwait(false);
                            await Task.Delay(0_500, token).ConfigureAwait(false);
                        }
                        await Connection.SendAsync(SwitchCommand.Release(SwitchButton.L), token).ConfigureAwait(false);
                        Console.WriteLine("This part should be AOK; now for the iffy stuff!?");
                        // GOT DODO
                        // RUN OUTSIDE NOW AND RESET YOURSELF
                        Console.WriteLine("Run left 1second");
                        await PlayerMove(2, 1_000, false, token);
                        Console.WriteLine("Run down and out the airport 2.5second");
                        await PlayerMove(0, 8_500, false, token);
                        Console.WriteLine("Loading 6 seconds...");
                        Console.WriteLine("Outside now?");
                        Console.WriteLine("Run left 1second");
                        await PlayerMove(2, 1_000, false, token);
                        Console.WriteLine("Run up 2second");
                        await PlayerMove(1, 1_800, false, token);
                        Console.WriteLine("face downwards these timers may be off or we may still be in the airport still! If not GG!");
                        await PlayerMove(0, 0_400, false, token);
                        // SEND DODO OUT
                        await GetDodoCode(token).ConfigureAwait(false);
                        Console.WriteLine("NEW DODO: " + DodoCode);
                        await SendMessage($"{Globals.Bot.Config.ISLAND_NAME} is now open!");
                        // CALL OUT DODO FOR STREAMERS IF ENABLED!
                        if (Config.TWITCH_ENABLED && Config.TWITCH_SHARE_DODO && !MuteTwitch) {
                            Globals.Twitch.SendMessage(Globals.TwitchChannel, $"{Globals.Bot.Config.ISLAND_NAME} is now open!");
                            Globals.Twitch.SendMessage(Globals.TwitchChannel, $"{Globals.Bot.Config.ISLAND_NAME} = {DodoCode}!");
                            Globals.Twitch.SendMessage(Globals.TwitchChannel, $"{Globals.Bot.Config.ISLAND_NAME} = {DodoCode}!");
                            Globals.Twitch.SendMessage(Globals.TwitchChannel, $"{Globals.Bot.Config.ISLAND_NAME} = {DodoCode}!");
                            Globals.Twitch.SendMessage(Globals.TwitchChannel, $"{Globals.Bot.Config.ISLAND_NAME} = {DodoCode}!");
                        }
                        RetrieveDodo = false;
                    }, token);
                } else {
                    if (ShowQueue) {
                        ShowQueue = false;
                        await ViewQueue().ConfigureAwait(false);
                    }
                    if (Delay >= 20) { // Maintenance Check (should be done on loading screen toggle state)
                        Delay = 0;
                        await GetUsersOnIsland(token).ConfigureAwait(false);
                        await GetDodoCode(token).ConfigureAwait(false);
                        if (LastUserCount - CountAvailable() > 1) {
                            Console.WriteLine("TWO OR MORE USERS LEFT STOP OPERATIONS!!!");
                            Console.WriteLine("TWO OR MORE USERS LEFT STOP OPERATIONS!!!");
                            Console.WriteLine("TWO OR MORE USERS LEFT STOP OPERATIONS!!!");
                            Console.WriteLine("--------------");
                            Console.WriteLine("RESET YOURSELF IF NECESSARY! :)");
                            RetrieveDodo = false;
                        }
                        LastUserCount = CountAvailable();
                    } else {
                        Delay++;
                    }
                    if (MovementActive) {
                        if (MovementType == 0) {
                            // Click
                            await PlayerClick(MovementButton, token).ConfigureAwait(false);
                        } else if (MovementType == 1) {
                            // Walk & Run
                            await PlayerMove(MovementDirection, MovementDuration, MovementState, token).ConfigureAwait(false);
                        } else {
                            // View Up/Down
                            await PlayerView(MovementState, token).ConfigureAwait(false);
                        }
                        // Cancel Movement
                        MovementActive = false;
                    }
                    if (PullValue) {
                        /* Testing Purposes when doing searches */
                        await GetOffset(token).ConfigureAwait(false);
                        PullValue = false;
                    }
                    // check commands now!
                    if (!Config.DISCORD_ACCEPTINGDROPS) {
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                    } else if (Injections.TryDequeue(out var item)) {
                        await DropItems(item, token).ConfigureAwait(false);
                        MovementActive = false;
                    } else {
                        MovementActive = false;
                        await Task.Delay(1_000, token).ConfigureAwait(false);
                    }
                }
            }
        }
        public async Task ChannelReady(DiscordSocketClient client) {
            Channel = new List<IMessageChannel>();
            foreach (ulong id in Config.DISCORD_QUEUECHANNEL) {
                if (client.GetChannel(id) is IMessageChannel channel) {
                    try { await channel.SendMessageAsync($"Bot is online who's ready!?").ConfigureAwait(false); } catch { Console.WriteLine("Queue channel required permissions you did not have!"); return; }
                    Channel.Add(channel);
                } else {
                    Console.WriteLine("Queue channel has not been selected or is invalid!");
                }
            }
        }
        private async Task<bool> GetIsPlayerInventoryValid(uint playerOfs, CancellationToken token) {
            var (ofs, len) = InventoryValidator.GetOffsetLength(playerOfs);
            var inventory = await Connection.ReadBytesAsync(ofs, len, token).ConfigureAwait(false);

            return InventoryValidator.ValidateItemBinary(inventory);
        }
        private async Task GetOffset(CancellationToken token) {
            Console.WriteLine("Provided Offset(in DEC): " + Config.STRING_SEARCH_OFFSET);
            var x = await Connection.ReadBytesAsync(Config.STRING_SEARCH_OFFSET, 0x0A, token).ConfigureAwait(false);
            foreach (byte i in x) {
                Console.Write("{0:X2} ", i);
            }
            string c = Encoding.UTF8.GetString(x);
            LogUtil.LogInfo($"Offset Test (Please Read): {c}).", Config.IP);
        }
        private async Task GetDodoCode(CancellationToken token) {
            var Search = await Connection.ReadBytesAsync(Config.DODO_OFFSET, 0x05, token).ConfigureAwait(false);
            DodoCode = Encoding.UTF8.GetString(Search);
            if (DodoCode != null && DodoCode.Length > 0) {
                if (Config.TWITCH_ENABLED && Config.TWITCH_SHARE_DODO && (DodoCode != LastDodo) && DodoCode != "No code set yet." && !MuteTwitch && SlotsAvailable()) {
                    Globals.Twitch.SendMessage(Globals.TwitchChannel, $"{Globals.Bot.Config.ISLAND_NAME} has {CountAvailable()} slot(s) open still! Dodo Code: {DodoCode}!");
                }

                if (Config.TWITCH_ENABLED && Config.TWITCH_SHARE_DODO && DodoCode != "No code set yet." && SlotsAvailable() && DelayTwitchDodoShare > 3 && !MuteTwitch) {
                    DelayTwitchDodoShare = 0;
                    Globals.Twitch.SendMessage(Globals.TwitchChannel, $"{Globals.Bot.Config.ISLAND_NAME} has {CountAvailable()} slot(s) open still! Dodo Code: {DodoCode}!");
                }
                if (DelayTwitchDodoShare > 7) DelayTwitchDodoShare = 0;
                DelayTwitchDodoShare++;
                LastDodo = DodoCode;
            }
        }
        private async Task GetUsersOnIsland(CancellationToken token) {
            bool canQueueView = false;
            for (int i = 0; i < 8; i++) {
                var Search = await Connection.ReadBytesAsync(Config.ONLINE_PLAYER_NAME_OFFSET[i], 0x0A, token).ConfigureAwait(false);
                var Name = Encoding.UTF8.GetString(Search).TrimEnd('\0');
                if (Name == null || Name == "7" || Name == "�7" || Name == " �7") break;
                if (OnlinePlayerList[i] != Name && !FirstRun) {
                    if (OnlinePlayerList[i] == "") {
                        OnlinePlayerList[i] = Name;
                        if (i != 0) {
                            canQueueView = true;
                            await SendMessage($"{OnlinePlayerList[i]} has joined {Globals.Bot.Config.ISLAND_NAME}.").ConfigureAwait(false);
                            if (Config.TWITCH_ENABLED && !MuteTwitch) {
                                Globals.Twitch.SendMessage(Globals.TwitchChannel, $"{OnlinePlayerList[i]} has joined {Globals.Bot.Config.ISLAND_NAME}.");
                                if (Config.TWITCH_SHARE_DODO && SlotsAvailable()) Globals.Twitch.SendMessage(Globals.TwitchChannel, $"{Globals.Bot.Config.ISLAND_NAME} is open! DODO CODE: {DodoCode}!");
                            }
                        }
                    } else {
                        if (Name == "") {
                            if (i != 0) {
                                canQueueView = true;
                                await SendMessage($"{OnlinePlayerList[i]} has left {Globals.Bot.Config.ISLAND_NAME}.").ConfigureAwait(false);
                                if (Config.TWITCH_ENABLED && !MuteTwitch) {
                                    Globals.Twitch.SendMessage(Globals.TwitchChannel, $"{OnlinePlayerList[i]} has left {Globals.Bot.Config.ISLAND_NAME}.");
                                    if (Config.TWITCH_SHARE_DODO && SlotsAvailable()) Globals.Twitch.SendMessage(Globals.TwitchChannel, $"{Globals.Bot.Config.ISLAND_NAME} is open! DODO CODE: {DodoCode}!");
                                }
                            }
                            OnlinePlayerList[i] = "";
                        } else {
                            canQueueView = true;
                            await SendMessage($"{OnlinePlayerList[i]} has left {Globals.Bot.Config.ISLAND_NAME}.\n{Name} has joined {Globals.Bot.Config.ISLAND_NAME}.").ConfigureAwait(false);
                            if (Config.TWITCH_ENABLED && !MuteTwitch) {
                                Globals.Twitch.SendMessage(Globals.TwitchChannel, $"{OnlinePlayerList[i]} has left and {Name} has joined {Globals.Bot.Config.ISLAND_NAME}.");
                                if (Config.TWITCH_SHARE_DODO && SlotsAvailable()) Globals.Twitch.SendMessage(Globals.TwitchChannel, $"{Globals.Bot.Config.ISLAND_NAME} is open! DODO CODE: {DodoCode}!");
                            }

                            OnlinePlayerList[i] = Name;
                        }
                    }
                    OnlinePlayerTime[i] = DateTime.Now;
                }

            }
            if (canQueueView) await ViewQueue().ConfigureAwait(false);
            FirstRun = false;
        }
        public bool SlotsAvailable() {
            if (OnlinePlayerList[0] == "") return false;
            for (int i = 1; i < 8; i++) if (OnlinePlayerList[i] == "") return true;
            return false;
        }
        public int CountAvailable() {
            var avail = 0;
            if (OnlinePlayerList[0] == "") return 0;
            for (int i = 0; i < 8; i++) {
                if (OnlinePlayerList[i] == "") avail++;
            }
            return avail;
        }
        public async Task ViewQueue() {
            string DiscordResp = $"**On The Island:**\n{Globals.Bot.Config.ISLAND_NAME}";
            if (OnlinePlayerList[0] == "") {
                DiscordResp = $"**Island:**\n{Globals.Bot.Config.ISLAND_NAME}```Offline```";
            } else {
                var slotsavailabe = 0;
                for (int i = 0; i < 8; i++) {
                    if (OnlinePlayerList[i] != "") TimeNowOnline = DateTime.Now - OnlinePlayerTime[i];
                    DiscordResp += (OnlinePlayerList[i] == "") ? $"```#{i + 1}: Available```" : $"```#{i + 1}: {OnlinePlayerList[i]}\n\nIsland time {String.Format((TimeNowOnline.Days > 0) ? TimeNowOnline.Days + " Day(s) " : "")}{String.Format((TimeNowOnline.Hours > 0) ? TimeNowOnline.Hours + " Hour(s) " : "")}{TimeNowOnline.Minutes} Minute(s).```";
                    if (OnlinePlayerList[i] == "") slotsavailabe++;
                }
            }
            await SendMessage($">>> {DiscordResp}").ConfigureAwait(false);
        }
        public bool IsQueueFull() {
            for (int i = 0; i < 8; i++) if (OnlinePlayerList[i] == "") return false;
            return true;
        }
        private async Task SendMessage(string Message) {
            foreach (IMessageChannel x in Channel) if (x != null) await x.SendMessageAsync($"{Message}").ConfigureAwait(false);
        }
        private async Task<int> DropItems(ItemRequest drop, CancellationToken token) {
            int dropped = 0;
            bool first = true;
            foreach (var item in drop.Items) {
                await DropItem(item, first, token).ConfigureAwait(false);
                first = false;
                dropped++;
            }

            await Click(SwitchButton.ZR, 1_000, token).ConfigureAwait(false);
            await Click(SwitchButton.A, 0_200, token).ConfigureAwait(false);
            await Click(SwitchButton.B, 0_600, token).ConfigureAwait(false);

            return dropped;
        }

        private async Task DropItem(Item item, bool first, CancellationToken token) {
            // Exit out of any menus.
            if (first) {
                for (int i = 0; i < 3; i++)
                    await Click(SwitchButton.B, 0_400, token).ConfigureAwait(false);
            }

            var itemName = GameInfo.Strings.GetItemName(item);
            LogUtil.LogInfo($"Injecting Item: {item.DisplayItemId:X4} ({itemName}).", Config.IP);
            // Inject item.
            await Connection.SendAsync(SwitchCommand.Poke(Config.INVENTORY_OFFSET, item.ToBytesClass()), token).ConfigureAwait(false);
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

        private const short XYMin = -30000;
        private const short XYMax = 30000;

        private async Task PlayerClick(SwitchButton Button, CancellationToken token) {
            await Connection.SendAsync(SwitchCommand.Click(Button), token).ConfigureAwait(false);
            await Task.Delay(0_300, token).ConfigureAwait(false);
        }
        private async Task PlayerView(bool UP, CancellationToken token) {
            await Connection.SendAsync(SwitchCommand.SetStick(SwitchStick.RIGHT, 0, ((UP) ? XYMax : XYMin)), token).ConfigureAwait(false);
            await Task.Delay(0_300, token).ConfigureAwait(false);
        }
        private async Task PlayerMove(int direction, int duration, bool run, CancellationToken token) {
            // Direction Check
            if (run) {
                await Connection.SendAsync(SwitchCommand.Hold(SwitchButton.B), token).ConfigureAwait(false);
                await Task.Delay(0_500, token).ConfigureAwait(false);
            }
            short y = (short)((direction == 1 || direction == 4 || direction == 5) ? XYMax : (direction == 0 || direction == 6 || direction == 7) ? XYMin : 0);
            short x = (short)((direction == 3 || direction == 5 || direction == 7) ? XYMax : (direction == 2 || direction == 4 || direction == 6) ? XYMin : 0);

            await Connection.SendAsync(SwitchCommand.SetStick(SwitchStick.LEFT, x, y), token).ConfigureAwait(false);
            await Task.Delay(duration, token).ConfigureAwait(false);
            if (run) {
                await Connection.SendAsync(SwitchCommand.Release(SwitchButton.B), token).ConfigureAwait(false);
                await Task.Delay(0_300, token).ConfigureAwait(false);
            }
            await Connection.SendAsync(SwitchCommand.SetStick(SwitchStick.LEFT, 0, 0), token).ConfigureAwait(false);
            await Task.Delay(0_300, token).ConfigureAwait(false);
        }
    }
}

using System;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace SysBot.AnimalCrossing {

    class TwitchBot {
        TwitchClient client;

        public TwitchBot(CrossBotConfig config) {
            ConnectionCredentials credentials = new ConnectionCredentials(config.TWITCH_USERNAME, config.TWITCH_TOKEN);
            var clientOptions = new ClientOptions {MessagesAllowedInPeriod = 750, ThrottlingPeriod = TimeSpan.FromSeconds(30)};
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);
            client.Initialize(credentials, config.TWITCH_CHANNEL);

#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate.
            client.OnLog += Client_OnLog;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate.
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate.
            client.OnJoinedChannel += Client_OnJoinedChannel;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate.
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate.
            client.OnConnected += Client_OnConnected;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate.

            client.Connect();

            Globals.Twitch = client;
        }

        private void Client_OnLog(object sender, OnLogArgs e) {
            Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }

        private void Client_OnConnected(object sender, OnConnectedArgs e) {
            Console.WriteLine($"Connected to {e.AutoJoinChannel}");
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e) {
            Console.WriteLine("Twitch bot is now in the channel!!");
            Globals.TwitchChannel = e.Channel;
            Globals.Twitch.SendMessage(Globals.TwitchChannel, "Twitch bot is now in the channel!!");
        }
    }
}
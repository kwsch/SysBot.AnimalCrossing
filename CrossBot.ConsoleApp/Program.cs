using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Threading;
using CrossBot.Discord;
using CrossBot.SysBot;
using SysBot.Base;

namespace CrossBot.ConsoleApp
{
    internal static class Program
    {
        private const string ConfigPath = "config.json";
        private const string DiscordPath = "discord.json";

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Starting up...");
            if (args.Length > 1)
                Console.WriteLine("This program does not support command line arguments.");

            if (!File.Exists(ConfigPath))
            {
                CreateConfigQuit();
                return;
            }

            var json = File.ReadAllText(ConfigPath);
            var cfgBot = JsonSerializer.Deserialize<BotConfig>(json);
            if (cfgBot == null)
            {
                Console.WriteLine("Failed to deserialize Bot configuration file.");
                WaitKeyExit();
                return;
            }

            var json_discord = File.ReadAllText(DiscordPath);
            var cfgDiscord = JsonSerializer.Deserialize<DiscordBotConfig>(json_discord);
            if (cfgDiscord == null)
            {
                Console.WriteLine("Failed to deserialize Discord configuration file.");
                WaitKeyExit();
                return;
            }

            SaveConfig(cfgBot, ConfigPath);
            SaveConfig(cfgDiscord, DiscordPath);

            // Set up logging for Console Window
            LogUtil.Forwarders.Add(Logger);
            static void Logger(string msg, string identity) => Console.WriteLine(GetMessage(msg, identity));
            static string GetMessage(string msg, string identity) => $"> [{DateTime.Now:hh:mm:ss}] - {identity}: {msg}";

            var cts = new CancellationTokenSource();
            var token = cts.Token;
            var bot = new Bot(cfgBot);
            var sys = new SysCord(bot, cfgDiscord);
            LogUtil.LogInfo("Starting Discord.", bot.Connection.IP);
#pragma warning disable 4014
            Task.Run(() => sys.MainAsync(cfgDiscord.Token, token), token);
#pragma warning restore 4014

            await BotRunner.RunFrom(bot, token).ConfigureAwait(false);
            WaitKeyExit();
            cts.Cancel();
        }

        private static void SaveConfig<T>(T config, string path)
        {
            var options = new JsonSerializerOptions {WriteIndented = true};
            var json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(path, json);
        }

        private static void CreateConfigQuit()
        {
            SaveConfig(new BotConfig { IP = "192.168.0.1", Port = 6000 }, ConfigPath);
            SaveConfig(new DiscordBotConfig(), DiscordPath);
            Console.WriteLine("Created blank config files. Please configure them and restart the program.");
            WaitKeyExit();
        }

        private static void WaitKeyExit()
        {
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using CrossBot.Discord;
using CrossBot.SysBot;
using SysBot.Base;

namespace CrossBot.ConsoleApp;

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

        var json = await File.ReadAllTextAsync(ConfigPath);
        var cfgBot = JsonSerializer.Deserialize<BotConfig>(json);
        if (cfgBot == null)
        {
            Console.WriteLine("Failed to deserialize Bot configuration file.");
            WaitKeyExit();
            return;
        }

        var json_discord = await File.ReadAllTextAsync(DiscordPath);
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
        if (!cfgDiscord.SkipDiscordBotCreation)
        {
            var sys = new SysCord(bot, cfgDiscord);
            bot.Log("Starting Discord.");
#pragma warning disable 4014
            Task.Run(() => sys.MainAsync(cfgDiscord.Token, cfgDiscord, token), token);
#pragma warning restore 4014
        }
        else
        {
            bot.Log("Skipped Discord setup per config settings.");
        }

        // Configure bot if settings require
        if (cfgBot.MaximumTransferSize > 0)
            bot.Connection.MaximumTransferSize = cfgBot.MaximumTransferSize;

        await BotRunner.RunFrom(bot, token).ConfigureAwait(false);
        WaitKeyExit();
        await cts.CancelAsync();
    }

    private static void SaveConfig<T>(T config, string path)
    {
#pragma warning disable CA1869 // Cache and reuse 'JsonSerializerOptions' instances
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            IgnoreReadOnlyProperties = true,
        };
#pragma warning restore CA1869 // Cache and reuse 'JsonSerializerOptions' instances
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
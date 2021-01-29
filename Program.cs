﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Threading;

namespace SysBot.AnimalCrossing
{
    internal static class Program
    {
        private const string ConfigPath = "config.json";

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
            var config = JsonSerializer.Deserialize<CrossBotConfig>(json);
            SaveConfig(config);

            await BotRunner.RunFrom(config, CancellationToken.None).ConfigureAwait(false);

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static void SaveConfig(CrossBotConfig config)
        {
            var options = new JsonSerializerOptions {WriteIndented = true};
            var json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(ConfigPath, json);
        }

        private static void CreateConfigQuit()
        {
            SaveConfig(new CrossBotConfig {IP = "192.168.0.1", Port = 6000});
            Console.WriteLine("Created blank config file. Please configure it and restart the program.");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}

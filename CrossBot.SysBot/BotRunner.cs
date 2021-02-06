using System;
using System.Threading;
using System.Threading.Tasks;
using SysBot.Base;

namespace CrossBot.SysBot
{
    /// <summary>
    /// Logic for creating a task that spins up the console bot handler task.
    /// </summary>
    public static class BotRunner
    {
        public static async Task RunFrom(Bot bot, CancellationToken cancel)
        {
            if (bot.Config.SkipConsoleBotCreation) // testing Discord only?
            {
                // Don't interact with the bot; this branch is only triggered if the testing bool is set.
                await Task.Delay(-1, cancel).ConfigureAwait(false);
                return;
            }

            bot.Log("Starting bot loop.");
            int faultCount = 0;
            while (!cancel.IsCancellationRequested)
            {
                if (faultCount != 0)
                {
                    await Task.Delay(5_000, cancel).ConfigureAwait(false);
                    bot.DropState.CleanRequested = true;
                    bot.Log("Restarting bot loop.");
                }

                // Run the bot until the bot task finishes. Depending on the result, we may restart.
                var start = DateTime.Now;
                var task = bot.RunAsync(cancel);
                await task.ConfigureAwait(false);

                if (!task.IsFaulted)
                    break; // Not a crash. We're done here.

                LogError(bot.Connection, task);

                var cfg = bot.Config;
                if (!cfg.RestartOnCrash)
                    break; // we're done

                var uptime = DateTime.Now - start;
                if (uptime > TimeSpan.FromSeconds(cfg.UptimeThreshold))
                    faultCount = 0; // Seems like it was running for a fair amount of time before crashing. Reset the fault counter.

                if (++faultCount <= cfg.MaximumRestarts)
                    continue; // restart loop!

                bot.Connection.LogError("Bot has crashed too many times in the current time-window. Stopping execution.");
                break; // we're done
            }

            bot.Log("Bot execution has terminated.");
        }

        private static void LogError(IConsoleConnection connection, Task task)
        {
            if (task.Exception == null)
            {
                connection.LogError("Bot has crashed due to an unknown error.");
                return;
            }

            connection.LogError("Bot has crashed due to an error:");
            foreach (var ex in task.Exception.InnerExceptions)
            {
                connection.LogError(ex.Message);
                var st = ex.StackTrace;
                if (st != null)
                    connection.LogError(st);
            }
        }
    }
}

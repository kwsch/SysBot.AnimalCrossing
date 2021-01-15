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

            LogUtil.LogInfo("Starting bot loop.", bot.Connection.IP);
            int faultCount = 0;
            while (!cancel.IsCancellationRequested)
            {
                if (faultCount != 0)
                {
                    await Task.Delay(5_000, cancel).ConfigureAwait(false);
                    bot.CleanRequested = true;
                    LogUtil.LogInfo("Restarting bot loop.", bot.Connection.IP);
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

                LogUtil.LogError("Bot has crashed too many times in the current time-window. Stopping execution.", bot.Connection.IP);
                break; // we're done
            }

            LogUtil.LogInfo("Bot execution has terminated.", bot.Connection.IP);
        }

        private static void LogError(SwitchConnectionBase connection, Task task)
        {
            if (task.Exception == null)
            {
                LogUtil.LogError("Bot has crashed due to an unknown error.", connection.IP);
                return;
            }

            LogUtil.LogError("Bot has crashed due to an error:", connection.IP);
            foreach (var ex in task.Exception.InnerExceptions)
            {
                LogUtil.LogError(ex.Message, connection.IP);
                var st = ex.StackTrace;
                if (st != null)
                    LogUtil.LogError(st, connection.IP);
            }
        }
    }
}

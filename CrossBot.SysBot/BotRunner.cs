using System.Threading;
using System.Threading.Tasks;
using SysBot.Base;

namespace CrossBot.SysBot
{
    public static class BotRunner
    {
        public static async Task RunFrom(Bot bot, CancellationToken cancel)
        {
            if (bot.Config.SkipConsoleBotCreation)
            {
                await Task.Delay(-1, cancel).ConfigureAwait(false);
                return;
            }

            LogUtil.LogInfo("Starting bot loop.", bot.Connection.IP);

            var task = bot.RunAsync(cancel);
            await task.ConfigureAwait(false);

            if (task.IsFaulted)
            {
                if (task.Exception == null)
                {
                    LogUtil.LogError("Bot has terminated due to an unknown error.", bot.Connection.IP);
                }
                else
                {
                    LogUtil.LogError("Bot has terminated due to an error:", bot.Connection.IP);
                    foreach (var ex in task.Exception.InnerExceptions)
                    {
                        LogUtil.LogError(ex.Message, bot.Connection.IP);
                        var st = ex.StackTrace;
                        if (st != null)
                            LogUtil.LogError(st, bot.Connection.IP);
                    }
                }
            }
            else
            {
                LogUtil.LogInfo("Bot has terminated.", bot.Connection.IP);
            }
        }
    }
}

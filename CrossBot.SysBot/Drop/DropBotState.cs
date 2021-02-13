using System.Collections.Concurrent;
using CrossBot.Core;

namespace CrossBot.SysBot
{
    /// <summary>
    /// Tracks the state of the Drop Bot
    /// </summary>
    public class DropBotState
    {
        public readonly ConcurrentQueue<DropRequest> Injections = new();
        public DropBotState(DropBotConfig cfg) => Config = cfg;

        public readonly DropBotConfig Config;
        private int DropCount;
        private int IdleCount;

        public bool CleanRequested;
        public bool ValidateRequested;

        public bool CleanRequired => DropCount != 0 && IdleCount > Config.NoActivitySeconds;

        public void AfterDrop(DropRequest dropRequest, int count)
        {
            dropRequest.NotifyFinished();
            DropCount += count;
            IdleCount = 0;
        }

        public void AfterClean()
        {
            DropCount = 0;
            IdleCount = 0;
            CleanRequested = false;
        }

        public void StillIdle()
        {
            IdleCount++;
        }
    }
}

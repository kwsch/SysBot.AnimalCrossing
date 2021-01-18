using System.Collections.Concurrent;
using CrossBot.Core;

namespace CrossBot.SysBot
{
    /// <summary>
    /// Tracks the state of the Drop Bot
    /// </summary>
    public class DropBotState
    {
        public readonly ConcurrentQueue<ItemRequest> Injections = new();
        public DropBotState(DropBotConfig cfg) => Config = cfg;

        public readonly DropBotConfig Config;
        private int DropCount;
        private int IdleCount;

        public bool CleanRequired => DropCount != 0 && IdleCount > Config.NoActivitySeconds;

        public void AfterDrop(int count)
        {
            DropCount += count;
            IdleCount = 0;
        }

        public void AfterClean()
        {
            DropCount = 0;
            IdleCount = 0;
        }

        public void StillIdle()
        {
            IdleCount++;
        }
    }
}

using System;
using System.Linq;
using System.Threading;

namespace CrossBot.Core
{
    /// <summary>
    /// Circular Buffer to track when a Slot was last injected at. Has a check to ensure that any slot is not overwritten until it is stale.
    /// </summary>
    public class SlotTracker
    {
        private int NextIndex = -1;
        private readonly DateTime[] RevisedAt;

        public SlotTracker(int count)
        {
            var defaultTime = DateTime.UtcNow.Subtract(TimeSpan.FromDays(100));
            var revised = new DateTime[count];
            for (int i = 0; i < revised.Length; i++)
                revised[i] = defaultTime;
            RevisedAt = revised;
        }

        public bool CanAdd(ISlotSetting setting)
        {
            var time = DateTime.UtcNow;
            return RevisedAt.Any(z => (time - z).Seconds > setting.StaleSeconds);
        }

        public int Add()
        {
            var injectAt = Interlocked.Increment(ref NextIndex);
            RevisedAt[injectAt] = DateTime.UtcNow;
            return injectAt;
        }
    }

    public interface ISlotSetting
    {
        int StaleSeconds { get; }
    }
}

using System;
using CrossBot.Core;
using FluentAssertions;
using Xunit;

namespace CrossBot.Tests
{
    public class TimingTests
    {
        private class FakeTimeSettings : ISlotSetting
        {
            public int StaleSeconds { get; init; } = 100;
        }

        [Theory]
        [InlineData(10, 10, 100)]
        [InlineData(10, 100, 30)]
        public void InjectSlots(int capacity, int count, int stale)
        {
            var tracker = new SlotTracker(capacity);
            var settings = new FakeTimeSettings {StaleSeconds = stale};
            var time = DateTime.UtcNow;
            for (int i = 1; i <= Math.Min(capacity, count); i++)
            {
                tracker.CanAdd(settings, time).Should().BeTrue();
                tracker.Add(time);
            }

            count -= capacity;

            while (count > 0)
            {
                var amount = Math.Min(capacity, count);
                for (int i = 1; i <= amount; i++)
                {
                    tracker.CanAdd(settings, time).Should().BeFalse();
                }

                time = time.AddSeconds(stale + 1);
                for (int i = 1; i <= amount; i++)
                {
                    tracker.CanAdd(settings, time).Should().BeTrue();
                    tracker.Add(time);
                }
                count -= capacity;
            }
        }
    }
}

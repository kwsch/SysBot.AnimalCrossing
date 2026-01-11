using System;
using System.Linq;
using System.Threading;

namespace CrossBot.Core;

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

    public bool CanAdd(ISlotSetting setting) => CanAdd(setting, DateTime.UtcNow);
    public int Add() => Add(DateTime.UtcNow);

    public bool CanAdd(ISlotSetting setting, DateTime time)
    {
        return RevisedAt.Any(z => (time - z).TotalSeconds > setting.StaleSeconds);
    }

    public int Add(DateTime time)
    {
        var injectAt = Interlocked.Increment(ref NextIndex) % RevisedAt.Length;
        RevisedAt[injectAt] = time;
        return injectAt;
    }
}

public interface ISlotSetting
{
    int StaleSeconds { get; }
}
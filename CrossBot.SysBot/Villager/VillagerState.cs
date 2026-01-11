using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CrossBot.Core;
using NHSE.Core;
using NHSE.Villagers;
using SysBot.Base;

namespace CrossBot.SysBot;

public sealed class VillagerState(SwitchRoutineExecutor<BotConfig> executor, VillagerConfig config)
{
    public readonly SlotTracker Tracker = new(config.VillagerCount);
    public readonly ConcurrentQueue<VillagerRequest> Injections = new();
    public readonly VillagerInfo?[] Existing = new VillagerInfo[10];

    public async Task InitializeVillagers(CancellationToken token)
    {
        executor.Log("Reading all villager slots from RAM. This may take some time depending on the maximum transfer setting.");
        for (int index = 0; index < 10; index++)
        {
            var vOfs = Offsets.GetVillagerOffset(index);
            var hOfs = Offsets.GetVillagerHouseOffset(index);

            var v = await executor.Connection.ReadBytesAsync(vOfs, Villager2.SIZE, token).ConfigureAwait(false);
            var h = await executor.Connection.ReadBytesAsync(hOfs, VillagerHouse2.SIZE, token).ConfigureAwait(false);
            executor.Log($"Villager {index+1}/10 done.");
            Existing[index] = new VillagerInfo(new Villager2(v), new VillagerHouse2(h));
        }
    }

    public async Task InjectVillager(VillagerRequest data, CancellationToken token)
    {
        var index = data.Index + config.MinVillagerIndex;
        data.Index = index;
        foreach (var v in data.Items)
        {
            await InjectVillager(v, index, token).ConfigureAwait(false);
            if (++index > config.MaxVillagerIndex)
                index = config.MinVillagerIndex;
        }

        data.Injected = true;
        data.NotifyFinished();
    }

    private async Task InjectVillager(VillagerData data, int index, CancellationToken token)
    {
        var villager = data.Villager;
        var house = data.House;

        var vOfs = Offsets.GetVillagerOffset(index);
        var hOfs = Offsets.GetVillagerHouseOffset(index);

        await executor.Connection.WriteBytesAsync(villager.ToArray(), vOfs, token).ConfigureAwait(false);
        await executor.Connection.WriteBytesAsync(house.ToArray(), hOfs, token).ConfigureAwait(false);
    }
}
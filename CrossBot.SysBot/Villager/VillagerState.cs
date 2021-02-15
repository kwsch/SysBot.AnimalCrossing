using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CrossBot.Core;
using NHSE.Core;
using NHSE.Villagers;
using SysBot.Base;

namespace CrossBot.SysBot
{
    public sealed class VillagerState
    {
        private readonly SwitchRoutineExecutor<BotConfig> Executor;
        private readonly VillagerConfig Config;

        public readonly SlotTracker Tracker;
        public readonly ConcurrentQueue<VillagerRequest> Injections = new();
        public readonly VillagerInfo?[] Existing = new VillagerInfo[10];

        public VillagerState(SwitchRoutineExecutor<BotConfig> executor, VillagerConfig config)
        {
            Executor = executor;
            Config = config;
            Tracker = new SlotTracker(config.VillagerCount);
        }

        public async Task InitializeVillagers(CancellationToken token)
        {
            for (int index = 0; index < 10; index++)
            {
                var vOfs = Offsets.GetVillagerOffset(index);
                var hOfs = Offsets.GetVillagerHouseOffset(index);

                var v = await Executor.Connection.ReadBytesAsync(vOfs, Villager2.SIZE, token).ConfigureAwait(false);
                var h = await Executor.Connection.ReadBytesAsync(hOfs, VillagerHouse.SIZE, token).ConfigureAwait(false);
                Existing[index] = new VillagerInfo(new Villager2(v), new VillagerHouse(h));
            }
        }

        public async Task InjectVillager(VillagerRequest data, CancellationToken token)
        {
            var index = data.Index + Config.MinVillagerIndex;
            data.Index = index;
            foreach (var v in data.Items)
            {
                await InjectVillager(v, index, token).ConfigureAwait(false);
                if (++index > Config.MaxVillagerIndex)
                    index = Config.MinVillagerIndex;
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

            await Executor.Connection.WriteBytesAsync(villager, vOfs, token).ConfigureAwait(false);
            await Executor.Connection.WriteBytesAsync(house, hOfs, token).ConfigureAwait(false);
        }
    }
}

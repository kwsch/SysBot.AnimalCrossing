using System;
using System.Collections.Concurrent;
using System.IO;
using CrossBot.Core;
using NHSE.Core;
using SysBot.Base;

namespace CrossBot.SysBot
{
    public class FieldItemState
    {
        public readonly ConcurrentQueue<SpawnRequest> Injections = new();
        public readonly FieldItemConfig Config;
        private DateTime FieldItemInjectedTime = DateTime.MinValue;
        public byte[] FieldItemLayer = [];

        public void ForceReload() => FieldItemInjectedTime = DateTime.MinValue;

        public FieldItemState(FieldItemConfig cfg)
        {
            Config = cfg;
            LoadFieldItemLayer(cfg.FieldItemLayerPath);
            X = cfg.SpawnMinX;
            Y = cfg.SpawnMinY;

            if ((X & 1) == 1)
                X++;
            if ((Y & 1) == 1)
                Y++;
        }

        public bool LoadFieldItemLayer(string path)
        {
            if (!File.Exists(path))
            {
                LogUtil.LogError($"Unable to load: Requested path does not exist: {path}.", nameof(FieldItemLayer));
                return false;
            }
            FieldItemLayer = File.ReadAllBytes(path);
            LogUtil.LogInfo($"Loaded field item layer (0x{FieldItemLayer.Length:X} bytes) from: {path}.", nameof(FieldItemLayer));
            return true;
        }

        public bool FullRefreshRequired => FieldItemLayer.Length != 0 && Config.InjectFieldItemRequest && DateTime.Now - FieldItemInjectedTime > TimeSpan.FromMinutes(Config.FullRefreshCooldownMinutes);

        public void AfterFullRefresh()
        {
            FieldItemInjectedTime = DateTime.Now;
        }

        private int X;
        private int Y;

        public (int x, int y) GetNextInjectCoordinates(int count, int height)
        {
            var width = count / height;

            var (x, y) = (X, Y);
            AdvanceCoordinates(width);

            // Might have overlapped the boundary. Check before returning.
            var cfg = Config;
            bool canInject = FieldItemDropper.CanFitDropped(x, y, count, height, cfg.SpawnMinX, cfg.SpawnMaxX, cfg.SpawnMinY, cfg.SpawnMaxY);
            if (canInject)
                return (x, y);

            (x, y) = (X, Y);
            AdvanceCoordinates(width);

            // Might be overlapping still on the Y boundary. If so, reset to initial.
            canInject = FieldItemDropper.CanFitDropped(x, y, count, height, cfg.SpawnMinX, cfg.SpawnMaxX, cfg.SpawnMinY, cfg.SpawnMaxY);
            if (canInject)
                return (x, y);

            X = Config.SpawnMinX;
            Y = Config.SpawnMinY;
            return (X, Y);
        }

        private void AdvanceCoordinates(int width)
        {
            X += Math.Max(Config.SpawnSpacingX, width + 1) * 2;
            if (X <= NHSE.Core.FieldItemLayer.FieldItemWidth - Config.SpawnMaxX)
                return;

            X = Config.SpawnMinX;
            Y += Config.SpawnSpacingY * 2;
            if (Y <= NHSE.Core.FieldItemLayer.FieldItemHeight - Config.SpawnMaxY)
                return;

            Y = Config.SpawnMinY;
        }

        public void AfterSpawn(SpawnRequest itemSet)
        {
            itemSet.NotifyFinished();
        }
    }
}

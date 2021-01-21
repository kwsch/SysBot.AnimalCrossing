using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using NHSE.Core;

namespace CrossBot.SysBot
{
    public class FieldItemState
    {
        public readonly ConcurrentQueue<IReadOnlyList<FieldItemColumn>> Injections = new();
        public readonly FieldItemConfig Config;
        private DateTime FieldItemInjectedTime = DateTime.MinValue;
        public byte[] FieldItemLayer = Array.Empty<byte>();

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
                return false;
            FieldItemLayer = File.ReadAllBytes(path);
            return true;
        }

        public bool FullRefreshRequired => FieldItemLayer.Length != 0 && Config.InjectFieldItemRequest && FieldItemInjectedTime - DateTime.Now > TimeSpan.FromMinutes(Config.FullRefreshCooldownMinutes);

        public void AfterFullRefresh()
        {
            FieldItemInjectedTime = DateTime.Now;
        }

        private int X;
        private int Y;

        public (int x, int y) GetNextInjectCoordinates(int count, int height)
        {
            var (x, y) = (X & ~1, Y & ~1);
            var width = count / height;
            AdvanceCoordinates(width);
            var cfg = Config;

            // Might have overlapped the boundary. Check before returning.
            bool canInject = FieldItemDropper.CanFitDropped(x, y, count, height, cfg.SpawnMinX, cfg.SpawnMaxX, cfg.SpawnMinY, cfg.SpawnMaxY);
            if (!canInject)
            {
                (x, y) = (X & ~1, Y & ~1);
                AdvanceCoordinates(width);
            }
            return (x, y);
        }

        private void AdvanceCoordinates(int width)
        {
            X += Math.Max(Config.SpawnSpacingX, (width * 2) + 2);
            if (X <= NHSE.Core.FieldItemLayer.FieldItemWidth - Config.SpawnMaxX)
                return;

            X = Config.SpawnMinX;
            Y += Config.SpawnSpacingY;
            if (Y <= NHSE.Core.FieldItemLayer.FieldItemHeight - Config.SpawnMaxY)
                return;

            Y = Config.SpawnMinY;
        }
    }
}

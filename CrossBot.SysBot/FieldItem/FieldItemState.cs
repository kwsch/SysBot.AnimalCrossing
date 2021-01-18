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

        public FieldItemState(FieldItemConfig cfg)
        {
            Config = cfg;
            LoadFieldItemLayer(cfg);
            X = cfg.SpawnMinX;
            Y = cfg.SpawnMinY;
        }

        private bool LoadFieldItemLayer(FieldItemConfig cfg)
        {
            if (!File.Exists(cfg.FieldItemLayerPath))
                return false;
            FieldItemLayer = File.ReadAllBytes(cfg.FieldItemLayerPath);
            return true;
        }

        public bool FullRefreshRequired => Config.InjectFieldItem && FieldItemInjectedTime - DateTime.Now > TimeSpan.FromMinutes(Config.FullRefreshCooldownMinutes);

        public void AfterFullRefresh()
        {
            FieldItemInjectedTime = DateTime.Now;
        }

        private int X;
        private int Y;

        public (int, int) GetNextInjectCoordinates(int count, int height)
        {
            var result = (X & ~1, Y & ~1);
            var width = count / height;
            X += Math.Max(Config.SpawnSpacingX, (width * 2) + 2);
            if (X > Config.SpawnMaxX)
            {
                X = Config.SpawnMinX;
                Y += Config.SpawnSpacingY;
                if (Y > Config.SpawnMaxY)
                    Y = Config.SpawnMinY;
            }
            return result;
        }
    }
}

using System;
using NHSE.Core;
using static NHSE.Core.FieldItemLayer;

namespace CrossBot.SysBot
{
    public class FieldItemConfig
    {
        public string FieldItemLayerPath { get; set; } = string.Empty;

        public bool InjectFieldItem { get; set; } = false;
        public double FullRefreshCooldownMinutes { get; set; } = 10d;
        public uint FieldItemOffset { get; set; }

        public bool RequireJoin { get; set; } = true;

        public int MaxSpawnCount { get; set; } = 40;

        public int SpawnSpacingX { get; set; } = 16;
        public int SpawnSpacingY { get; set; } = 16;

        public int SpawnMaxHeight { get; set; } = 8;

        public int SpawnMinX { get; set; } = (int)(TilesPerAcreDim * 1.1); // 0
        public int SpawnMinY { get; set; } = (int)(TilesPerAcreDim * 2.5); // 0

        public int SpawnMaxX { get; set; } = (int)(TilesPerAcreDim * 5.9); // 7
        public int SpawnMaxY { get; set; } = (int)(TilesPerAcreDim * 5.5); // 6

        public FieldItemSpawnMode Mode { get; set; }

        public int GetSpawnHeight(int count) => GetSpawnHeight(count, Mode);

        private int GetSpawnHeight(int count, FieldItemSpawnMode mode) => mode switch
        {
            FieldItemSpawnMode.MaxHeight => SpawnMaxHeight,
            FieldItemSpawnMode.Square => (int) Math.Sqrt(count),
            FieldItemSpawnMode.VerticalLine => count,
            _ => throw new ArgumentOutOfRangeException(nameof(mode)),
        };

        public bool IsCoordinatesValid()
        {
            if (SpawnMinX < 0 || SpawnMinX > SpawnMaxX)
                return false;
            if (SpawnMinY < 0 || SpawnMinY > SpawnMaxY)
                return false;
            if (SpawnMaxX >= FieldItemWidth)
                return false;
            if (SpawnMaxY >= FieldItemHeight)
                return false;
            return true;
        }
    }

    public enum FieldItemSpawnMode
    {
        MaxHeight,
        Square,
        VerticalLine,
    }
}
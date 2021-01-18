using System;
using static NHSE.Core.FieldItemLayer;

namespace CrossBot.SysBot
{
    [Serializable]
    public class FieldItemConfig
    {
        /// <summary> Offset the Field Item Layer 1 starts at. Shouldn't be zero. </summary>
        public uint FieldItemOffset { get; set; }

        #region Layer

        /// <summary> Path to load the Field Item layer that is automatically refreshed. </summary>
        public string FieldItemLayerPath { get; set; } = string.Empty;

        /// <summary> How often to reload the item layer. </summary>
        public double FullRefreshCooldownMinutes { get; set; } = 10d;

        /// <summary> Toggle to determine if the layer should be periodically refreshed or not at all. </summary>
        public bool InjectFieldItemLayer { get; set; }

        #endregion

        #region Spawn

        /// <summary> Toggle to determine if field item spawn requests should be accepted. </summary>
        public bool InjectFieldItemRequest { get; set; }

        /// <summary> Max amount of Items to spawn in at a time. </summary>
        public int MaxSpawnCount { get; set; } = 40;

        /// <summary> Next injection will be X (double-tile) item coordinates away from the previous spawn root. </summary>
        public int SpawnSpacingX { get; set; } = 16;
        /// <summary> Next injection will be Y (double-tile) item coordinates away from the previous spawn root minimum. Once a row is injected, it shifts down to the next row. </summary>
        public int SpawnSpacingY { get; set; } = 16;

        /// <summary> Minimum X coordinate to inject at. </summary>
        public int SpawnMinX { get; set; } = (int)(TilesPerAcreDim * 1.1); // 0
        /// <summary> Minimum Y coordinate to inject at. </summary>
        public int SpawnMinY { get; set; } = (int)(TilesPerAcreDim * 2.5); // 0

        /// <summary> Maximum X Coordinate to inject at. </summary>
        public int SpawnMaxX { get; set; } = (int)(TilesPerAcreDim * 5.9); // 7
        /// <summary> Maximum Y Coordinate to inject at. </summary>
        public int SpawnMaxY { get; set; } = (int)(TilesPerAcreDim * 5.5); // 6

        /// <summary> Max amount of items to inject in a vertical line. The horizontal width is calculated. </summary>
        public int SpawnMaxHeight { get; set; } = 8;

        /// <summary> Injection Mode pattern to spawn items. </summary>
        public FieldItemSpawnMode Mode { get; set; }

        #endregion

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
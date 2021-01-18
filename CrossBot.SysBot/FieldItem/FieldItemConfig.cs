using System;
using static NHSE.Core.FieldItemLayer;
using static CrossBot.SysBot.CoordinateResult;

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

        /// <summary> Max amount of items to inject in a vertical line. The horizontal width is calculated. </summary>
        public int SpawnMaxHeight { get; set; } = 8;

        /// <summary> Next injection will be a minimum of X (double-tile) item coordinates away from the previous spawn root. </summary>
        public int SpawnSpacingX { get; set; } = 1;
        /// <summary> Next injection will be a minimum of Y (double-tile) item coordinates away from the previous spawn root minimum. Once a row is injected, it shifts down to the next row. </summary>
        public int SpawnSpacingY { get; set; } = 8;

        /// <summary> Amount of tiles to not drop at the left side of the map.</summary>
        public int SpawnMinX { get; set; } = (int)(TilesPerAcreDim * 1.1) & ~1;
        /// <summary> Amount of tiles to not drop at the top side of the map.</summary>
        public int SpawnMinY { get; set; } = (int)(TilesPerAcreDim * 2.5) & ~1;

        /// <summary> Amount of tiles to not drop at the right side of the map.</summary>
        public int SpawnMaxX { get; set; } = (int)(TilesPerAcreDim * 0.9) & ~1;
        /// <summary> Amount of tiles to not drop at the bottom side of the map.</summary>
        public int SpawnMaxY { get; set; } = (int)(TilesPerAcreDim * 1.5) & ~1;

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

        public CoordinateResult ValidateCoordinates()
        {
            if (SpawnMinX < 0 || SpawnMaxX < 0)
                return NegativeX;
            if (SpawnMinY < 0 || SpawnMaxY < 0)
                return NegativeY;
            if (SpawnMinX + SpawnMaxX >= FieldItemWidth)
                return LargeX;
            if (SpawnMinY + SpawnMaxY >= FieldItemHeight)
                return LargeY;
            if (SpawnMaxHeight > SpawnSpacingY)
                return SpacingGapTooSmall;
            return Valid;
        }
    }

    public enum CoordinateResult
    {
        Valid,
        NegativeX,
        NegativeY,
        LargeX,
        LargeY,
        SpacingGapTooSmall,
    }

    public enum FieldItemSpawnMode
    {
        MaxHeight,
        Square,
        VerticalLine,
    }
}
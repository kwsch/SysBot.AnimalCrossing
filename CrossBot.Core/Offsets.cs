using NHSE.Core;

namespace CrossBot.Core
{
    /// <summary>
    /// Offsets for game RAM locations based on the latest patch revision.
    /// </summary>
    public static class Offsets
    {
        // Helpers
        public const uint PlayerSize = 0x11B968;

        public const uint InventoryOffset = 0xAFB1E6E0; // player 0 (A)
        public const string CoordinatePointer = "[[[[main+4627088]+18]+178]+D0]+DA";

        // Main player offsets functions
        public static uint GetPlayerInventoryOffset(uint profiles) => InventoryOffset + (PlayerSize * (profiles - 1));

        // Main save offsets
        public const uint TurnipAddress = 0xAEA140F4;
        public const uint VillagerAddress = TurnipAddress - 0x2D40 - 0x45B50C + 0x10;
        public const uint VillagerHouseAddress = TurnipAddress - 0x2D40 - 0x45B50C + 0x44f7FC;

        public static uint GetVillagerOffset(int index) => VillagerAddress + (uint)(Villager2.SIZE * index);
        public static uint GetVillagerHouseOffset(int index) => VillagerHouseAddress + (uint)(VillagerHouse2.SIZE * index);

        public const uint FieldItemStart = VillagerAddress - 0x10 + 0x22F3F0;

        public const uint DodoAddress = 0xABE015C;
        public const uint OnlineSessionAddress = 0x945F740;
    }
}

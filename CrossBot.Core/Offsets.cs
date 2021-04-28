using NHSE.Core;

namespace CrossBot.Core
{
    /// <summary>
    /// Offsets for game RAM locations based on the latest patch revision.
    /// </summary>
    public static class Offsets
    {
        // Helpers
        public const uint PlayerSize = 0x10E500;

        public const uint InventoryOffset = 0xAE61F840; // player 0 (A)
        public const string CoordinatePointer = "[[[[main+3A08B40]+18]+178]+D0]+DA";

        // Main player offsets functions
        public static uint GetPlayerInventoryOffset(uint profiles) => (InventoryOffset + (PlayerSize * (profiles - 1)));

        // Main save offsets
        public const uint TurnipAddress = 0xAD618B84;
        public const uint VillagerAddress = TurnipAddress - 0x2d40 - 0x43be2c + 0x10 - 0x90;
        public const uint VillagerHouseAddress = TurnipAddress - 0x2d40 - 0x43be2c + 0x43abd4 - 0x90;

        public static uint GetVillagerOffset(int index) => VillagerAddress + (uint)(Villager2.SIZE * index);
        public static uint GetVillagerHouseOffset(int index) => VillagerHouseAddress + (uint)(Villager2.SIZE * index);

        public const uint FieldItemStart = VillagerAddress - 0x10 + 0x22e1b8;

        // Other Addresses
        public const uint ArriverNameLocAddress = 0xB66D6FF8;
        public const ulong ArriverVillageLocAddress = ArriverNameLocAddress - 0x1C;
        public const uint TextSpeedAddress = 0xBACE3B8;
        public const uint DodoAddress = 0xA98F15C;
        public const uint OnlineSessionAddress = 0x920E740;
        public const ulong OnlineSessionVisitorAddress = 0x9F974B8;
        public const ulong OnlineSessionVisitorSize = 0x448; // reverse order
    }
}

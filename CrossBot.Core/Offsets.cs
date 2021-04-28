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
        public const uint TurnipAddress = 0xAD195B74;
        public const uint VillagerAddress = TurnipAddress - 0x2cb0 - 0x43be1c + 0x10 - 0x90;
        public const uint VillagerHouseAddress = TurnipAddress - 0x2cb0 - 0x43be1c + 0x43abd4 - 0x90;

        public static uint GetVillagerOffset(int index) => VillagerAddress + (uint)(Villager2.SIZE * index);
        public static uint GetVillagerHouseOffset(int index) => VillagerHouseAddress + (uint)(Villager2.SIZE * index);

        public const uint FieldItemStart = VillagerAddress - 0x10 + 0x22e1a8;

        // Other Addresses
        public const uint ArriverNameLocAddress = 0xB6351EA0;
        public const ulong ArriverVillageLocAddress = ArriverNameLocAddress - 0x1C;
        public const uint TextSpeedAddress = 0xBA88BC8;
        public const uint DodoAddress = 0xA98D15C;
        public const uint OnlineSessionAddress = 0x920C740;
        public const ulong OnlineSessionVisitorAddress = 0x9D3BFB0;
        public const ulong OnlineSessionVisitorSize = 0x448; // reverse order
    }
}

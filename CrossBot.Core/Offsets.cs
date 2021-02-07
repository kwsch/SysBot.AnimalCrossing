using NHSE.Core;

namespace CrossBot.Core
{
    /// <summary>
    /// Offsets for game RAM locations based on the latest patch revision.
    /// </summary>
    public static class Offsets
    {
        // Helpers
        public const uint PlayerSize = 0x10E3A8;
        public const uint PlayerOtherStartPadding = 0x36A50;

        public const uint InventoryOffset = 0xACDAD530; // player 0 (A)
        public const string CoordinatePointer = "[[[[main+37D6A40]+18]+178]+D0]+DA";

        // Main player offsets functions
        private static uint GetPlayerStart(uint inventoryOffset) => inventoryOffset - 0x10 - PlayerOtherStartPadding + 0x110;
        public static uint GetPlayerIdAddress(uint inventoryOffset) => GetPlayerStart(inventoryOffset) + 0xAFA8;
        public static uint GetPlayerProfileMainAddress(uint inventoryOffset) => GetPlayerStart(inventoryOffset) + 0x116A0;
        public static uint GetManpu(uint inventoryOffset) => inventoryOffset - 0x10 + 0xAF7C + 72;

        public static uint GetPlayerInventoryOffset(uint profiles) => (InventoryOffset + (PlayerSize * (profiles - 1)));

        // Main save offsets
        public const uint TurnipAddress = 0xABE151EC;
        public const uint VillagerAddress = TurnipAddress - 0x2cb0 - 0x41887c + 0x10;
        public const uint VillagerHouseAddress = TurnipAddress - 0x2cb0 - 0x41887c + 0x417634;

        public static uint GetVillagerOffset(int index) => VillagerAddress + (uint)(Villager2.SIZE * index);
        public static uint GetVillagerHouseOffset(int index) => VillagerHouseAddress + (uint)(Villager2.SIZE * index);

        public const uint FieldItemStart = VillagerAddress - 0x10 + 0x20ac08;

        // Other Addresses
        public const uint ArriverNameLocAddress = 0xB66F4EE0;
        public const uint TextSpeedAddress = 0xBA21BB8;
        public const uint DodoAddress = 0xA97E15C;
        public const uint OnlineSessionAddress = 0x91FD740;
    }
}

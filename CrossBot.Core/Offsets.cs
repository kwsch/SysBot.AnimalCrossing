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

        // PlayerOther 
        public const uint InventoryOffset = 0xACDAD530; // player 0 (A)
        private const uint PlayerOtherStart = InventoryOffset - 0x10; // helps to get other values, unused 

        public const uint WalletAddress = InventoryOffset + 0xB8;
        public const uint MilesAddress = InventoryOffset - 0x25590;
        public const uint BankAddress = InventoryOffset + 0x224CC;

        // Main player offsets functions
        private static uint GetPlayerStart(uint inventoryOffset) => inventoryOffset - 0x10 - PlayerOtherStartPadding + 0x110;
        public static uint GetPlayerIdAddress(uint inventoryOffset) => GetPlayerStart(inventoryOffset) + 0xAFA8;
        public static uint GetPlayerProfileMainAddress(uint inventoryOffset) => GetPlayerStart(inventoryOffset) + 0x116A0;
        public static uint GetManpu(uint inventoryOffset) => inventoryOffset - 0x10 + 0xAF7C + 72;

        // Main save offsets
        public const uint TurnipAddress = 0xABE151EC;
        public const uint VillagerAddress = TurnipAddress - 0x2cb0 - 0x41887c + 0x10;
        public const uint VillagerHouseAddress = TurnipAddress - 0x2cb0 - 0x41887c + 0x417634;

        public static uint GetVillagerOffset(int index) => VillagerAddress + (uint)(Villager2.SIZE * index);
        public static uint GetVillagerHouseOffset(int index) => VillagerHouseAddress + (uint)(Villager2.SIZE * index);

        public const uint FieldItemStart = VillagerAddress - 0x10 + 0x20ac08;
        public const uint LandMakingMapStart = FieldItemStart + 0xAAA00;
        public const uint OutsideFieldStart = FieldItemStart + 0xCF998;
        public const uint MainFieldStructurStart = FieldItemStart + 0xCF600;

        // Other Addresses
        public const uint ArriverNameLocAddress = 0xB66F4EE0;

        public const uint TextSpeedAddress = 0xBA21BB8;

        public const uint DodoAddress = 0xA97E15C;
        public const uint OnlineSessionAddress = 0x91FD740;
    }
}

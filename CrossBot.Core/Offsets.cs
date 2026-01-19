using NHSE.Core;

namespace CrossBot.Core;

/// <summary>
/// Offsets for game RAM locations based on the latest patch revision.
/// </summary>
public static class Offsets
{
    // Helpers
    public const uint PlayerSize = 0x131F70;

    public const uint InventoryOffset = 0xB27BB758; // player 0 (A)
    public const string CoordinatePointer = "[[[[main+4BF9E30]+18]+178]+D0]+DA";

    // Main player offsets functions
    public static uint GetPlayerInventoryOffset(uint profiles) => InventoryOffset + (PlayerSize * (profiles - 1));

    // Main save offsets
    public const uint TurnipAddress = 0xB14DBB30;
    public const uint VillagerAddress = TurnipAddress - 0x2d40 - 0x48d920 + 0x10;
    public const uint VillagerHouseAddress = TurnipAddress - 0x2d40 - 0x48d920 + 0x481c10;

    public static uint GetVillagerOffset(int index) => VillagerAddress + (uint)(Villager2.SIZE * index);
    public static uint GetVillagerHouseOffset(int index) => VillagerHouseAddress + (uint)(VillagerHouse2.SIZE * index);

    public const uint FieldItemStart = VillagerAddress - 0x10 + 0x22f3f0;

    public const uint DodoAddress = 0xAC1A164;
    public const uint OnlineSessionAddress = 0x9499748;
}
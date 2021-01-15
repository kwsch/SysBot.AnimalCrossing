using System;
using System.Collections.Generic;
using NHSE.Core;

namespace CrossBot.Core
{
    /// <summary>
    /// Checks raw RAM to see if the player inventory matches the expected data layout.
    /// </summary>
    public static class InventoryValidator
    {
        private const int ItemSet_Quantity = 2; // Pouch & Pocket.
        private const int ItemSet_ItemCount = 20; // 20 items per item set.
        private const int ItemSet_ItemSize = Item.SIZE * ItemSet_ItemCount;
        private const int ItemSet_MetaSize = 4 + ItemSet_ItemCount;
        private const int ItemSet_TotalSize = (ItemSet_ItemSize + ItemSet_MetaSize) * ItemSet_Quantity;
        private const int ShiftToTopOfStructure = -ItemSet_MetaSize - (Item.SIZE * ItemSet_ItemCount); // shifts slot1 offset => top of data structure

        /// <summary>
        /// Gets the Offset and Size to read from based on the Item 1 RAM offset.
        /// </summary>
        /// <param name="slot1">Item Slot 1 offset in RAM</param>
        public static (uint Offset, int Size) GetOffsetLength(uint slot1) => ((uint)((int)slot1 + ShiftToTopOfStructure), ItemSet_TotalSize);

        /// <summary>
        /// Compares the raw data to the expected data layout.
        /// </summary>
        /// <param name="data">Raw RAM from the game from the offset read (as per <see cref="GetOffsetLength"/>).</param>
        /// <returns>True if valid, false if not valid or corrupt.</returns>
        public static bool ValidateItemBinary(byte[] data)
        {
            // Check the unlocked slot count -- expect 0,10,20
            var bagCount = BitConverter.ToUInt32(data, ItemSet_ItemSize);
            if (bagCount > ItemSet_ItemCount || bagCount % 10 != 0) // pouch21-39 count
                return false;

            var pocketCount = BitConverter.ToUInt32(data, ItemSet_ItemSize + ItemSet_MetaSize + ItemSet_ItemSize);
            if (pocketCount != ItemSet_ItemCount) // pouch0-19 count should be 20.
                return false;

            // Check the item wheel binding -- expect -1 or [0,7]
            // Disallow duplicate binds!
            // Don't bother checking that bind[i] (when ! -1) is not NONE at items[i]. We don't need to check everything!
            var bound = new List<byte>();
            if (!ValidateBindList(data, ItemSet_ItemSize + 4, bound))
                return false;
            if (!ValidateBindList(data, ItemSet_ItemSize + 4 + (ItemSet_ItemSize + ItemSet_MetaSize), bound))
                return false;

            return true;
        }

        private static bool ValidateBindList(byte[] data, int bindStart, ICollection<byte> bound)
        {
            for (int i = 0; i < ItemSet_ItemCount; i++)
            {
                var bind = data[bindStart + i];
                if (bind == 0xFF) // Not bound
                    continue;
                if (bind > 7) // Only [0,7] permitted as the wheel has 8 spots
                    return false;
                if (bound.Contains(bind)) // Wheel index is already bound to another item slot
                    return false;

                bound.Add(bind);
            }

            return true;
        }
    }
}

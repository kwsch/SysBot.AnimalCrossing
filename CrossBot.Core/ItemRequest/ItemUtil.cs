using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NHSE.Core;

namespace CrossBot.Core
{
    /// <summary>
    /// Logic for retrieving <see cref="Item"/> details based off input strings.
    /// </summary>
    public static class ItemUtil
    {
        private static readonly CompareInfo Comparer = CultureInfo.InvariantCulture.CompareInfo;
        private const CompareOptions opt = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreWidth;

        /// <summary>
        /// Gets the first item name-value that contains the <see cref="itemName"/> (case insensitive).
        /// </summary>
        /// <param name="itemName">Requested Item</param>
        /// <param name="lang">Game strings language to fetch with</param>
        /// <returns>Returns <see cref="Item.NO_ITEM"/> if no match found.</returns>
        public static Item GetItem(string itemName, string lang = "en")
        {
            var strings = GameInfo.GetStrings(lang).ItemDataSource;
            return GetItem(itemName, strings);
        }

        /// <summary>
        /// Gets the first item name-value that contains the <see cref="itemName"/> (case insensitive).
        /// </summary>
        /// <param name="itemName">Requested Item</param>
        /// <param name="strings">Game strings</param>
        /// <returns>Returns <see cref="Item.NO_ITEM"/> if no match found.</returns>
        public static Item GetItem(string itemName, IEnumerable<ComboItem> strings)
        {
            if (TryGetItem(itemName, strings, out var id))
                return new Item(id);
            return Item.NO_ITEM;
        }

        /// <summary>
        /// Gets the first item name-value that contains the <see cref="itemName"/> (case insensitive).
        /// </summary>
        /// <param name="itemName">Requested Item</param>
        /// <param name="strings">List of item name-values</param>
        /// <param name="value">Item ID, if found. Otherwise, 0</param>
        /// <returns>True if found, false if none.</returns>
        public static bool TryGetItem(string itemName, IEnumerable<ComboItem> strings, out ushort value)
        {
            foreach (var item in strings)
            {
                var result = Comparer.Compare(item.Text, 0, itemName, 0, opt);
                if (result != 0)
                    continue;

                value = (ushort)item.Value;
                return true;
            }

            value = Item.NONE;
            return false;
        }

        /// <summary>
        /// Gets an enumerable list of item key-value pairs that contain (case insensitive) the requested <see cref="itemName"/>.
        /// </summary>
        /// <param name="itemName">Item name</param>
        /// <param name="strings">Item names (and their Item ID values)</param>
        public static IEnumerable<ComboItem> GetItemsMatching(string itemName, IReadOnlyList<ComboItem> strings)
        {
            foreach (var item in strings)
            {
                var result = Comparer.IndexOf(item.Text, itemName, opt);
                if (result < 0)
                    continue;
                yield return item;
            }
        }

        /// <summary>
        /// Gets an enumerable list of item key-value pairs that contain (case insensitive) the requested <see cref="itemName"/>.
        /// </summary>
        /// <remarks>
        /// Orders the items based on the closest match (<see cref="LevenshteinDistance"/>).
        /// </remarks>
        /// <param name="itemName">Item name</param>
        /// <param name="strings">Item names (and their Item ID values)</param>
        public static IEnumerable<ComboItem> GetItemsMatchingOrdered(string itemName, IReadOnlyList<ComboItem> strings)
        {
            var matches = GetItemsMatching(itemName, strings);
            return GetItemsClosestOrdered(itemName, matches);
        }

        /// <summary>
        /// Gets an enumerable list of item key-value pairs ordered by the closest <see cref="LevenshteinDistance"/> for the requested <see cref="itemName"/>.
        /// </summary>
        /// <param name="itemName">Item name</param>
        /// <param name="strings">Item names (and their Item ID values)</param>
        public static IEnumerable<ComboItem> GetItemsClosestOrdered(string itemName, IEnumerable<ComboItem> strings)
        {
            return strings.OrderBy(z => LevenshteinDistance.Compute(z.Text, itemName));
        }

        /// <summary>
        /// Gets the Item Name and raw 8-byte value as a string.
        /// </summary>
        /// <param name="item">Item value</param>
        public static string GetItemText(Item item)
        {
            var value = BitConverter.ToUInt64(item.ToBytesClass(), 0);
            var name = GameInfo.Strings.GetItemName(item.ItemId);
            return $"{name}: {value:X16}";
        }

        /// <summary>
        /// Gets the u16 item ID from the input hex code.
        /// </summary>
        /// <param name="text">Hex code for the item (preferably 4 digits)</param>
        public static ushort GetID(string text)
        {
            if (!ulong.TryParse(text.Trim(), NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out var value))
                return Item.NONE;
            return (ushort)value;
        }

        /// <summary>
        /// Lists the customization options for the requested <see cref="itemID"/>
        /// </summary>
        /// <param name="itemID">Item ID</param>
        public static string GetItemInfo(ushort itemID)
        {
            var remake = ItemRemakeUtil.GetRemakeIndex(itemID);
            if (remake < 0)
                return string.Empty;

            var info = ItemRemakeInfoData.List[remake];
            return GetItemInfo(info, GameInfo.Strings);
        }

        /// <summary>
        /// Lists the customization options for the requested <see cref="info"/>
        /// </summary>
        /// <param name="info">Item customization possibilities</param>
        /// <param name="str">Game strings</param>
        public static string GetItemInfo(ItemRemakeInfo info, IRemakeString str)
        {
            var sb = new StringBuilder();
            var body = info.GetBodySummary(str);
            if (body.Length > 0)
                sb.AppendLine(body);

            var fabric = info.GetFabricSummary(str);
            if (fabric.Length > 0)
                sb.AppendLine(fabric);

            return sb.ToString();
        }

        /// <summary>
        /// Calculates the position the "Drop" option shows up in the item's interaction menu.
        /// </summary>
        /// <param name="item">Item object to drop</param>
        /// <returns>How many times the down button has to be pressed to reach the "Drop" option.</returns>
        public static int GetItemDropOption(this Item item)
        {
            if (Item.DIYRecipe == item.ItemId)
                return 1;
            if (item.IsWrapped)
                return 0;

            var kind = ItemInfo.GetItemKind(item);
            return kind switch
            {
                ItemKind.Kind_DIYRecipe => 1,

                ItemKind.Kind_Flower => 2,
                _ => 1,
            };
        }

        /// <summary>
        /// Determines if wrapping the <see cref="item"/> is possible.
        /// </summary>
        /// <param name="item">Item object to drop</param>
        /// <returns>True if can be wrapped</returns>
        public static bool ShouldWrapItem(this Item item)
        {
            if (Item.DIYRecipe == item.ItemId)
                return false;

            var kind = ItemInfo.GetItemKind(item);
            return kind switch
            {
                ItemKind.Kind_DIYRecipe => false,

                ItemKind.Kind_Flower => false,
                _ => true,
            };
        }

        /// <summary>
        /// Checks if the <see cref="item"/> is able to be dropped by the player character.
        /// </summary>
        /// <param name="item">Item object to drop</param>
        /// <returns>True if can be dropped</returns>
        public static bool IsDroppable(Item item)
        {
            if (item.IsFieldItem)
                return false;
            if (item.IsExtension)
                return false;
            if (item.IsNone)
                return false;
            if (item.SystemParam > 3)
                return false; // buried, dropped, etc

            var kind = ItemInfo.GetItemKind(item);
            return kind switch
            {
                ItemKind.Kind_Insect => false,

                ItemKind.Kind_DummyPresentbox => false,

                ItemKind.Kind_Fish => false,
                ItemKind.Kind_DiveFish => false,
                ItemKind.Kind_FlowerBud => false,
                ItemKind.Kind_Bush => false,
                ItemKind.Kind_Tree => false,

                _ => true,
            };
        }

        /// <summary>
        /// Checks if the item can be dropped, and sanitizes up any erroneous values if it can be.
        /// </summary>
        /// <param name="item">Requested item to drop.</param>
        /// <returns>True if can be dropped, false if cannot be dropped.</returns>
        public static bool IsSaneItemForDrop(Item item)
        {
            if (!IsDroppable(item))
                return false;

            // Sanitize Values
            if (item.ItemId == Item.MessageBottle || item.ItemId == Item.MessageBottleEgg)
            {
                item.ItemId = Item.DIYRecipe;
                item.FreeParam = 0;
            }

            return true;
        }
    }
}

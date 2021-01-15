using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NHSE.Core;

namespace CrossBot.Core
{
    /// <summary>
    /// Converts hex code requests into lists of items.
    /// </summary>
    public static class ItemRequestUtil
    {
        /// <summary>
        /// Invert the recipe dictionary so we can look up recipe IDs from an input item ID.
        /// </summary>
        public static readonly IReadOnlyDictionary<ushort, ushort> InvertedRecipeDictionary =
            RecipeList.Recipes.ToDictionary(z => z.Value, z => z.Key);

        // Users can put spaces between item codes, or newlines. Recognize both!
        private static readonly string[] Splitters = {" ", "\n", "\r\n"};

        /// <summary>
        /// Gets a list of items from the requested hex string(s).
        /// </summary>
        /// <remarks>
        /// If the first input is a language code (2 characters), the logic will try to parse item names for that language instead of item IDs.
        /// </remarks>
        /// <param name="requestHex">8 byte hex item values (u64 format)</param>
        /// <param name="cfg">Options for packaging items</param>
        public static IReadOnlyCollection<Item> GetItemsFromUserInput(string requestHex, IConfigItem cfg)
        {
            var split = requestHex.Split(Splitters, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                // having a language 2char code will cause an exception in parsing; this is fine and is handled by our catch statement.
                return GetItemsHexCode(split, cfg);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return GetItemsLanguage(split, cfg, GameLanguage.DefaultLanguage);
            }
        }

        /// <summary>
        /// Gets a list of DIY item cards from the requested list of DIY IDs.
        /// </summary>
        /// <remarks>
        /// If the first input is a language code (2 characters), the logic will try to parse item names for that language instead of DIY IDs.
        /// </remarks>
        /// <param name="requestHex">8 byte hex item values (u64 format)</param>
        public static IReadOnlyCollection<Item> GetDIYsFromUserInput(string requestHex)
        {
            var split = requestHex.Split(Splitters, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                // having a language 2char code will cause an exception in parsing; this is fine and is handled by our catch statement.
                return GetDIYItemsHexCode(split);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return GetDIYItemsLanguage(split);
            }
        }

        /// <summary>
        /// Gets a list of items from the requested list of DIY hex code strings.
        /// </summary>
        /// <remarks>
        /// If a hex code parse fails or a recipe ID does not exist, exceptions will be thrown.
        /// </remarks>
        /// <param name="split">List of recipe IDs as u16 hex</param>
        public static IReadOnlyCollection<Item> GetDIYItemsHexCode(IReadOnlyList<string> split)
        {
            var result = new Item[split.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var text = split[i].Trim();
                bool parse = ulong.TryParse(text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var value);
                if (!parse)
                    throw new Exception($"Item value out of expected range ({text}).");

                if (!RecipeList.Recipes.TryGetValue((ushort)value, out _))
                    throw new Exception($"DIY recipe appears to be invalid ({text}).");

                result[i] = new Item(Item.DIYRecipe) { Count = (ushort)value };
            }
            return result;
        }

        /// <summary>
        /// Gets a list of DIY item cards from the requested list of item name strings.
        /// </summary>
        /// <remarks>
        /// If a item name parse fails or a recipe ID does not exist, exceptions will be thrown.
        /// </remarks>
        /// <param name="split">List of item names</param>
        /// <param name="lang">Language code to parse with. If the first entry in <see cref="split"/> is a language code, it will be used instead of <see cref="lang"/>.</param>
        public static IReadOnlyCollection<Item> GetDIYItemsLanguage(IReadOnlyList<string> split, string lang = GameLanguage.DefaultLanguage)
        {
            if (split.Count > 1 && split[0].Length < 3)
            {
                var langIndex = GameLanguage.GetLanguageIndex(split[0]);
                lang = GameLanguage.Language2Char(langIndex);
                split = split.Skip(1).ToArray();
            }

            var result = new Item[split.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var text = split[i].Trim();
                var item = ItemUtil.GetItem(text, lang);
                if (!InvertedRecipeDictionary.TryGetValue(item.ItemId, out var diy))
                    throw new Exception($"DIY recipe appears to be invalid ({text}).");

                result[i] = new Item(Item.DIYRecipe) { Count = diy };
            }
            return result;
        }

        /// <summary>
        /// Gets a list of items from the requested list of item name strings.
        /// </summary>
        /// <remarks>
        /// If a item name parse fails or the item ID does not exist as a known item, exceptions will be thrown.
        /// </remarks>
        /// <param name="split">List of item names</param>
        /// <param name="config">Item packaging options</param>
        /// <param name="lang">Language code to parse with. If the first entry in <see cref="split"/> is a language code, it will be used instead of <see cref="lang"/>.</param>
        public static IReadOnlyCollection<Item> GetItemsLanguage(IReadOnlyList<string> split, IConfigItem config, string lang = GameLanguage.DefaultLanguage)
        {
            if (split.Count > 1 && split[0].Length < 3)
            {
                var langIndex = GameLanguage.GetLanguageIndex(split[0]);
                lang = GameLanguage.Language2Char(langIndex);
                split = split.Skip(1).ToArray();
            }

            var strings = GameInfo.Strings.itemlistdisplay;
            var result = new Item[split.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var text = split[i].Trim();
                var item = CreateItem(text, i, config, lang);

                if (item.ItemId >= strings.Length)
                    throw new Exception($"Item requested is out of expected range ({item.ItemId:X4} > {strings.Length:X4}).");
                if (string.IsNullOrWhiteSpace(strings[item.ItemId]))
                    throw new Exception($"Item requested does not have a valid name ({item.ItemId:X4}).");

                result[i] = item;
            }
            return result;
        }

        /// <summary>
        /// Gets a list of items from the requested list of item hex code strings.
        /// </summary>
        /// <remarks>
        /// If a hex code parse fails or a recipe ID does not exist, exceptions will be thrown.
        /// </remarks>
        /// <param name="split">List of recipe IDs as u16 hex</param>
        /// <param name="config">Item packaging options</param>
        public static IReadOnlyCollection<Item> GetItemsHexCode(IReadOnlyList<string> split, IConfigItem config)
        {
            var strings = GameInfo.Strings.itemlistdisplay;
            var result = new Item[split.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var text = split[i].Trim();
                var convert = GetBytesFromString(text);
                var item = CreateItem(convert, i, config);

                if (item.ItemId >= strings.Length)
                    throw new Exception($"Item requested is out of expected range ({item.ItemId:X4} > {strings.Length:X4}).");
                if (string.IsNullOrWhiteSpace(strings[item.ItemId]))
                    throw new Exception($"Item requested does not have a valid name ({item.ItemId:X4}).");

                result[i] = item;
            }
            return result;
        }

        private static byte[] GetBytesFromString(string text)
        {
            if (!ulong.TryParse(text, NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out var value))
                return Item.NONE.ToBytes();
            return BitConverter.GetBytes(value);
        }

        private static Item CreateItem(string name, int requestIndex, IConfigItem config, string lang = "en")
        {
            var item = ItemUtil.GetItem(name, lang);
            if (item.IsNone)
                throw new Exception($"Failed to convert item (index {requestIndex}: {name}) for Language {lang}.");

            if (!ItemUtil.IsSaneItemForDrop(item))
                throw new Exception($"Unsupported item: (index {requestIndex}: {name}).");

            if (config.WrapAllItems && item.ShouldWrapItem())
                item.SetWrapping(ItemWrapping.WrappingPaper, config.WrappingPaper, true);

            return item;
        }

        private static Item CreateItem(byte[] convert, int requestIndex, IConfigItem config)
        {
            Item item;
            try
            {
                item = convert.ToClass<Item>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to convert item (index {requestIndex}: {ex.Message}).");
            }

            if (!ItemUtil.IsSaneItemForDrop(item) || convert.Length != Item.SIZE)
                throw new Exception($"Unsupported item: (index {requestIndex}).");

            if (config.WrapAllItems && item.ShouldWrapItem())
                item.SetWrapping(ItemWrapping.WrappingPaper, config.WrappingPaper, true);
            return item;
        }
    }
}

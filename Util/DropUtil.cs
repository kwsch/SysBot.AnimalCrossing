using System;
using System.Collections.Generic;
using System.Globalization;
using NHSE.Core;

namespace SysBot.AnimalCrossing
{
    public static class DropUtil
    {
        public static IReadOnlyCollection<Item> GetDIYItems(IReadOnlyList<string> split)
        {
            var result = new Item[split.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var text = split[i];
                bool parse = ulong.TryParse(text.Trim(), NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out var val);
                if (!parse || val > 0x420)
                    throw new Exception($"Item value out of expected range ({text}).");

                if (!RecipeList.Recipes.TryGetValue((ushort)val, out _))
                    throw new Exception($"DIY recipe appears to be invalid ({text}).");

                result[i] = new Item(Item.DIYRecipe) { Count = (ushort)val };
            }
            return result;
        }

        public static IReadOnlyCollection<Item> GetItems(IReadOnlyList<string> split, IConfigItem config)
        {
            var strings = GameInfo.Strings.itemlistdisplay;
            var result = new Item[split.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var text = split[i];
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
            if (!ulong.TryParse(text.Trim(), NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out var val))
                return Item.NONE.ToBytes();
            return BitConverter.GetBytes(val);
        }

        private static Item CreateItem(byte[] convert, int i, IConfigItem config)
        {
            Item item;
            try
            {
                item = convert.ToClass<Item>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to convert item {i}: {ex.Message}");
            }

            if (!IsSaneItem(item) || convert.Length != Item.SIZE)
                throw new Exception($"Unsupported item: {i}");

            if (config.WrapAllItems && item.ShouldWrapItem())
                item.SetWrapping(ItemWrapping.WrappingPaper, config.WrappingPaper, true);
            return item;
        }

        private static bool IsSaneItem(Item item)
        {
            if (item.IsFieldItem)
                return false;
            if (item.IsExtension)
                return false;
            if (item.IsNone)
                return false;
            if (item.SystemParam > 3)
                return false; // buried, dropped, etc

            if (item.ItemId == Item.MessageBottle || item.ItemId == Item.MessageBottleEgg)
            {
                item.ItemId = Item.DIYRecipe;
                item.FreeParam = 0;
            }

            return true;
        }
    }
}

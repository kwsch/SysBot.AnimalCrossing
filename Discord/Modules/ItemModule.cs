using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using NHSE.Core;

namespace SysBot.AnimalCrossing
{
    public class ItemModule : ModuleBase<SocketCommandContext>
    {
        [Command("lookupLang")]
        [Alias("ll")]
        [Summary("Gets a list of items that contain the request string.")]
        public async Task SearchItemsAsync(string language, [Remainder]string itemName)
        {
            var strings = GameInfo.GetStrings(language).ItemDataSource;
            await PrintItemsAsync(itemName, strings).ConfigureAwait(false);
        }

        [Command("lookup")]
        [Alias("li")]
        [Summary("Gets a list of items that contain the request string.")]
        public async Task SearchItemsAsync([Remainder]string itemName)
        {
            var strings = GameInfo.Strings.ItemDataSource;
            await PrintItemsAsync(itemName, strings).ConfigureAwait(false);
        }

        private async Task PrintItemsAsync(string itemName, IEnumerable<ComboItem> strings)
        {
            const int minLength = 2;
            if (itemName.Length <= minLength)
            {
                await ReplyAsync($"Please enter a search term longer than {minLength} characters.").ConfigureAwait(false);
                return;
            }

            var ci = CultureInfo.InvariantCulture.CompareInfo;
            var matches = strings.Where(z => ci.IndexOf(z.Text, itemName, CompareOptions.OrdinalIgnoreCase) >= 0);
            var result = string.Join(Environment.NewLine, matches.Select(z => $"{z.Value:X4} {z.Text}"));

            if (result.Length == 0)
            {
                await ReplyAsync("No matches found.").ConfigureAwait(false);
                return;
            }

            const int maxLength = 500;
            if (result.Length > maxLength)
                result = result.Substring(0, maxLength);

            await ReplyAsync(Format.Code(result)).ConfigureAwait(false);
        }

        [Command("item")]
        [Summary("Gets the info for an item.")]
        public async Task GetItemInfoAsync(string itemHex)
        {
            ushort itemID = GetID(itemHex);
            if (itemID == Item.NONE)
            {
                await ReplyAsync("Invalid item requested.").ConfigureAwait(false);
                return;
            }

            var name = GameInfo.Strings.GetItemName(itemID);
            var result = GetItemInfo(itemID);
            if (result.Length == 0)
                await ReplyAsync($"No customization data available for the requested item ({name}).").ConfigureAwait(false);
            else
                await ReplyAsync($"{name}:\r\n{result}").ConfigureAwait(false);
        }

        [Command("stack")]
        [Summary("Stacks an item and prints the hex code.")]
        public async Task StackAsync(string itemHex, int count)
        {
            ushort itemID = GetID(itemHex);
            if (itemID == Item.NONE || count < 1 || count > 99)
            {
                await ReplyAsync("Invalid item requested.").ConfigureAwait(false);
                return;
            }

            var ct = count - 1; // value 0 => count of 1
            var item = new Item(itemID) {Count = (ushort)ct};
            var msg = GetItemText(item);
            await ReplyAsync(msg).ConfigureAwait(false);
        }

        [Command("customize")]
        [Summary("Customizes an item and prints the hex code.")]
        public async Task CustomizeAsync(string itemHex, int sum)
        {
            ushort itemID = GetID(itemHex);
            if (itemID == Item.NONE)
            {
                await ReplyAsync("Invalid item requested.").ConfigureAwait(false);
                return;
            }

            var remake = ItemRemakeUtil.GetRemakeIndex(itemID);
            if (remake < 0)
            {
                await ReplyAsync("No customization data available for the requested item.").ConfigureAwait(false);
                return;
            }

            int body = sum & 7;
            int fabric = sum >> 5;

            var info = ItemRemakeInfoData.List[remake];
            bool hasBody = body <= 7 && body <= info.ReBodyPatternNum;
            bool hasFabric = fabric <= 7 && info.GetFabricDescription(fabric) != "Invalid";

            if (!hasBody || !hasFabric)
                await ReplyAsync("Requested customization for item appears to be invalid.").ConfigureAwait(false);

            var item = new Item(itemID) {BodyType = body, PatternChoice = fabric};
            var msg = GetItemText(item);
            await ReplyAsync(msg).ConfigureAwait(false);
        }

        private static string GetItemText(Item item)
        {
            var value = BitConverter.ToUInt64(item.ToBytesClass(), 0);
            var name = GameInfo.Strings.GetItemName(item.ItemId);
            return $"{name}: {value:X16}";
        }

        private static ushort GetID(string text)
        {
            if (!ulong.TryParse(text.Trim(), NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out var val))
                return Item.NONE;
            return (ushort)val;
        }

        public static string GetItemInfo(ushort itemID)
        {
            var remake = ItemRemakeUtil.GetRemakeIndex(itemID);
            if (remake < 0)
                return string.Empty;

            var info = ItemRemakeInfoData.List[remake];
            var body = info.GetBodySummary(GameInfo.Strings);

            var sb = new StringBuilder();
            if (body.Length > 0)
                sb.AppendLine(body);

            var fabric = info.GetFabricSummary(GameInfo.Strings);
            if (fabric.Length > 0)
                sb.AppendLine(fabric);

            return sb.ToString();
        }
    }
}

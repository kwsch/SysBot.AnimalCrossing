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
        public async Task SearchItemsAsync([Summary("Language code to search with")] string language, [Summary("Item name / item substring")][Remainder]string itemName)
        {
            var strings = GameInfo.GetStrings(language).ItemDataSource;
            await PrintItemsAsync(itemName, strings).ConfigureAwait(false);
        }

        [Command("lookup")]
        [Alias("li")]
        [Summary("Gets a list of items that contain the request string.")]
        public async Task SearchItemsAsync([Summary("Item name / item substring")][Remainder]string itemName)
        {
            var strings = GameInfo.Strings.ItemDataSource;
            await PrintItemsAsync(itemName, strings).ConfigureAwait(false);
        }

        private async Task PrintItemsAsync(string itemName, IReadOnlyList<ComboItem> strings)
        {
            const int minLength = 2;
            if (itemName.Length <= minLength)
            {
                await ReplyAsync($"Please enter a search term longer than {minLength} characters.").ConfigureAwait(false);
                return;
            }

            foreach (var item in strings)
            {
                if (!string.Equals(item.Text, itemName, StringComparison.OrdinalIgnoreCase))
                    continue;

                var msg = $"{item.Value:X4} {item.Text}";
                await ReplyAsync(Format.Code(msg)).ConfigureAwait(false);
                return;
            }

            var ci = CultureInfo.InvariantCulture.CompareInfo;
            var matches = strings.Where(z => ci.IndexOf(z.Text, itemName, CompareOptions.OrdinalIgnoreCase) >= 0).ToArray();
            var result = string.Join(Environment.NewLine, matches.Select(z => $"{z.Value:X4} {z.Text}"));

            if (result.Length == 0)
            {
                await ReplyAsync("No matches found.").ConfigureAwait(false);
                return;
            }

            const int maxLength = 500;
            if (result.Length > maxLength)
            {
                var ordered = matches.OrderBy(z => LevenshteinDistance.Compute(z.Text, itemName));
                result = string.Join(Environment.NewLine, ordered.Select(z => $"{z.Value:X4} {z.Text}"));
                result = result.Substring(0, maxLength) + "...[truncated]";
            }

            await ReplyAsync(Format.Code(result)).ConfigureAwait(false);
        }

        [Command("item")]
        [Summary("Gets the info for an item.")]
        public async Task GetItemInfoAsync([Summary("Item ID (in hex)")]string itemHex)
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
        public async Task StackAsync([Summary("Item ID (in hex)")]string itemHex, [Summary("Count of items in the stack")]int count)
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
        public async Task CustomizeAsync([Summary("Item ID (in hex)")]string itemHex, [Summary("Customization value sum")]int sum)
        {
            ushort itemID = GetID(itemHex);
            if (itemID == Item.NONE)
            {
                await ReplyAsync("Invalid item requested.").ConfigureAwait(false);
                return;
            }
            if (sum <= 0)
            {
                await ReplyAsync("No customization data specified.").ConfigureAwait(false);
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
            if (fabric > 7 || ((fabric << 5) | body) != sum)
            {
                await ReplyAsync("Invalid customization data specified.").ConfigureAwait(false);
                return;
            }

            var info = ItemRemakeInfoData.List[remake];
            bool hasBody = body == 0 || (body <= 7 && body <= info.ReBodyPatternNum);
            bool hasFabric = fabric == 0 || (fabric <= 7 && info.GetFabricDescription(fabric) != "Invalid");

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
            return GetItemInfo(info, GameInfo.Strings);
        }

        private static string GetItemInfo(ItemRemakeInfo info, IRemakeString str)
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
    }
}

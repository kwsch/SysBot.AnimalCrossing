using System;
using System.Threading.Tasks;
using CrossBot.Core;
using Discord;
using NHSE.Core;

namespace CrossBot.Discord
{
    public static class DiscordUtil
    {
        public static async Task<(DownloadResult Code, Item[] Items)> TryDownloadItems(Attachment att1, int maxCount)
        {
            return await NetUtil.GetItemArrayFromLink(att1.Filename, att1.Size, att1.Url, maxCount).ConfigureAwait(false);
        }

        public static string GetItemErrorMessage(DownloadResult code, int max) => code switch
        {
            DownloadResult.Unsupported => "That `nhi` does not appear to be a valid size.",
            DownloadResult.SizeBad => "That `nhi` does not appear to be a valid size.",
            DownloadResult.SizeBig => $"That `nhi` file is way too big. I only allow at most {max} items from an `nhi` file.",
            _ => throw new ArgumentOutOfRangeException(nameof(code)),
        };
    }
}

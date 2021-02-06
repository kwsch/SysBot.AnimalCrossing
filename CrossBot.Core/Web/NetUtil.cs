using System;
using System.Net;
using System.Threading.Tasks;
using NHSE.Core;

namespace CrossBot.Core
{
    public static class NetUtil
    {
        private static readonly WebClient webClient = new();

        public static async Task<byte[]> DownloadFromUrlAsync(string url)
        {
            return await webClient.DownloadDataTaskAsync(url).ConfigureAwait(false);
        }

        public static async Task<(DownloadResult, Item[])> GetItemArrayFromLink(string fn, int size, string url, int maxCount)
        {
            if (!fn.EndsWith(".nhi"))
                return (DownloadResult.Unsupported, Array.Empty<Item>());

            if (size % Item.SIZE != 0 || size == 0)
                return (DownloadResult.SizeBad, Array.Empty<Item>());

            if (size > Item.SIZE * maxCount)
                return (DownloadResult.SizeBig, Array.Empty<Item>());

            var data = await DownloadFromUrlAsync(url).ConfigureAwait(false);
            var items = Item.GetArray(data);
            return (DownloadResult.Success, items);
        }
    }
}

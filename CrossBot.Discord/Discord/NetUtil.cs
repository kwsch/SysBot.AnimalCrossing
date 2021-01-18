using System.Net;
using System.Threading.Tasks;

namespace CrossBot.Discord
{
    public static class NetUtil
    {
        private static readonly WebClient webClient = new();

        public static async Task<byte[]> DownloadFromUrlAsync(string url)
        {
            return await webClient.DownloadDataTaskAsync(url).ConfigureAwait(false);
        }
    }
}

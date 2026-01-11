using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SysBot.Base;

namespace CrossBot.SysBot
{
    public static partial class PointerUtil
    {

        [GeneratedRegex(@"(\+|\-)([A-Fa-f0-9]+)")]
        private static partial Regex PointerRegex();

        public static async Task<ulong> GetPointerAddressFromExpression(ISwitchConnectionAsync sw, string pointerExpression, CancellationToken token)
        {
            // Regex pattern to get operators and offsets from pointer expression.
            var regex = PointerRegex();
            var match = regex.Match(pointerExpression);

            // Get first offset from pointer expression and read address at that offset from main start.
            var ofs = Convert.ToUInt64(match.Groups[2].Value, 16);
            var address = BitConverter.ToUInt64(await sw.ReadBytesMainAsync(ofs, 0x8, token).ConfigureAwait(false), 0);
            match = match.NextMatch();

            // Matches the rest of the operators and offsets in the pointer expression.
            while (match.Success)
            {
                // Get operator and offset from match.
                string opp = match.Groups[1].Value;
                ofs = Convert.ToUInt64(match.Groups[2].Value, 16);

                // Add or subtract the offset from the current stored address based on operator in front of offset.
                switch (opp)
                {
                    case "+":
                        address += ofs;
                        break;
                    case "-":
                        address -= ofs;
                        break;
                }

                // Attempt another match and if successful read bytes at address and store the new address.
                match = match.NextMatch();
                if (!match.Success)
                    continue;

                byte[] bytes = await sw.ReadBytesAbsoluteAsync(address, 0x8, token).ConfigureAwait(false);
                address = BitConverter.ToUInt64(bytes, 0);
            }

            return address;
        }
    }
}

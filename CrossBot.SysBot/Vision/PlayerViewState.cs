using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SysBot.Base;

namespace CrossBot.SysBot
{
    public abstract class PlayerViewState
    {
        protected readonly ISwitchConnectionAsync Connection;
        protected readonly SwitchRoutineExecutor<BotConfig> Executor;

        protected const uint LinkSessionActiveOffset = 0x91DD740;
        protected const uint DodoCodeOffset = 0xA95E0F4;
        protected ulong CoordinateAddressIsland;
        protected ulong CoordinateAddressAirport;

        protected PlayerViewState(SwitchRoutineExecutor<BotConfig> con)
        {
            Executor = con;
            Connection = (ISwitchConnectionAsync)con.Connection;
        }

        protected async Task SetPosition(byte[] x, byte[] y, ulong address, CancellationToken token)
        {
            Debug.Assert(x.Length == 8);
            Debug.Assert(y.Length == 8);
            var data = x.Concat(y).ToArray();
            await Connection.WriteBytesAbsoluteAsync(data, address, token).ConfigureAwait(false);
        }

        protected async Task SetRotation(byte[] r, ulong address, CancellationToken token)
        {
            Debug.Assert(r.Length == 4);
            await Connection.WriteBytesAbsoluteAsync(r, address, token).ConfigureAwait(false);
        }

        // Checks if player is in overworld (outside of a building).
        protected async Task<bool> IsOverworld(CancellationToken token)
        {
            var state = await Connection.ReadBytesAbsoluteAsync(CoordinateAddressIsland + 0x1E, 0x4, token).ConfigureAwait(false);
            var x = BitConverter.ToUInt32(state, 0);
            return x == 0xC0066666;
        }

        public async Task<bool> IsLinkSessionActive(CancellationToken token)
        {
            // Checks if the session is still active and gates are still open. (Can close due to a player disconnecting while flying to your island.)
            var x = await Connection.ReadBytesAsync(LinkSessionActiveOffset, 1, token).ConfigureAwait(false);
            return x[0] == 1;
        }
    }
}

using System;
using System.Diagnostics;
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
        protected const int PlayerCoordinateSize = 10;

        protected PlayerViewState(SwitchRoutineExecutor<BotConfig> con)
        {
            Executor = con;
            Connection = (ISwitchConnectionAsync)con.Connection;
        }

        protected async Task SetPosition(ushort x, ushort y, ulong address, CancellationToken token)
        {
            var coords = new byte[10];
            BitConverter.GetBytes(x).CopyTo(coords, 0);
            BitConverter.GetBytes(y).CopyTo(coords, 8);
            await SetPosition(coords, address, token).ConfigureAwait(false);
        }

        protected async Task SetPosition(byte[] coords, ulong address, CancellationToken token)
        {
            Debug.Assert(coords.Length == PlayerCoordinateSize);
            await Connection.WriteBytesAbsoluteAsync(coords, address, token).ConfigureAwait(false);
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
            var value = BitConverter.ToUInt32(state, 0);
            return GetOverworldState(value) == OverworldState.Overworld;
        }

        public static OverworldState GetOverworldState(uint value) => value switch
        {
            0x00000000 => OverworldState.Null,
            0xC0066666 => OverworldState.Overworld,
            0xBE200000 => OverworldState.UserArriveLeaving,
            _ when (value & 0xFFFF) == 0xC906 => OverworldState.Loading,
            _ => OverworldState.Unknown,
        };

        public async Task<bool> IsLinkSessionActive(CancellationToken token)
        {
            // Checks if the session is still active and gates are still open. (Can close due to a player disconnecting while flying to your island.)
            var x = await Connection.ReadBytesAsync(LinkSessionActiveOffset, 1, token).ConfigureAwait(false);
            return x[0] == 1;
        }
    }

    public enum OverworldState
    {
        Null,
        Overworld,
        UserArriveLeaving,
        Unknown,
        Loading
    }
}

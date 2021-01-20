using System;
using System.Threading;
using System.Threading.Tasks;
using SysBot.Base;

namespace CrossBot.SysBot
{
    public class AdvancedViewState : PlayerViewState
    {
        private byte[] InitialPlayerCoordinates = Array.Empty<byte>();

        public AdvancedViewState(SwitchRoutineExecutor<BotConfig> con) : base(con) => Config = con.Config.ViewConfig;
        public readonly ViewStateConfig Config;

        public async Task Initialize(CancellationToken token)
        {
            var exp = Executor.Config.ViewConfig.CoordinatePointer;
            CoordinateAddressIsland = await PointerUtil.GetPointerAddressFromExpression(Connection, exp, token).ConfigureAwait(false);

            Executor.Log("Saving starting position.");
            InitialPlayerCoordinates = await Connection.ReadBytesAbsoluteAsync(CoordinateAddressIsland, PlayerCoordinateSize, token).ConfigureAwait(false);
        }

        #region Open Gates, Get Dodo Code
        public async Task<string?> GetDodoCode(CancellationToken token)
        {
            // Obtain Dodo code from offset and store it.
            byte[] bytes = await Connection.ReadBytesAsync(DodoCodeOffset, 0x5, token).ConfigureAwait(false);
            var result = System.Text.Encoding.UTF8.GetString(bytes, 0, 5);
            if (string.IsNullOrWhiteSpace(result))
                Connection.LogError($"Failed to retrieve dodo code from 0x{DodoCodeOffset:X8}.");
            Connection.Log($"Retrieved Dodo code: {result}.");
            return result;
        }

        public async Task ResetToStartPosition(CancellationToken token)
        {
            // Sets player xy coordinates to their initial values when bot was started and set player rotation to 0.
            await SetPosition(InitialPlayerCoordinates, CoordinateAddressIsland, token).ConfigureAwait(false);
            await SetRotation(new byte[4], CoordinateAddressIsland + 0x3A, token).ConfigureAwait(false);
        }

        public async Task WarpToAirportFromIsland(CancellationToken token)
        {
            // Teleport player to airport entrance and set rotation to face doorway.
            var x = Config.AirportX;
            var y = Config.AirportY;
            await SetPosition(x, y, CoordinateAddressIsland, token).ConfigureAwait(false);

            var r = new byte[] { 0, 0, 0, 112 };
            await SetRotation(r, CoordinateAddressIsland + 0x3A, token).ConfigureAwait(false);

            // Walk through airport entrance.
            await Executor.SetStick(SwitchStick.LEFT, 20_000, 20_000, 1_000, token).ConfigureAwait(false);
            await Executor.SetStick(SwitchStick.LEFT, 0, 0, 9_000, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Teleport into warp zone to leave airport
        /// </summary>
        public async Task WarpToIslandFromAirport(CancellationToken token)
        {
            await Connection.WriteBytesAbsoluteAsync(new byte[] { 32, 67, 0, 0, 0, 0, 0, 0, 120, 67 }, CoordinateAddressAirport, token).ConfigureAwait(false);
        }

        public async Task WarpToDodoCounter(CancellationToken token)
        {
            // Get player's coordinate address when inside airport and teleport player to Dodo.
            var exp = Executor.Config.ViewConfig.CoordinatePointer;
            CoordinateAddressAirport = await PointerUtil.GetPointerAddressFromExpression(Connection, exp, token).ConfigureAwait(false);

            var coords = new byte[] {58, 67, 0, 0, 0, 0, 0, 0, 38, 67};
            await SetPosition(coords, CoordinateAddressAirport, token).ConfigureAwait(false);
        }

        public async Task OpenGates(CancellationToken token)
        {
            // Navigate through dialog with Dodo to open gates and to get Dodo code.
            var Hold = SwitchCommand.Hold(SwitchButton.L);
            await Connection.SendAsync(Hold, token).ConfigureAwait(false);
            await Task.Delay(0_500, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 3_500, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 1_500, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 1_500, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.DDOWN, 0_300, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 2_500, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 1_000, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.DDOWN, 0_300, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 2_000, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 1_000, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 12_000, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 1_500, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.DDOWN, 0_300, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.DDOWN, 0_300, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 1_500, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 1_000, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.DUP, 0_300, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 2_500, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 1_000, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 1_500, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 2_500, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 3_000, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 2_000, token).ConfigureAwait(false);
            await Executor.Click(SwitchButton.A, 2_000, token).ConfigureAwait(false);
            var Release = SwitchCommand.Release(SwitchButton.L);
            await Connection.SendAsync(Release, token).ConfigureAwait(false);
        }

        // Wait for loading screen to finish animating and return to island.
        public async Task WaitEnterIsland(CancellationToken token)
        {
            while (!await IsOverworld(token).ConfigureAwait(false))
                await Task.Delay(Executor.Config.ViewConfig.OverworldLoopCheckDelay, token).ConfigureAwait(false);
        }
        #endregion

        public async Task<(ushort X, ushort Y)> GetCoordinates(CancellationToken token)
        {
            var exp = Executor.Config.ViewConfig.CoordinatePointer;
            CoordinateAddressIsland = await PointerUtil.GetPointerAddressFromExpression(Connection, exp, token).ConfigureAwait(false);

            var result = await Connection.ReadBytesAbsoluteAsync(CoordinateAddressIsland, PlayerCoordinateSize, token).ConfigureAwait(false);
            var x = BitConverter.ToUInt16(result, 0);
            var y = BitConverter.ToUInt16(result, 8);
            return (x, y);
        }

        public async Task SetCoordinates(ushort x, ushort y, CancellationToken token)
        {
            var exp = Executor.Config.ViewConfig.CoordinatePointer;
            var ofs = await PointerUtil.GetPointerAddressFromExpression(Connection, exp, token).ConfigureAwait(false);
            await SetPosition(x, y, ofs, token).ConfigureAwait(false);
        }
    }
}

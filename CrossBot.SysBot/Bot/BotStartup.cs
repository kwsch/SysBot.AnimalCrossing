using System.Threading;
using System.Threading.Tasks;
using SysBot.Base;

namespace CrossBot.SysBot
{
    public static class BotStartup
    {
        public static async Task<bool> ValidateStartup(Bot b, CancellationToken token)
        {
            // Validate our config file inputs.
            if (!ValidateConfigFileParameters(b))
                return false;

            // Disconnect our virtual controller; will reconnect once we send a button command after a request.
            b.Log("Detaching controller on startup as first interaction.");
            await b.Connection.SendAsync(SwitchCommand.DetachController(b.UseCRLF), token).ConfigureAwait(false);
            await Task.Delay(200, token).ConfigureAwait(false);

            // Validate inventory offset.
            var inventoryValid = await ValidateStartupInventory(b, token).ConfigureAwait(false);
            if (!inventoryValid)
                return false;

            var sessionActive = await b.ViewState.IsLinkSessionActive(token).ConfigureAwait(false);
            if (sessionActive)
                return true;

            return await b.ViewState.StartupOpenGates(b, token).ConfigureAwait(false);
        }

        private static bool ValidateConfigFileParameters(Bot b)
        {
            var coord = b.Config.FieldItemConfig.ValidateCoordinates();
            if (coord == CoordinateResult.Valid)
                return true;
            b.Log($"Coordinates are not valid! {coord}.");
            return false;
        }

        private static async Task<bool> ValidateStartupInventory(Bot b, CancellationToken token)
        {
            b.Log("Checking inventory offset for validity.");
            var valid = await b.GetIsPlayerInventoryValid(b.Config.Offset, token).ConfigureAwait(false);
            if (valid)
                return true;

            b.Log($"Inventory read from {b.Config.Offset} (0x{b.Config.Offset:X8}) does not appear to be valid.");
            return !b.Config.RequireValidInventoryMetadata;
        }

        public static async Task<bool> StartupOpenGates(this AdvancedViewState s, Bot b, CancellationToken token)
        {
            if (!s.Config.DodoCodeRetrieval)
            {
                b.Log($"{nameof(ViewStateConfig.DodoCodeRetrieval)} has to be enabled to automatically retrieve Dodo code.");
                return false;
            }

            if (!s.Config.AllowTeleportation)
            {
                b.Log($"{nameof(ViewStateConfig.AllowTeleportation)} has to be enabled to automatically retrieve Dodo code.");
                return false;
            }

            await s.Initialize(token).ConfigureAwait(false);

            // Open gates and retrieve Dodo code in airport.
            b.Log("Opening gates and obtaining Dodo code.");
            await s.WarpToAirportFromIsland(token).ConfigureAwait(false);
            await s.WarpToDodoCounter(token).ConfigureAwait(false);
            await s.OpenGates(token).ConfigureAwait(false);
            await s.WarpToIslandFromAirport(token).ConfigureAwait(false);
            await s.WaitEnterIsland(token).ConfigureAwait(false);

            var code = await s.GetDodoCode(token).ConfigureAwait(false);
            if (code == null)
                return false;

            b.Island.DodoCode = code;

            // Reset player position to initial position.
            await s.ResetToStartPosition(token).ConfigureAwait(false);

            return true;
        }
    }
}

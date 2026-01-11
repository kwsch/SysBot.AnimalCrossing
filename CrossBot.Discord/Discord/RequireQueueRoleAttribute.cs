using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace CrossBot.Discord
{
    /// <summary>
    /// Attribute that requires the command issuer to have a certain assigned role.
    /// </summary>
    /// <remarks>
    /// If the user has elevated permissions (sudo) or is the owner, the command will be permitted regardless of assigned role matching.
    /// </remarks>
    public sealed class RequireQueueRoleAttribute(string name) : PreconditionAttribute
    {
        // Create a field to store the specified name

        // Create a constructor so the name can be specified

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var mgr = Globals.Self.Config;
            if (mgr.CanUseSudo(context.User.Id) || Globals.Self.Owner == context.User.Id)
                return Task.FromResult(PreconditionResult.FromSuccess());

            // Check if this user is a Guild User, which is the only context where roles exist
            if (context.User is not SocketGuildUser gUser)
                return Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));

            if (!Globals.Bot.Config.AcceptingCommands)
                return Task.FromResult(PreconditionResult.FromError("Sorry, I am not currently accepting commands!"));

            bool hasRole = mgr.GetHasRole(name, gUser.Roles.Select(z => z.Name));
            if (!hasRole)
                return Task.FromResult(PreconditionResult.FromError("You do not have the required role to run this command."));

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}

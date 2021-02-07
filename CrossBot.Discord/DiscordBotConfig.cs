using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossBot.Discord
{
    [Serializable]
    public class DiscordBotConfig
    {
        /// <summary> Custom Discord Status for playing a game. </summary>
        public string Name { get; set; } = "CrossBot";

        /// <summary> Bot login token. </summary>
        public string Token { get; set; } = "DISCORD_TOKEN";

        /// <summary> Bot command prefix. </summary>
        public string Prefix { get; set; } = "$";

        /// <summary> Users with this role are allowed to interact with the bot. If "@everyone", anyone can interact. </summary>
        public string RoleUseBot { get; set; } = "@everyone";

        /// <summary> Skips creating the Discord bot (logging in); helpful for those not wanting interactions via Discord. </summary>
        public bool SkipDiscordBotCreation { get; set; }

        /// <summary> Sets the current Dodo code as the Bot's status. </summary>
        public bool SetStatusAsDodoCode { get; set; }

        // 64bit numbers white-listing certain channels/users for permission
        public List<ulong> Channels { get; set; } = new();
        public List<ulong> Users { get; set; } = new();
        public List<ulong> Sudo { get; set; } = new();

        /// <summary>
        /// Modules that are blacklisted. Comma separated, by class name.
        /// </summary>
        public string ModuleBlacklist { get; set; } = string.Empty;

        public bool CanUseCommandUser(ulong authorId) => Users.Count == 0 || Users.Contains(authorId);
        public bool CanUseCommandChannel(ulong channelId) => Channels.Count == 0 || Channels.Contains(channelId);
        public bool CanUseSudo(ulong userId) => Sudo.Contains(userId);

        public bool GetHasRole(string roleName, IEnumerable<string> roles)
        {
            return roleName switch
            {
                nameof(RoleUseBot) => roles.Contains(RoleUseBot),
                _ => throw new ArgumentException($"{roleName} is not a valid role type.", nameof(roleName)),
            };
        }
    }
}
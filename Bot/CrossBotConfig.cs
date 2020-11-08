﻿using System;
using System.Collections.Generic;
using System.Linq;
using SysBot.Base;

namespace SysBot.AnimalCrossing
{
    public sealed class CrossBotConfig : SwitchBotConfig
    {
        #region Discord

        /// <summary> When enabled, the bot will accept commands from users via Discord. </summary>
        public bool AcceptingCommands { get; set; } = true;

        /// <summary> Custom Discord Status for playing a game. </summary>
        public string Name { get; set; } = "CrossBot";

        /// <summary> Bot login token. </summary>
        public string Token { get; set; } = "DISCORD_TOKEN";

        /// <summary> Bot command prefix. </summary>
        public string Prefix { get; set; } = "$";

        /// <summary> Users with this role are allowed to interact with the bot. If "@everyone", anyone can request custom items. </summary>
        public string RoleUseBot { get; set; } = "@everyone";

        // 64bit numbers white-listing certain channels/users for permission
        public List<ulong> Channels { get; set; } = new List<ulong>();
        public List<ulong> Users { get; set; } = new List<ulong>();
        public List<ulong> Sudo { get; set; } = new List<ulong>();

        #endregion

        #region Features

        /// <summary> Skips creating bots when the program is started; helpful for testing integrations. </summary>
        public bool SkipConsoleBotCreation { get; set; }

        /// <summary> Offset the items are injected at. This should be the player inventory slot you have currently selected in-game. </summary>
        public uint Offset { get; set; } = 0xABC25840;

        /// <summary> When enabled, the Bot will not allow RAM edits if the player's item metadata is invalid. </summary>
        /// <remarks> Only disable this as a last resort, and you have corrupted your item metadata through other means. </remarks>
        public bool RequireValidInventoryMetadata { get; set; } = true;

        public DropBotConfig DropConfig { get; set; } = new DropBotConfig();

        /// <summary> When enabled, users in Discord can request the bot to pick up items (spamming Y a <see cref="DropBotConfig.PickupCount"/> times). </summary>
        public bool AllowClean { get; set; }

        #endregion

        public bool CanUseCommandUser(ulong authorId) => Users.Count == 0 || Users.Contains(authorId);
        public bool CanUseCommandChannel(ulong channelId) => Channels.Count == 0 || Channels.Contains(channelId);
        public bool CanUseSudo(ulong userId) => Sudo.Contains(userId);

        public bool GetHasRole(string roleName, IEnumerable<string> roles)
        {
            return roleName switch
            {
                nameof(RoleUseBot) => roles.Contains(RoleUseBot),
                _ => throw new ArgumentException(nameof(roleName))
            };
        }
    }
}

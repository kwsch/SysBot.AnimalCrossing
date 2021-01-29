using System;
using System.Collections.Generic;
using System.Linq;
using SysBot.Base;

namespace SysBot.AnimalCrossing {
    public sealed class CrossBotConfig:SwitchBotConfig {
        #region Offsets
        public uint[] ONLINE_PLAYER_NAME_OFFSET { get; set; } = { 164810672, 164809576, 164808480, 164807384, 164806288, 164805192, 164804096, 164803000 };
        /// <summary> Offset the items are injected at. This should be the player inventory slot you have currently selected in-game. </summary>
        public uint INVENTORY_OFFSET { get; set; } = 2900022576;
        /// <summary> Offset the DODO is found at!</summary>
        public uint DODO_OFFSET { get; set; } = 177725788;
        /// <summary> Offset for DEV search; Debug and trial different offsets, outputs in string!</summary>
        public uint STRING_SEARCH_OFFSET { get; set; } = 164810672;
        #endregion
        #region Twitch
        public bool TWITCH_ENABLED { get; set; } = false;
        public string TWITCH_TOKEN { get; set; } = "";
        public string TWITCH_USERNAME { get; set; } = "";
        public string TWITCH_CHANNEL { get; set; } = "";
        public bool TWITCH_SHARE_DODO { get; set; } = false;
        #endregion
        #region Discord

        /// <summary> Custom Discord Status for playing a game. </summary>
        public string DISCORD_NAME { get; set; } = "CosBot";

        /// <summary> Bot login token. </summary>
        public string DISCORD_TOKEN { get; set; } = "";

        /// <summary> Bot command prefix. </summary>
        public string DISCORD_PREFIX { get; set; } = "$";
        public bool DISCORD_ACCEPTINGDROPS { get; set; } = true;
        public bool DISCORD_ACCEPTINGLOOKUPS { get; set; } = true;
        /// <summary> Users with this role are allowed to request custom items. If empty, anyone can request custom items. </summary>
        public string DISCORD_ROLECUSTOM { get; set; } = string.Empty;

        /// <summary> Channels queue gets posted however not necessarily where commands get taken!</summary>
        public ulong[] DISCORD_QUEUECHANNEL { get; set; } = { };
        public List<ulong> DISCORD_CHANNELS { get; set; } = new List<ulong>();
        public List<ulong> DISCORD_USERS { get; set; } = new List<ulong>();
        public List<ulong> DISCORD_SUDO { get; set; } = new List<ulong>();
        // What user can use the bot!?
        public bool CanUseCommandUser(ulong authorId) => DISCORD_USERS.Count == 0 || DISCORD_USERS.Contains(authorId);
        // What channels can use the bot!?
        public bool CanUseCommandChannel(ulong channelId) => DISCORD_CHANNELS.Count == 0 || DISCORD_CHANNELS.Contains(channelId);
        // What role can control the bot!?
        public bool CanUseSudo(ulong userId) => DISCORD_SUDO.Contains(userId);

        // What role can use the bot!?
        public bool GetHasRole(string roleName, IEnumerable<string> roles)
        {
            return roleName switch
            {
                nameof(DISCORD_ROLECUSTOM) => roles.Contains(DISCORD_ROLECUSTOM),
                _ => throw new ArgumentException(nameof(roleName))
            };
        }
        #endregion
        #region BotOptions
        /// <summary> Skips creating bots when the program is started; helpful for testing integrations. </summary>
        public bool SKIP_CONSOLE_BOT_CREATION { get; set; } = false;
        public string ISLAND_NAME { get; set; } = "set island name";
        public ushort[] DISABLED_ITEMID_DECVAL { get; set; } = {8760, 2750, 2755, 2756, 2757, 3349, 4310, 4245, 4311, 4411, 4480, 4245, 4645, 5166, 5357, 5358, 5359, 5360, 5361, 5603, 5604, 5605, 5793, 5827, 5828, 5829, 6841, 7211,
8773, 8774, 8775, 8776, 8777, 8778, 8779, 8780, 8781, 9046, 9047, 9048, 9049, 9050, 9051, 9052, 9221, 9616, 9771, 10309, 11140, 12185, 12294, 12327, 12544, 13256, 13257, 2374,
2375, 2381, 2545, 2546, 3076, 3077, 4642, 4643, 4702, 5426, 5427, 6058, 6895, 6896, 9283, 12212, 13105, 4315, 4316, 4317, 5863, 5864, 5865, 5866, 5867, 5868, 5869, 5870,
5912, 5913, 5914, 5915, 5916, 5917, 5918, 5919, 7730, 7731, 7732, 7733, 7734, 7735, 7736, 2579, 2571, 5342, 5343, 5344, 5802, 8661, 8662, 8663, 8664, 8665, 8666, 8667,
8672, 8673, 8674, 8675, 8676, 8677, 8678, 8827, 8828, 8829, 8830, 8831, 8832, 8833, 8897, 8898, 8899, 8900, 8901, 8902, 8903 };

        #endregion
    }
}
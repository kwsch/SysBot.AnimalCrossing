using System;
using SysBot.Base;

namespace CrossBot.SysBot
{
    [Serializable]
    public sealed class BotConfig : SwitchBotConfig
    {
        /// <summary> When enabled, the bot will accept commands from users. </summary>
        public bool AcceptingCommands { get; set; } = true;

        /// <summary> Skips creating bots when the program is started; helpful for testing integrations. </summary>
        public bool SkipConsoleBotCreation { get; set; }

        /// <summary> Offset the items are injected at. This should be the player inventory slot you have currently selected in-game. </summary>
        public uint Offset { get; set; } = 0xABC25840;

        /// <summary> When enabled, the Bot will not allow RAM edits if the player's item metadata is invalid. </summary>
        /// <remarks> Only disable this as a last resort, and you have corrupted your item metadata through other means. </remarks>
        public bool RequireValidInventoryMetadata { get; set; } = true;

        public DropBotConfig DropConfig { get; set; } = new();

        /// <summary> When enabled, users in Discord can request the bot to pick up items (spamming Y a <see cref="DropBotConfig.PickupCount"/> times). </summary>
        public bool AllowClean { get; set; }
    }
}

using System;
using CrossBot.Core;

namespace CrossBot.SysBot
{
    [Serializable]
    public sealed class VillagerConfig : ISlotSetting
    {
        /// <summary> Allows for injecting villagers. </summary>
        public bool AllowVillagerInjection { get; set; }

        /// <summary> Min villager index to allow changing. </summary>
        public int MinVillagerIndex { get; set; } = 0;

        /// <summary> Max villager index to allow changing. </summary>
        public int MaxVillagerIndex { get; set; } = 9;

        /// <summary>
        /// Count of villagers to allow changing.
        /// </summary>
        public int VillagerCount => MaxVillagerIndex - MinVillagerIndex;

        /// <summary> Amount of time a villager must be available for without being overwritten. </summary>
        public int StaleSeconds { get; set; } = 10 * 60; // 10 minutes
    }
}

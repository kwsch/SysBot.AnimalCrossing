using System;

namespace CrossBot.SysBot
{
    [Serializable]
    public class ViewStateConfig
    {
        public bool AllowTeleportation { get; set; } = true;
        public bool DodoCodeRetrieval { get; set; } = true;

        /// <summary> Offset the player coordinate pointer is located at. </summary>
        public string CoordinatePointer { get; set; } = "[[[[main+37D6A40]+18]+178]+D0]+DA";

        public int OverworldLoopCheckDelay { get; set; } = 500;
    }
}

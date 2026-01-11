using System;

namespace CrossBot.SysBot;

[Serializable]
public class ViewStateConfig
{
    /// <summary> Allows moving the bot by changing the coordinates instantaneously. </summary>
    public bool AllowTeleportation { get; set; } = true;

    /// <summary> Allows opening the gates via calculated procedure. </summary>
    public bool DodoCodeRetrieval { get; set; } = true;

    /// <summary> Skips the bot checks for startup, including retrieval of dodo code. </summary>
    public bool SkipSessionCheck { get; set; } = true;

    /// <summary> Delay between overworld state checks. </summary>
    public int OverworldLoopCheckDelay { get; set; } = 500;

    /// <summary> X coordinate of the airport entrance. </summary>
    public ushort AirportX { get; set; } = 0;

    /// <summary> Y coordinate of the airport entrance. </summary>
    /// <remarks> This is actually the Z coordinate, but we don't care about the 3rd dimension (elevation). </remarks>
    public ushort AirportY { get; set; } = 0;

    /// <summary> X coordinate of where you want the bot to drop stuff. Leave 0 unless you want the bot to warp with <see cref="AllowTeleportation"/> rather than warp to the initial position. </summary>
    public ushort DropX { get; set; } = 0;

    /// <summary> Y coordinate of where you want the bot to drop stuff. Leave 0 unless you want the bot to warp with <see cref="AllowTeleportation"/> rather than warp to the initial position. </summary>
    /// <remarks> This is actually the Z coordinate, but we don't care about the 3rd dimension (elevation). </remarks>
    public ushort DropY { get; set; } = 0;
}
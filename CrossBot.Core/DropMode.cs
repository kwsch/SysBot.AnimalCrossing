namespace CrossBot.Core;

public enum DropMode
{
    /// <summary>
    /// Drops items that can normally be dropped.
    /// </summary>
    Legacy,

    /// <summary>
    /// Drops items by overwriting right before attempting to drop a droppable-only item.
    /// </summary>
    SingleDropOptionOverwrite,
}
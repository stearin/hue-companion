namespace HueCompanion.Core.Models;

/// <summary>
/// Represents an item pinned to the custom dashboard.
/// </summary>
public class PinnedItem
{
    /// <summary>
    /// The unique ID of the pinned resource (room, zone, or light).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The type of pinned item.
    /// </summary>
    public PinnedItemType Type { get; set; }

    /// <summary>
    /// Display order (0-based, lower = earlier).
    /// </summary>
    public int Order { get; set; }
}

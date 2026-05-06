namespace LeaveItThere.Addon;

public class FakeItemAddonFlags
{
    /// <summary>
    /// When true, the Move interaction is disabled.
    /// </summary>
    public bool MoveModeDisabled { get; set; } = false;
    /// <summary>
    /// Reason displayed for Move interaction being disabled in context menu
    /// </summary>
    public string MoveModeDisabledReason { get; set; } = "Disabled";
    /// <summary>
    /// When true, the Reclaim interaction is disabled.
    /// </summary>
    public bool ReclaimInteractionDisabled { get; set; } = false;
    /// <summary>
    /// Reason displayed for Reclaim interaction being disabled in context menu
    /// </summary>
    public string ReclaimInteractionDisabledReason { get; set; } = "Disabled";
    /// <summary>
    /// Set to true to make this FakeItem ignore size restrictions and always be have player collision / block bot pathing.
    /// </summary>
    public bool AlwaysPhysical { get; set; } = false;
    /// <summary>
    /// When true, the root BoxCollider component that is added to LootItems will be removed. Useful for when a custom item has a child object defining a custom collider to be used for interactions.
    /// </summary>
    public bool RemoveRootCollider { get; set; } = false;
}

using Content.Shared.Item;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Xenoarchaeology.XenoArtifacts;

/// <summary>
/// This is a prototype for...
/// </summary>
[Prototype("artifactTrigger")]
[DataDefinition]
public sealed partial class ArtifactTriggerPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("components", serverOnly: true)]
    public ComponentRegistry Components = new();

    /// <summary>
    /// The most likely depth this trigger will occur at
    /// </summary>
    [DataField("targetDepth")]
    public float TargetDepth = 0.0f;

    /// <summary>
    /// How many depths above or below the target can the trigger occur; The chance decreases linearly to 0 at the min/max depth
    /// </summary>
    [DataField("depthRange")]
    public float DepthRange = 1.0f;

    /// <summary>
    /// How likely is the trigger to occur at the range it can occur
    /// </summary>
    [DataField("weight")]
    public float Weight = 1.0f;

    [DataField("triggerHint")]
    public string? TriggerHint;

    [DataField("whitelist")]
    public EntityWhitelist? Whitelist;

    [DataField("blacklist")]
    public EntityWhitelist? Blacklist;

    /// <summary>
    /// Artifact types that can have this trigger, leave blank for all
    /// </summary>
    [DataField("originWhitelist")]
    public List<String>? OriginWhitelist;
}

using Content.Shared.Item;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Xenoarchaeology.XenoArtifacts;

public enum ArtiOrigin : byte
{
    Eldritch,
    Martian,
    Precursor,
    Silicon,
    Wizard
}

/// <summary>
/// This is a prototype for...
/// </summary>
[Prototype("artifactEffect")]
[DataDefinition]
public sealed partial class ArtifactEffectPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Components that are added to the artifact when the specfic effect is active.
    /// These are removed after the node is exited and the effect is changed.
    /// </summary>
    [DataField("components", serverOnly: true)]
    public ComponentRegistry Components = new();

    /// <summary>
    /// Components that are permanently added to an entity when the effect's node is entered.
    /// </summary>
    [DataField("permanentComponents")]
    public ComponentRegistry PermanentComponents = new();

    /// <summary>
    /// The most likely depth this effect will occur at
    /// </summary>
    [DataField("targetDepth")]
    public float TargetDepth = 0.0f;

    /// <summary>
    /// How many depths above or below the target can the effect occur; The chance decreases linearly to 0 at the min/max depth
    /// </summary>
    [DataField("depthRange")]
    public float DepthRange = 1.0f;

    /// <summary>
    /// How likely is the effect to occur at the range it can occur; It takes the weight of the trigger preconditions into account already
    /// </summary>
    [DataField("weight")]
    public float Weight = 1.0f;

    [DataField("effectHint")]
    public string? EffectHint;

    [DataField("whitelist")]
    public EntityWhitelist? Whitelist;

    [DataField("blacklist")]
    public EntityWhitelist? Blacklist;

    [DataField("triggerWhitelist", customTypeSerializer: typeof(PrototypeIdSerializer<ArtifactTriggerPrototype>))]
    public List<ArtifactTriggerPrototype>? TriggerWhitelist;

    /// <summary>
    /// Artifact types that can have this effect, leave blank for all
    /// </summary>
    [DataField("originWhitelist")]
    public List<String>? OriginWhitelist;
}

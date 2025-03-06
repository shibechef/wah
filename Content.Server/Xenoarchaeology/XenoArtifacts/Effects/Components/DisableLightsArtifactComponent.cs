using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;

/// <summary>
/// Artifact that disables every powered light in its radius
/// </summary>
public sealed partial class DisableLightsArtifactComponent : Component
{
    [DataField("timeLeft", customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
    [AutoPausedField]
    public TimeSpan NextUpdate;

    /// <summary>
    /// How often it queries for light sources, and how long it disables them
    /// </summary>
    [DataField("updateFrequency")]
    public TimeSpan UpdateFrequency = TimeSpan.FromSeconds(1.5f);

    [DataField("range")]
    public float Range = 8f;
}

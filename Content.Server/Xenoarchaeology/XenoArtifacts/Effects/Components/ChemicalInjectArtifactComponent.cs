namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;

/// <summary>
/// When activated, will shuffle the position of all players
/// within a certain radius.
/// </summary>
[RegisterComponent]
public sealed partial class ChemicalInjectArtifactComponent : Component
{
    [DataField("radius")]
    public float Radius = 6f;

    /// <summary>
    ///     If true, the artifact will only inject whatever entity activated it
    /// </summary>
    [DataField("onlyEffectsActivator")]
    public bool onlyEffectsActivator = false;
}

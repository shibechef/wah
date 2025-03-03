using Content.Shared.Chemistry.Reagent;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;

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
    [DataField("onlyInjectsActivator")]
    public bool OnlyInjectsActivator = false;

    /// <summary>
    ///     How many reagents to choose from in the reagent list
    /// </summary>
    [DataField("reagentCount")]
    public int ReagentCount = 1;

    /// <summary>
    ///     Random reagents to choose from, with a min and max amount for each
    /// </summary>
    [DataField("reagentFill", required: true)]
    public Dictionary<ProtoId<ReagentPrototype>, int[]> ReagentFill = default!;
}

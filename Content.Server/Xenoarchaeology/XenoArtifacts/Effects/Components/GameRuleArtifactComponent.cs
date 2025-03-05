namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;

/// <summary>
/// When activated, will run a gamerule, and cannot be ran again
/// </summary>
[RegisterComponent]
public sealed partial class GameRuleArtifactComponent : Component
{
    /// <summary>
    ///     Game rule that is added when the artifact is triggered
    /// </summary>
    [DataField("addedGameRule", required: true)]
    public string AddedGameRule = "MouseMigration";
}

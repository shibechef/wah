using Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Content.Server.GameTicking;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Systems;

public sealed class GameRuleArtifactSystem : EntitySystem
{
    [Dependency] private readonly ArtifactSystem _artifact = default!;
    [Dependency] public readonly GameTicker GameTicker = default!;

    public const string HasActivated = "hasActivated";
    public override void Initialize()
    {
        SubscribeLocalEvent<GameRuleArtifactComponent, ArtifactActivatedEvent>(OnActivate);
    }

    private void OnActivate(EntityUid uid, GameRuleArtifactComponent component, ArtifactActivatedEvent args)
    {
        if (!_artifact.TryGetNodeData(uid, HasActivated, out bool hasActivated))
        {
            _artifact.SetNodeData(uid, HasActivated, false);
        }
        if (hasActivated)
            return;
        GameTicker.AddGameRule(component.AddedGameRule);
        _artifact.SetNodeData(uid, HasActivated, true);
    }
}

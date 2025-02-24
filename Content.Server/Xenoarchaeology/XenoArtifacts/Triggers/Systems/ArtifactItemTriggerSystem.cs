using Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Components;
using Content.Shared.Interaction;
using Content.Shared.Whitelist;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Systems;

public sealed class ArtifactItemTriggerSystem : EntitySystem
{
    [Dependency] private readonly ArtifactSystem _artifactSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ArtifactItemTriggerComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(EntityUid uid, ArtifactItemTriggerComponent component, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (_whitelistSystem.IsWhitelistPass(component.Whitelist, args.Used))
            _artifactSystem.TryActivateArtifact(uid, args.User);
    }
}

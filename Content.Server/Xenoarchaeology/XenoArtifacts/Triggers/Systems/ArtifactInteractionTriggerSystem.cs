using Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Components;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Systems;

public sealed class ArtifactInteractionTriggerSystem : EntitySystem
{
    [Dependency] private readonly ArtifactSystem _artifactSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ArtifactInteractionTriggerComponent, PullStartedMessage>(OnPull);
        SubscribeLocalEvent<ArtifactInteractionTriggerComponent, AttackedEvent>(OnAttack);
        SubscribeLocalEvent<ArtifactInteractionTriggerComponent, InteractHandEvent>(OnInteract);
    }

    private void OnPull(EntityUid uid, ArtifactInteractionTriggerComponent component, PullStartedMessage args)
    {
        if (!component.PullActivation)
            return;

        if (!_whitelistSystem.IsWhitelistPassOrNull(component.EntityWhitelist, args.PullerUid))
            return;

        _artifactSystem.TryActivateArtifact(uid, args.PullerUid);
    }

    private void OnAttack(EntityUid uid, ArtifactInteractionTriggerComponent component, AttackedEvent args)
    {
        if (!component.AttackActivation)
            return;

        if (!_whitelistSystem.IsWhitelistPassOrNull(component.EntityWhitelist, args.User))
            return;

        _artifactSystem.TryActivateArtifact(uid, args.User);
    }

    private void OnInteract(EntityUid uid, ArtifactInteractionTriggerComponent component, InteractHandEvent args)
    {
        if (args.Handled)
            return;

        if (!component.EmptyHandActivation)
            return;

        if (!_whitelistSystem.IsWhitelistPassOrNull(component.EntityWhitelist, args.User))
            return;

        args.Handled = _artifactSystem.TryActivateArtifact(uid, args.User);
    }
}

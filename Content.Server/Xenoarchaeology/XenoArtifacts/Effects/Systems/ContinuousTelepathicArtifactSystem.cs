using Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Shared.Hands;
using Content.Shared.Popups;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Systems;

public sealed class ContinuousTelepathicArtifactSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ContinuousTelepathicArtifactComponent, GotEquippedHandEvent>(OnGotEquippedHand);
        SubscribeLocalEvent<ContinuousTelepathicArtifactComponent, GotUnequippedHandEvent>(OnGotUnequippedHand);
    }

    private void OnGotEquippedHand(Entity<ContinuousTelepathicArtifactComponent> ent, ref GotEquippedHandEvent args)
    {
        ent.Comp.NextMessageTime = _gameTiming.CurTime + TimeSpan.FromSeconds(_random.NextDouble(ent.Comp.MinTime.TotalSeconds, ent.Comp.MaxTime.TotalSeconds));
        ent.Comp.Holder = args.User;
        ent.Comp.ActivelyHeld = true;
    }

    private void OnGotUnequippedHand(Entity<ContinuousTelepathicArtifactComponent> ent, ref GotUnequippedHandEvent args)
    {
        ent.Comp.ActivelyHeld = false;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ContinuousTelepathicArtifactComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (_gameTiming.CurTime > comp.NextMessageTime && comp.ActivelyHeld)
                SendMessage(uid, comp);
        }
    }

    private void SendMessage(EntityUid uid, ContinuousTelepathicArtifactComponent comp)
    {
        var messageId = _random.Pick(comp.Messages);
        var message = Loc.GetString(messageId);
        _popupSystem.PopupEntity(message, comp.Holder, comp.Holder);
        comp.NextMessageTime = _gameTiming.CurTime + TimeSpan.FromSeconds(_random.NextDouble(comp.MinTime.TotalSeconds, comp.MaxTime.TotalSeconds));
    }
}

using System.Linq;
using Content.Server.Light.Components;
using Content.Server.Power.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Content.Shared.Light.Components;
using Content.Shared.PowerCell.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Systems;

public sealed class DisableLightsArtifactSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    private void OnInit(Entity<DisableLightsArtifactComponent> ent, ref ComponentInit args)
    {
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.UpdateFrequency;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DisabledLightComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.DisabledUntil < _timing.CurTime)
            {
                RemComp<DisabledLightComponent>(uid);
                var ev = new DisabledLightComponent();
                RaiseLocalEvent(uid, ref ev);
            }
        }

        var query2 = EntityQueryEnumerator<DisableLightsArtifactComponent>();
        while (query2.MoveNext(out var uid, out var comp))
        {
            if (comp.NextUpdate < _timing.CurTime)
            {
                FindLights(uid, comp);
            }
        }
    }

    //PoweredLight for wall lights
    //HandheldLight + PowerCellSlot for most items
    //HandheldLight + Battery for hardsuit helmets
    //LitOnPowered for vending machines and arcades
    private void FindLights(EntityUid uid, DisableLightsArtifactComponent component)
    {
        var disableTime = component.UpdateFrequency * 1.1f;
        var ents = _entityLookup.GetEntitiesInRange(_transform.GetMapCoordinates(uid), component.Range, LookupFlags.Uncontained);
        foreach (var ent in ents)
        {
            if (HasComp<PoweredLightComponent>(ent) || HasComp<LitOnPoweredComponent>(ent))
            {
                var disabled = EnsureComp<DisabledLightComponent>(ent);
                disabled.DisabledUntil = _timing.CurTime + disableTime;
            }
            else if (HasComp<HandheldLightComponent>(ent) && (HasComp<BatteryComponent>(ent) || HasComp<PowerCellSlotComponent>(ent)))
            {
                var disabled = EnsureComp<DisabledLightComponent>(ent);
                disabled.DisabledUntil = _timing.CurTime + disableTime;
            }
        }
    }
}

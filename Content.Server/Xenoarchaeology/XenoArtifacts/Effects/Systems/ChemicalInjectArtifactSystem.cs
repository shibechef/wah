using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Systems;

public sealed class ChemicalInjectArtifactSystem : EntitySystem
{
    [Dependency] private readonly ArtifactSystem _artifact = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    /// <summary>
    /// The key to get the node's constant chem data
    /// </summary>
    public const string NodeDataChemicalList = "nodeDataChemicalList";

    private EntityQuery<InjectableSolutionComponent> _injectableQuery;

    public override void Initialize()
    {
        SubscribeLocalEvent<ChemicalInjectArtifactComponent, ArtifactActivatedEvent>(OnActivated);

        _injectableQuery = GetEntityQuery<InjectableSolutionComponent>();
    }

    private void OnActivated(EntityUid uid, ChemicalInjectArtifactComponent component, ArtifactActivatedEvent args)
    {
        if (!TryComp<ArtifactComponent>(uid, out var artifact))
            return;

        //if the chemicals haven't already been chosen, choose them, and their amounts
        if (!_artifact.TryGetNodeData(uid, NodeDataChemicalList, out List<(ProtoId<ReagentPrototype>, int)>? chems, artifact))
        {
            chems = new();
            var reagentsChosen = _random.GetItems(component.ReagentFill.ToList(), component.ReagentCount, false);

            foreach (var reagent in reagentsChosen)
            {
                var amount = _random.Next(reagent.Value[0], reagent.Value[1]);
                chems.Add((reagent.Key, amount));
            }

            _artifact.SetNodeData(uid, NodeDataChemicalList, chems, artifact);
        }

        var entsToInject = new List<EntityUid>();
        if (component.OnlyInjectsActivator)
        {
            //only get the person activating it
            if (args.Activator != null && _entManager.HasComponent<InjectableSolutionComponent>(args.Activator))
                entsToInject.Add(args.Activator.Value);
        }
        else
        {
            //get everything injectable in the radius
            var xformQuery = GetEntityQuery<TransformComponent>();
            var xform = xformQuery.GetComponent(uid);
            entsToInject = _lookup.GetEntitiesInRange<InjectableSolutionComponent>(_transform.GetMapCoordinates(uid, xform: xform), component.Radius).Select(x => x.Owner).ToList();
        }

        //inject everyone chosen with the reagents
        foreach (var ent in entsToInject)
        {
            if (!_solutionContainer.TryGetInjectableSolution(ent, out var injectable, out _))
                continue;

            foreach (var chem in chems)
            {
                if (_injectableQuery.TryGetComponent(ent, out var injEnt))
                {
                    _solutionContainer.TryAddReagent(injectable.Value, chem.Item1, chem.Item2, out _);
                }

            }
        }
    }
}

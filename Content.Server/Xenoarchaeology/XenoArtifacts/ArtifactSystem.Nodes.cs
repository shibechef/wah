using System;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Content.Shared.Whitelist;
using Content.Shared.Xenoarchaeology.XenoArtifacts;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Robust.Shared.Random;

namespace Content.Server.Xenoarchaeology.XenoArtifacts;

public sealed partial class ArtifactSystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    private const int MaxEdgesPerNode = 4;

    private readonly HashSet<int> _usedNodeIds = new();

    /// <summary>
    /// Generate an Artifact tree with fully developed nodes.
    /// </summary>
    /// <param name="artifact"></param>
    /// <param name="allNodes"></param>
    /// <param name="nodesToCreate">The amount of nodes it has.</param>
    private void GenerateArtifactNodeTree(EntityUid artifact, List<ArtifactNode> allNodes, int nodesToCreate)
    {
        if (nodesToCreate < 1)
        {
            Log.Error($"nodesToCreate {nodesToCreate} is less than 1. Aborting artifact tree generation.");
            return;
        }

        _usedNodeIds.Clear();

        var uninitializedNodes = new List<ArtifactNode> { new(){ Id = GetValidNodeId() } };
        var createdNodes = 1;

        while (uninitializedNodes.Count > 0)
        {
            var node = uninitializedNodes[0];
            uninitializedNodes.Remove(node);

            node.Trigger = GetRandomTrigger(artifact, ref node);
            node.Effect = GetRandomEffect(artifact, ref node);

            var maxChildren = _random.Next(1, MaxEdgesPerNode - 1);

            for (var i = 0; i < maxChildren; i++)
            {
                if (nodesToCreate <= createdNodes)
                {
                    break;
                }

                var child = new ArtifactNode {Id = GetValidNodeId(), Depth = node.Depth + 1};
                node.Edges.Add(child.Id);
                child.Edges.Add(node.Id);

                uninitializedNodes.Add(child);
                createdNodes++;
            }

            allNodes.Add(node);
        }
    }

    private int GetValidNodeId()
    {
        var id = _random.Next(100, 1000);
        while (_usedNodeIds.Contains(id))
        {
            id = _random.Next(100, 1000);
        }

        _usedNodeIds.Add(id);

        return id;
    }

    //yeah these two functions are near duplicates but i don't
    //want to implement an interface or abstract parent

    private string GetRandomTrigger(EntityUid artifact, ref ArtifactNode node)
    {
        var allTriggers = _prototype.EnumeratePrototypes<ArtifactTriggerPrototype>()
            .Where(x => _whitelistSystem.IsWhitelistPassOrNull(x.Whitelist, artifact) &&
            _whitelistSystem.IsBlacklistFailOrNull(x.Blacklist, artifact) &&
            IsTriggerValid(EntityManager.GetComponent<ArtifactComponent>(artifact), x)).ToList();

        //get the sum of weights
        var sum = 0.0f;
        foreach (var trigger in allTriggers)
        {
            sum += GetTriggerWeight(trigger, node.Depth);
        }

        //get random from a weight
        var random = _random.NextFloat(0.0f, sum);
        foreach (var trigger in allTriggers)
        {
            random -= GetTriggerWeight(trigger, node.Depth);
            if (random < 0.0f)
                return trigger.ID;
        }

        //failsafe
        return _random.Pick(allTriggers).ID;
    }

    private string GetRandomEffect(EntityUid artifact, ref ArtifactNode node)
    {
        var trigger = _prototype.Index<ArtifactTriggerPrototype>(node.Trigger);
        var allEffects = _prototype.EnumeratePrototypes<ArtifactEffectPrototype>()
            .Where(x => _whitelistSystem.IsWhitelistPassOrNull(x.Whitelist, artifact) &&
            _whitelistSystem.IsBlacklistFailOrNull(x.Blacklist, artifact) &&
            IsEffectValid(EntityManager.GetComponent<ArtifactComponent>(artifact), trigger, x)).ToList();

        //get the sum of weights
        var sum = 0.0f;
        foreach (var effect in allEffects)
        {
            sum += GetEffectWeight(effect, node.Depth);
        }

        //get random from a weight
        var random = _random.NextFloat(0.0f, sum);
        foreach (var effect in allEffects)
        {
            random -= GetEffectWeight(effect, node.Depth);
            if (random < 0.0f)
                return effect.ID;
        }

        //failsafe
        return _random.Pick(allEffects).ID;

    }

    //gets the weight of a trigger at a depth
    //It is 100% of the weight at the target depth, and decreases to 0% of the weight at the edge
    private float GetTriggerWeight(ArtifactTriggerPrototype trigger, int depth)
    {
        return trigger.Weight * (1.0f - MathF.Abs(depth - trigger.TargetDepth) / (trigger.DepthRange + 1.0f));
    }

    //gets the weight of a trigger at a depth
    //It is 100% of the weight at the target depth, and decreases to 0% of the weight at the edge
    //The weight also takes into account how likely the trigger precondition is to occur
    private float GetEffectWeight(ArtifactEffectPrototype effect, int depth)
    {
        var preconditionWeight = 1.0f;
        if (effect.TriggerWhitelist != null)
        {
            foreach (var trigger in effect.TriggerWhitelist)
            {
                preconditionWeight += _prototype.Index<ArtifactTriggerPrototype>(trigger).Weight;
            }
        }
        return effect.Weight * (1.0f - MathF.Abs(depth - effect.TargetDepth) / (effect.DepthRange + 1.0f)) / preconditionWeight;
    }

    /// <summary>
    /// Enter a node: attach the relevant components
    /// </summary>
    private void EnterNode(EntityUid uid, ref ArtifactNode node, ArtifactComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.CurrentNodeId != null)
        {
            ExitNode(uid, component);
        }

        component.CurrentNodeId = node.Id;

        var trigger = _prototype.Index<ArtifactTriggerPrototype>(node.Trigger);
        var effect = _prototype.Index<ArtifactEffectPrototype>(node.Effect);

        var allComponents = effect.Components.Concat(effect.PermanentComponents).Concat(trigger.Components);
        foreach (var (name, entry) in allComponents)
        {
            var reg = _componentFactory.GetRegistration(name);

            if (node.Discovered && EntityManager.HasComponent(uid, reg.Type))
            {
                // Don't re-add permanent components unless this is the first time you've entered this node
                if (effect.PermanentComponents.ContainsKey(name))
                    continue;

                EntityManager.RemoveComponent(uid, reg.Type);
            }

            var comp = (Component)_componentFactory.GetComponent(reg);

            var temp = (object)comp;
            _serialization.CopyTo(entry.Component, ref temp);
            EntityManager.RemoveComponent(uid, temp!.GetType());
            EntityManager.AddComponent(uid, (Component)temp!);
        }

        node.Discovered = true;
        RaiseLocalEvent(uid, new ArtifactNodeEnteredEvent(component.CurrentNodeId.Value));
    }

    /// <summary>
    /// Exit a node: remove the relevant components.
    /// </summary>
    private void ExitNode(EntityUid uid, ArtifactComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.CurrentNodeId == null)
            return;
        var currentNode = GetNodeFromId(component.CurrentNodeId.Value, component);

        var trigger = _prototype.Index<ArtifactTriggerPrototype>(currentNode.Trigger);
        var effect = _prototype.Index<ArtifactEffectPrototype>(currentNode.Effect);

        var entityPrototype = MetaData(uid).EntityPrototype;
        var toRemove = effect.Components.Keys.Concat(trigger.Components.Keys).ToList();

        foreach (var name in toRemove)
        {
            // if the entity prototype contained the component originally
            if (entityPrototype?.Components.TryGetComponent(name, out var entry) ?? false)
            {
                var comp = (Component)_componentFactory.GetComponent(name);
                var temp = (object)comp;
                _serialization.CopyTo(entry, ref temp);
                EntityManager.RemoveComponent(uid, temp!.GetType());
                EntityManager.AddComponent(uid, (Component)temp);
                continue;
            }

            EntityManager.RemoveComponentDeferred(uid, _componentFactory.GetRegistration(name).Type);
        }
        component.CurrentNodeId = null;
    }

    [PublicAPI]
    public ArtifactNode GetNodeFromId(int id, ArtifactComponent component)
    {
        return component.NodeTree.First(x => x.Id == id);
    }

    [PublicAPI]
    public ArtifactNode GetNodeFromId(int id, IEnumerable<ArtifactNode> nodes)
    {
        return nodes.First(x => x.Id == id);
    }

    private bool IsTriggerValid(ArtifactComponent artifact, ArtifactTriggerPrototype artiTrigger)
    {
        if (artiTrigger.OriginWhitelist != null && !artiTrigger.OriginWhitelist.Contains(artifact.ArtiType.ToString()))
            return false;
        return true;
    }

    private bool IsEffectValid(ArtifactComponent artifact, ArtifactTriggerPrototype artifactTrigger, ArtifactEffectPrototype artiEffect)
    {
        if (artiEffect.OriginWhitelist != null && !artiEffect.OriginWhitelist.Contains(artifact.ArtiType.ToString()))
            return false;
        if (artiEffect.TriggerWhitelist != null && !artiEffect.TriggerWhitelist.Contains(artifactTrigger))
            return false;
        return true;
    }
}

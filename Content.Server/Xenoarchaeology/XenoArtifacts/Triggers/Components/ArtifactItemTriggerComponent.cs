using Content.Shared.Tag;
﻿using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using YamlDotNet.Core.Tokens;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Components;

/// <summary>
///     Activate artifact by using an item on it
/// </summary>
[RegisterComponent]
public sealed partial class ArtifactItemTriggerComponent : Component
{
    /// <summary>
    ///     What items can trigger this artifact
    /// </summary>
    [DataField("whitelist")]
    public EntityWhitelist? Whitelist;
}

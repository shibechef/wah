using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;

/// <summary>
///     Artifact that occassionally speaks when being held
/// </summary>
[RegisterComponent]
public sealed partial class ContinuousTelepathicArtifactComponent : Component
{
    /// <summary>
    ///     Loc string ids of telepathic messages.
    ///     Will be randomly picked and shown to player.
    /// </summary>
    [DataField("messages")]
    [ViewVariables(VVAccess.ReadWrite)]
    public List<string> Messages = default!;

    /// <summary>
    ///     Max time between messages
    /// </summary>
    [DataField("maxTime")]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan MaxTime = TimeSpan.FromSeconds(8);

    /// <summary>
    ///     Min time between messages
    /// </summary>
    [DataField("minTime")]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan MinTime = TimeSpan.FromSeconds(4);

    /// <summary>
    ///     The next time a message will be sent
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextMessageTime;

    /// <summary>
    ///     Is the artifact actively being held
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool ActivelyHeld = false;

    /// <summary>
    ///     The UID of who is holding the artifact
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid Holder;

}

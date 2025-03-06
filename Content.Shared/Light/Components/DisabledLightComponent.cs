using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Light.Components;

/// <summary>
/// While any light has this component, turn it off in its own systems
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DisabledLightComponent : Component
{
    [DataField("timeLeft", customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
    [AutoPausedField]
    public TimeSpan DisabledUntil;
}

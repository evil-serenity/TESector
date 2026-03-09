using Content.Server._TE.Laws.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._TE.Laws.Components;

[RegisterComponent, AutoGenerateComponentPause]
[Access(typeof(LawsSystem))]
public sealed partial class LawsWriterComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool PublishEnabled;

    [ViewVariables(VVAccess.ReadWrite), DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextPublish;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float PublishCooldown = 20f;

    [DataField]
    public SoundSpecifier NoAccessSound = new SoundPathSpecifier("/Audio/Machines/airlock_deny.ogg");

    [DataField]
    public SoundSpecifier ConfirmSound = new SoundPathSpecifier("/Audio/Machines/scan_finish.ogg");

    /// <summary>
    /// This stores the working content of the current article
    /// </summary>
    [DataField, ViewVariables]
    public string DraftContent = "";
}

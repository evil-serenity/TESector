using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared._TE.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class LawsReaderUiMessageEvent : CartridgeMessageEvent
{
    public readonly LawsReaderUiAction Action;

    public LawsReaderUiMessageEvent(LawsReaderUiAction action)
    {
        Action = action;
    }
}

[Serializable, NetSerializable]
public enum LawsReaderUiAction
{
    Next,
    Prev,
    NotificationSwitch
}

using Content.Client.UserInterface.Fragments;
using Content.Shared._TE.CartridgeLoader.Cartridges;
using Content.Shared.CartridgeLoader;
using Robust.Client.UserInterface;

namespace Content.Client.CartridgeLoader.Cartridges;

public sealed partial class LawsReaderUi : UIFragment
{
    private LawsReaderUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new LawsReaderUiFragment();

        _fragment.OnNextButtonPressed += () =>
        {
            SendLawsReaderMessage(LawsReaderUiAction.Next, userInterface);
        };
        _fragment.OnPrevButtonPressed += () =>
        {
            SendLawsReaderMessage(LawsReaderUiAction.Prev, userInterface);
        };
        _fragment.OnNotificationSwithPressed += () =>
        {
            SendLawsReaderMessage(LawsReaderUiAction.NotificationSwitch, userInterface);
        };
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        switch (state)
        {
            case LawsReaderBoundUserInterfaceState cast:
                _fragment?.UpdateState(cast.Article, cast.TargetNum, cast.TotalNum, cast.NotificationOn);
                break;
            case LawsReaderEmptyBoundUserInterfaceState empty:
                _fragment?.UpdateEmptyState(empty.NotificationOn);
                break;
        }
    }

    private void SendLawsReaderMessage(LawsReaderUiAction action, BoundUserInterface userInterface)
    {
        var lawMessage = new LawsReaderUiMessageEvent(action);
        var message = new CartridgeUiMessage(lawMessage);
        userInterface.SendMessage(message);
    }
}

using Content.Shared._TE.Laws.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared._TE.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class LawsReaderBoundUserInterfaceState : BoundUserInterfaceState
{
    public LawArticle Article;
    public int TargetNum;
    public int TotalNum;
    public bool NotificationOn;

    public LawsReaderBoundUserInterfaceState(LawArticle article, int targetNum, int totalNum, bool notificationOn)
    {
        Article = article;
        TargetNum = targetNum;
        TotalNum = totalNum;
        NotificationOn = notificationOn;
    }
}

[Serializable, NetSerializable]
public sealed class LawsReaderEmptyBoundUserInterfaceState : BoundUserInterfaceState
{
    public bool NotificationOn;

    public LawsReaderEmptyBoundUserInterfaceState(bool notificationOn)
    {
        NotificationOn = notificationOn;
    }
}

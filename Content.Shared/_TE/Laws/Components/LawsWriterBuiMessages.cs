using Content.Shared._TE.Laws.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared._TE.Laws.Components;

[Serializable, NetSerializable]
public enum LawsWriterUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class LawsWriterBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly LawArticle[] Articles;
    public readonly bool PublishEnabled;
    public readonly TimeSpan NextPublish;
    public readonly string DraftContent;

    public LawsWriterBoundUserInterfaceState(LawArticle[] articles, bool publishEnabled, TimeSpan nextPublish, string draftContent)
    {
        Articles = articles;
        PublishEnabled = publishEnabled;
        NextPublish = nextPublish;
        DraftContent = draftContent;
    }
}

[Serializable, NetSerializable]
public sealed class LawsWriterPublishMessage : BoundUserInterfaceMessage
{
    public readonly string Content;


    public LawsWriterPublishMessage(string content)
    {
        Content = content;
    }
}

[Serializable, NetSerializable]
public sealed class LawsWriterDeleteMessage : BoundUserInterfaceMessage
{
    public readonly int ArticleNum;

    public LawsWriterDeleteMessage(int num)
    {
        ArticleNum = num;
    }
}

[Serializable, NetSerializable]
public sealed class LawsWriterArticlesRequestMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class LawsWriterSaveDraftMessage : BoundUserInterfaceMessage
{
    public readonly string DraftContent;

    public LawsWriterSaveDraftMessage(string draftContent)
    {
        DraftContent = draftContent;
    }
}

[Serializable, NetSerializable]
public sealed class LawsWriterRequestDraftMessage : BoundUserInterfaceMessage
{
}

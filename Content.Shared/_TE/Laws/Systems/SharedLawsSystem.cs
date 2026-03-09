using Robust.Shared.Serialization;

namespace Content.Shared._TE.Laws.Systems;

public abstract class SharedLawsSystem : EntitySystem
{
    public const int MaxContentLength = 2048;
}

[Serializable, NetSerializable]
public struct LawArticle
{
    [ViewVariables(VVAccess.ReadWrite)]
    public string Content;

    [ViewVariables(VVAccess.ReadWrite)]
    public string? Author;

    [ViewVariables]
    public ICollection<(NetEntity, uint)>? AuthorStationRecordKeyIds;

    [ViewVariables]
    public TimeSpan ShareTime;
}

[ByRefEvent]
public record struct LawArticlePublishedEvent(LawArticle Article);

[ByRefEvent]
public record struct LawArticleDeletedEvent;

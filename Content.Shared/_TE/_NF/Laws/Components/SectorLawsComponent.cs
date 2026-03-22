using Content.Shared._TE.Laws.Systems;

namespace Content.Shared._TE._NF.Laws.Components;

[RegisterComponent]
public sealed partial class SectorLawsComponent : Component
{
    public static List<LawArticle> Articles = new();
}

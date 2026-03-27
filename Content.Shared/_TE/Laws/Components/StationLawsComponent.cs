using Content.Shared._TE.Laws.Systems;

namespace Content.Shared._TE.Laws.Components;

[RegisterComponent]
public sealed partial class StationLawsComponent : Component
{
    [DataField]
    public List<LawArticle> Laws = new();
}

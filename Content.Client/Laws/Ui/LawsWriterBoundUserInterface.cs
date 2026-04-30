using Content.Shared._TE.Laws.Components;
using Content.Shared._TE.Laws.Systems;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Utility;

namespace Content.Client.Laws.Ui;

[UsedImplicitly]
public sealed class LawsWriterBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private LawWriterMenu? _menu;

    public LawsWriterBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {

    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<LawWriterMenu>();

        _menu.LawArticleEditorPanel.PublishButtonPressed += OnPublishButtonPressed;
        _menu.DeleteButtonPressed += OnDeleteButtonPressed;

        _menu.CreateButtonPressed += OnCreateButtonPressed;
        _menu.LawArticleEditorPanel.ArticleDraftUpdated += OnArticleDraftUpdated;

        SendMessage(new LawsWriterArticlesRequestMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not LawsWriterBoundUserInterfaceState cast)
            return;

        _menu?.UpdateUI(cast.Articles, cast.PublishEnabled, cast.NextPublish, cast.DraftContent);
    }

    private void OnPublishButtonPressed()
    {
        if (_menu == null)
            return;

        var stringContent = Rope.Collapse(_menu.LawArticleEditorPanel.ContentField.TextRope).Trim();

        if (stringContent.Length == 0)
            return;

        var content = stringContent.Length <= SharedLawsSystem.MaxContentLength
            ? stringContent
            : $"{stringContent[..(SharedLawsSystem.MaxContentLength - 3)]}...";


        SendMessage(new LawsWriterPublishMessage(content));
    }

    private void OnDeleteButtonPressed(int articleNum)
    {
        if (_menu == null)
            return;

        SendMessage(new LawsWriterDeleteMessage(articleNum));
    }

    private void OnCreateButtonPressed()
    {
        SendMessage(new LawsWriterRequestDraftMessage());
    }

    private void OnArticleDraftUpdated(string content)
    {
        SendMessage(new LawsWriterSaveDraftMessage(content));
    }
}

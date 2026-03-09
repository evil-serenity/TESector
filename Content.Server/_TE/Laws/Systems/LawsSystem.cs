using System.Diagnostics.CodeAnalysis;
using Content.Server._TE.Laws.Components;
using Content.Server.Administration.Logs;
using Content.Server.CartridgeLoader;
using Content.Server.CartridgeLoader.Cartridges;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.Popups;
using Content.Shared._TE._NF.Laws.Components;
using Content.Shared._TE.CartridgeLoader.Cartridges;
using Content.Shared._TE.Laws.Components;
using Content.Shared._TE.Laws.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.CartridgeLoader;
using Content.Shared.Database;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Content.Shared.IdentityManagement;
using Robust.Shared.Timing;
using Content.Shared.GameTicking;

namespace Content.Server._TE.Laws.Systems;

public sealed class LawsSystem : SharedLawsSystem
{
    [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoaderSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);

        // New writer bui messages
        Subs.BuiEvents<LawsWriterComponent>(LawsWriterUiKey.Key, subs =>
        {
            subs.Event<LawsWriterDeleteMessage>(OnWriteUiDeleteMessage);
            subs.Event<LawsWriterArticlesRequestMessage>(OnRequestArticlesUiMessage);
            subs.Event<LawsWriterPublishMessage>(OnWriteUiPublishMessage);
            subs.Event<LawsWriterSaveDraftMessage>(OnLawsWriterDraftUpdatedMessage);
            subs.Event<LawsWriterRequestDraftMessage>(OnRequestArticleDraftMessage);
        });

        // Laws reader
        SubscribeLocalEvent<LawsReaderCartridgeComponent, LawArticlePublishedEvent>(OnArticlePublished);
        SubscribeLocalEvent<LawsReaderCartridgeComponent, LawArticleDeletedEvent>(OnArticleDeleted);
        SubscribeLocalEvent<LawsReaderCartridgeComponent, CartridgeMessageEvent>(OnReaderUiMessage);
        SubscribeLocalEvent<LawsReaderCartridgeComponent, CartridgeUiReadyEvent>(OnReaderUiReady);
    }

    // Frontier: article lifecycle management
    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        // A new round is starting, clear any articles from the previous round.
        SectorLawsComponent.Articles.Clear();
    }
    // End Frontier

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<LawsWriterComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.PublishEnabled || _timing.CurTime < comp.NextPublish)
                continue;

            comp.PublishEnabled = true;
            UpdateWriterUi((uid, comp));
        }
    }

    #region Writer Event Handlers

    private void OnWriteUiDeleteMessage(Entity<LawsWriterComponent> ent, ref LawsWriterDeleteMessage msg)
    {
        if (!TryGetArticles(ent, out var articles))
            return;

        if (msg.ArticleNum >= articles.Count)
            return;

        var article = articles[msg.ArticleNum];
        if (CanUse(msg.Actor, ent.Owner))
        {
            _adminLogger.Add(
                LogType.Chat,
                LogImpact.Medium,
                $"{ToPrettyString(msg.Actor):actor} deleted law by {article.Author}: {article.Content}"
                );

            articles.RemoveAt(msg.ArticleNum);
            _audio.PlayPvs(ent.Comp.ConfirmSound, ent);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("laws-write-no-access-popup"), ent, PopupType.SmallCaution);
            _audio.PlayPvs(ent.Comp.NoAccessSound, ent);
        }

        var args = new LawArticleDeletedEvent();
        var query = EntityQueryEnumerator<LawsReaderCartridgeComponent>();
        while (query.MoveNext(out var readerUid, out _))
        {
            RaiseLocalEvent(readerUid, ref args);
        }

        UpdateWriterDevices();
    }

    private void OnRequestArticlesUiMessage(Entity<LawsWriterComponent> ent, ref LawsWriterArticlesRequestMessage msg)
    {
        UpdateWriterUi(ent);
    }

    private void OnWriteUiPublishMessage(Entity<LawsWriterComponent> ent, ref LawsWriterPublishMessage msg)
    {
        if (!ent.Comp.PublishEnabled)
            return;

        if (!TryGetArticles(ent, out var articles))
        {
            Log.Error("OnWriteUiPublishMessage: no articles!");
            return;
        }

        if (!CanUse(msg.Actor, ent.Owner))
        {
            _popup.PopupEntity(Loc.GetString("laws-write-no-access-popup"), ent, PopupType.SmallCaution);
            _audio.PlayPvs(ent.Comp.NoAccessSound, ent);
            return;
        }

        ent.Comp.PublishEnabled = false;
        ent.Comp.NextPublish = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.PublishCooldown);

        var tryGetIdentityShortInfoEvent = new TryGetIdentityShortInfoEvent(ent, msg.Actor);
        RaiseLocalEvent(tryGetIdentityShortInfoEvent);
        string? authorName = tryGetIdentityShortInfoEvent.Title;

        var content = msg.Content.Trim();

        var article = new LawArticle
        {
            Content = content.Length <= MaxContentLength ? content : $"{content[..MaxContentLength]}...",
            Author = authorName,
            ShareTime = _ticker.RoundDuration()
        };

        _audio.PlayPvs(ent.Comp.ConfirmSound, ent);

        _adminLogger.Add(
            LogType.Chat,
            LogImpact.Medium,
            $"{ToPrettyString(msg.Actor):actor} created law by {article.Author}: {article.Content}"
            );

        _chatManager.SendAdminAnnouncement(Loc.GetString("laws-publish-admin-announcement",
            ("actor", msg.Actor),
            ("title", article.Content.Length <= 30 ? article.Content : $"{article.Content[..30]}..."),
            ("author", article.Author ?? Loc.GetString("news-read-ui-no-author"))
            ));

        articles.Add(article);

        var args = new LawArticlePublishedEvent(article);
        var query = EntityQueryEnumerator<LawsReaderCartridgeComponent>();
        while (query.MoveNext(out var readerUid, out _))
        {
            RaiseLocalEvent(readerUid, ref args);
        }

        UpdateWriterDevices();
    }
    #endregion

    #region Reader Event Handlers

    private void OnArticlePublished(Entity<LawsReaderCartridgeComponent> ent, ref LawArticlePublishedEvent args)
    {
        if (Comp<CartridgeComponent>(ent).LoaderUid is not { } loaderUid)
            return;

        UpdateReaderUi(ent, loaderUid);

        if (!ent.Comp.NotificationOn)
            return;

        _cartridgeLoaderSystem.SendNotification(
            loaderUid,
            Loc.GetString("laws-pda-notification-header"),
            args.Article.Content.Length <= 30 ? args.Article.Content : $"{args.Article.Content[..30]}...");
    }

    private void OnArticleDeleted(Entity<LawsReaderCartridgeComponent> ent, ref LawArticleDeletedEvent args)
    {
        if (Comp<CartridgeComponent>(ent).LoaderUid is not { } loaderUid)
            return;

        UpdateReaderUi(ent, loaderUid);
    }

    private void OnReaderUiMessage(Entity<LawsReaderCartridgeComponent> ent, ref CartridgeMessageEvent args)
    {
        if (args is not LawsReaderUiMessageEvent message)
            return;

        switch (message.Action)
        {
            case LawsReaderUiAction.Next:
                LawsReaderLeafArticle(ent, 1);
                break;
            case LawsReaderUiAction.Prev:
                LawsReaderLeafArticle(ent, -1);
                break;
            case LawsReaderUiAction.NotificationSwitch:
                ent.Comp.NotificationOn = !ent.Comp.NotificationOn;
                break;
        }

        UpdateReaderUi(ent, GetEntity(args.LoaderUid));
    }

    private void OnReaderUiReady(Entity<LawsReaderCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        UpdateReaderUi(ent, args.Loader);
    }
    #endregion

    private bool TryGetArticles(EntityUid uid, [NotNullWhen(true)] out List<LawArticle>? articles)
    {
        // Any SectorLawsComponent will have a complete article set, we ensure one exists before returning the complete set.
        var query = EntityQueryEnumerator<SectorLawsComponent>();
        if (query.MoveNext(out var _)) {
            articles = SectorLawsComponent.Articles;
            return true;
        }
        articles = null;
        return false;
        // End Frontier
    }

    private void UpdateWriterUi(Entity<LawsWriterComponent> ent)
    {
        if (!_ui.HasUi(ent, LawsWriterUiKey.Key))
            return;

        if (!TryGetArticles(ent, out var articles))
            return;

        var state = new LawsWriterBoundUserInterfaceState(articles.ToArray(), ent.Comp.PublishEnabled, ent.Comp.NextPublish, ent.Comp.DraftContent);
        _ui.SetUiState(ent.Owner, LawsWriterUiKey.Key, state);
    }

    private void UpdateReaderUi(Entity<LawsReaderCartridgeComponent> ent, EntityUid loaderUid)
    {
        if (!TryGetArticles(ent, out var articles))
            return;

        LawsReaderLeafArticle(ent, 0);

        if (articles.Count == 0)
        {
            _cartridgeLoaderSystem.UpdateCartridgeUiState(loaderUid, new LawsReaderEmptyBoundUserInterfaceState(ent.Comp.NotificationOn));
            return;
        }

        var state = new LawsReaderBoundUserInterfaceState(
            articles[ent.Comp.ArticleNumber],
            ent.Comp.ArticleNumber + 1,
            articles.Count,
            ent.Comp.NotificationOn);

        _cartridgeLoaderSystem.UpdateCartridgeUiState(loaderUid, state);
    }

    private void LawsReaderLeafArticle(Entity<LawsReaderCartridgeComponent> ent, int leafDir)
    {
        if (!TryGetArticles(ent, out var articles))
            return;

        ent.Comp.ArticleNumber += leafDir;

        if (ent.Comp.ArticleNumber >= articles.Count)
            ent.Comp.ArticleNumber = 0;

        if (ent.Comp.ArticleNumber < 0)
            ent.Comp.ArticleNumber = articles.Count - 1;
    }

    private void UpdateWriterDevices()
    {
        var query = EntityQueryEnumerator<LawsWriterComponent>();
        while (query.MoveNext(out var owner, out var comp))
        {
            UpdateWriterUi((owner, comp));
        }
    }

    private bool CanUse(EntityUid user, EntityUid console)
    {
        if (TryComp<AccessReaderComponent>(console, out var accessReaderComponent))
        {
            return _accessReaderSystem.IsAllowed(user, console, accessReaderComponent);
        }
        return true;
    }

    private void OnLawsWriterDraftUpdatedMessage(Entity<LawsWriterComponent> ent, ref LawsWriterSaveDraftMessage args)
    {
        ent.Comp.DraftContent = args.DraftContent;
    }

    private void OnRequestArticleDraftMessage(Entity<LawsWriterComponent> ent, ref LawsWriterRequestDraftMessage msg)
    {
        UpdateWriterUi(ent);
    }
}

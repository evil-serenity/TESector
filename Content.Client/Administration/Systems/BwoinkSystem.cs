#nullable enable
using Content.Shared.Administration;
using JetBrains.Annotations;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Client.Administration.Systems
{
    [UsedImplicitly]
    public sealed class BwoinkSystem : SharedBwoinkSystem
    {
        [Dependency] private readonly IGameTiming _timing = default!;

        public event EventHandler<BwoinkTextMessage>? OnBwoinkTextMessageRecieved;
        public event Action<AhelpAdminConfigState>? AhelpAdminConfigUpdated;
        public event Action<PlayerShipInspectionResponseMessage>? PlayerShipInspectionReceived;
        public event Action<PlayerSnapshotResponseMessage>? PlayerSnapshotReceived;
        public event Action<AdminStatisticsResponseMessage>? AdminStatisticsReceived;
        public event Action<PlayerBankInfoResponseMessage>? PlayerBankInfoReceived;
        public event Action<ModifyPlayerBankResponseMessage>? PlayerBankModified;
        public event Action<TeleportPlayerToStationResponseMessage>? PlayerTeleportedToStation;
        public event Action<UnstickPlayerShipPreviewResponseMessage>? PlayerShipUnstickPreviewReceived;
        public event Action<UnstickPlayerShipResponseMessage>? PlayerShipUnstuck;
        public event Action<SaveShipPreviewResponseMessage>? PlayerShipSavePreviewReceived;
        public event Action<SaveShipResponseMessage>? PlayerShipSaved;

        public AhelpAdminConfigState? CurrentAhelpAdminConfig { get; private set; }
        private (TimeSpan Timestamp, bool Typing) _lastTypingUpdateSent;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeNetworkEvent<AhelpAdminConfigStateMessage>(OnAhelpAdminConfigState);
            SubscribeNetworkEvent<PlayerShipInspectionResponseMessage>(OnPlayerShipInspection);
            SubscribeNetworkEvent<PlayerSnapshotResponseMessage>(OnPlayerSnapshot);
            SubscribeNetworkEvent<AdminStatisticsResponseMessage>(OnAdminStatistics);
            SubscribeNetworkEvent<PlayerBankInfoResponseMessage>(OnPlayerBankInfo);
            SubscribeNetworkEvent<ModifyPlayerBankResponseMessage>(OnPlayerBankModified);
            SubscribeNetworkEvent<TeleportPlayerToStationResponseMessage>(OnPlayerTeleportedToStation);
            SubscribeNetworkEvent<UnstickPlayerShipPreviewResponseMessage>(OnPlayerShipUnstickPreview);
            SubscribeNetworkEvent<UnstickPlayerShipResponseMessage>(OnPlayerShipUnstuck);
            SubscribeNetworkEvent<SaveShipPreviewResponseMessage>(OnPlayerShipSavePreview);
            SubscribeNetworkEvent<SaveShipResponseMessage>(OnPlayerShipSaved);
        }

        protected override void OnBwoinkTextMessage(BwoinkTextMessage message, EntitySessionEventArgs eventArgs)
        {
            OnBwoinkTextMessageRecieved?.Invoke(this, message);
        }

        private void OnAhelpAdminConfigState(AhelpAdminConfigStateMessage message, EntitySessionEventArgs eventArgs)
        {
            CurrentAhelpAdminConfig = message.State;
            AhelpAdminConfigUpdated?.Invoke(message.State);
        }

        private void OnPlayerShipInspection(PlayerShipInspectionResponseMessage message, EntitySessionEventArgs eventArgs)
        {
            PlayerShipInspectionReceived?.Invoke(message);
        }

        private void OnPlayerSnapshot(PlayerSnapshotResponseMessage message, EntitySessionEventArgs eventArgs)
        {
            PlayerSnapshotReceived?.Invoke(message);
        }

        private void OnAdminStatistics(AdminStatisticsResponseMessage message, EntitySessionEventArgs eventArgs)
        {
            AdminStatisticsReceived?.Invoke(message);
        }

        private void OnPlayerBankInfo(PlayerBankInfoResponseMessage message, EntitySessionEventArgs eventArgs)
        {
            PlayerBankInfoReceived?.Invoke(message);
        }

        private void OnPlayerBankModified(ModifyPlayerBankResponseMessage message, EntitySessionEventArgs eventArgs)
        {
            PlayerBankModified?.Invoke(message);
        }

        private void OnPlayerShipUnstuck(UnstickPlayerShipResponseMessage message, EntitySessionEventArgs eventArgs)
        {
            PlayerShipUnstuck?.Invoke(message);
        }

        private void OnPlayerShipUnstickPreview(UnstickPlayerShipPreviewResponseMessage message, EntitySessionEventArgs eventArgs)
        {
            PlayerShipUnstickPreviewReceived?.Invoke(message);
        }

        private void OnPlayerShipSaved(SaveShipResponseMessage message, EntitySessionEventArgs eventArgs)
        {
            PlayerShipSaved?.Invoke(message);
        }

        private void OnPlayerShipSavePreview(SaveShipPreviewResponseMessage message, EntitySessionEventArgs eventArgs)
        {
            PlayerShipSavePreviewReceived?.Invoke(message);
        }

        private void OnPlayerTeleportedToStation(TeleportPlayerToStationResponseMessage message, EntitySessionEventArgs eventArgs)
        {
            PlayerTeleportedToStation?.Invoke(message);
        }

        public void RequestPlayerShipInspection(NetUserId player)
        {
            RaiseNetworkEvent(new RequestPlayerShipInspectionMessage(player.UserId.ToString()));
        }

        public void RequestPlayerSnapshot(NetUserId player)
        {
            RaiseNetworkEvent(new RequestPlayerSnapshotMessage(player.UserId.ToString()));
        }

        public void RequestAdminStatistics()
        {
            RaiseNetworkEvent(new RequestAdminStatisticsMessage());
        }

        public void RequestPlayerBankInfo(NetUserId player)
        {
            RaiseNetworkEvent(new RequestPlayerBankInfoMessage(player.UserId.ToString()));
        }

        public void RequestModifyPlayerBank(NetUserId player, int amount, string reason)
        {
            RaiseNetworkEvent(new RequestModifyPlayerBankMessage(player.UserId.ToString(), amount, reason));
        }

        public void RequestUnstickPlayerShip(NetUserId player)
        {
            RaiseNetworkEvent(new RequestUnstickPlayerShipMessage(player.UserId.ToString()));
        }

        public void RequestUnstickPlayerShipPreview(NetUserId player)
        {
            RaiseNetworkEvent(new RequestUnstickPlayerShipPreviewMessage(player.UserId.ToString()));
        }

        public void RequestSaveShip(NetUserId player)
        {
            RaiseNetworkEvent(new RequestSaveShipMessage(player.UserId.ToString()));
        }

        public void RequestSaveShipPreview(NetUserId player)
        {
            RaiseNetworkEvent(new RequestSaveShipPreviewMessage(player.UserId.ToString()));
        }

        public void RequestTeleportPlayerToStation(NetUserId player)
        {
            RaiseNetworkEvent(new RequestTeleportPlayerToStationMessage(player.UserId.ToString()));
        }

        public void RequestAhelpAdminConfig()
        {
            RaiseNetworkEvent(new RequestAhelpAdminStateMessage());
        }

        public void SetAhelpAutoReplyEnabled(bool enabled)
        {
            RaiseNetworkEvent(new SetAhelpAutoReplyEnabledMessage(enabled));
        }

        public void SetAhelpTriageEnabled(bool enabled)
        {
            RaiseNetworkEvent(new SetAhelpTriageEnabledMessage(enabled));
        }

        public void SetAhelpAutoReplyBotName(string botName)
        {
            RaiseNetworkEvent(new SetAhelpAutoReplyBotNameMessage(botName));
        }

        public void SetAhelpCategoryAutoReplyEnabled(string category, bool enabled)
        {
            RaiseNetworkEvent(new SetAhelpCategoryAutoReplyEnabledMessage(category, enabled));
        }

        public void SetAhelpCategoryTriageEnabled(string category, bool enabled)
        {
            RaiseNetworkEvent(new SetAhelpCategoryTriageEnabledMessage(category, enabled));
        }

        public void AddOrRestoreAhelpCategory(string category, string template, string keywords)
        {
            RaiseNetworkEvent(new AddOrRestoreAhelpCategoryMessage(category, template, keywords));
        }

        public void RemoveAhelpCategory(string category)
        {
            RaiseNetworkEvent(new RemoveAhelpCategoryMessage(category));
        }

        public void SetAhelpAutoReplyTemplate(string category, string template)
        {
            RaiseNetworkEvent(new SetAhelpAutoReplyTemplateMessage(category, template));
        }

        public void ResetAhelpAutoReplyTemplate(string category)
        {
            RaiseNetworkEvent(new ResetAhelpAutoReplyTemplateMessage(category));
        }

        public void SetAhelpTriageKeywords(string category, string keywords)
        {
            RaiseNetworkEvent(new SetAhelpTriageKeywordsMessage(category, keywords));
        }

        public void ResetAhelpTriageKeywords(string category)
        {
            RaiseNetworkEvent(new ResetAhelpTriageKeywordsMessage(category));
        }

        public void Send(NetUserId channelId, string text, bool playSound, bool adminOnly)
        {
            // Reuse the channel ID as the 'true sender'.
            // Server will ignore this and if someone makes it not ignore this (which is bad, allows impersonation!!!), that will help.
            RaiseNetworkEvent(new BwoinkTextMessage(channelId, channelId, text, playSound: playSound, adminOnly: adminOnly));
            SendInputTextUpdated(channelId, false);
        }

        public void SendInputTextUpdated(NetUserId channel, bool typing)
        {
            if (_lastTypingUpdateSent.Typing == typing &&
                _lastTypingUpdateSent.Timestamp + TimeSpan.FromSeconds(1) > _timing.RealTime)
            {
                return;
            }

            _lastTypingUpdateSent = (_timing.RealTime, typing);
            RaiseNetworkEvent(new BwoinkClientTypingUpdated(channel, typing));
        }
    }
}

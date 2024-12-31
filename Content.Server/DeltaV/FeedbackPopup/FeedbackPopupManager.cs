﻿using Content.Server.Discord;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Content.Shared.DeltaV.CCVars;
using Content.Shared.DeltaV.FeedbackOverwatch;

namespace Content.Server.DeltaV.FeedbackPopup;

/// <summary>
///     This manager sends feedback from players to the discord through a webhook.
/// </summary>
public sealed class FeedbackPopupManager
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly DiscordWebhook _discord = default!;
    [Dependency] private readonly IServerNetManager _netManager = default!;

    private ISawmill _sawmill = default!;

    /// <summary>
    ///     Webhook to send the messages to!
    /// </summary>
    private string _webhookUrl = default!;

    public void Initialize()
    {
        _sawmill = Logger.GetSawmill("discord");

        _netManager.RegisterNetMessage<FeedbackResponseMessage>(RecieveFeedbackResponse);
        _cfg.OnValueChanged(DCCVars.DiscordPlayerFeedbackWebhook, SetWebhookUrl, true);
    }

    private void SetWebhookUrl(string webhookUrl)
    {
        _webhookUrl = webhookUrl;
    }

    private void RecieveFeedbackResponse(FeedbackResponseMessage message)
    {
        if (string.IsNullOrWhiteSpace(_webhookUrl))
            return;

        SendDiscordWebhookMessage(CreateMessage(message.FeedbackName, message.FeedbackMessage, message.MsgChannel.UserName));
    }

    private string CreateMessage(string feedbackName, string feedback, string username)
    {
        var header = Loc.GetString("feedbackpopup-discord-format-header", ("playerName", username));
        var info = Loc.GetString("feedbackpopup-discord-format-info", ("feedbackName", feedbackName));
        var spacer = Loc.GetString("feedbackpopup-discord-format-spacer");
        var feedbackbody = Loc.GetString("feedbackpopup-discord-format-feedbackbody", ("feedback", feedback));

        return header + info + "\n" + spacer + "\n" + feedbackbody;
    }

    private async void SendDiscordWebhookMessage(string msg)
    {
        try
        {
            var webhookData = await _discord.GetWebhook(_webhookUrl);
            if (webhookData == null)
                return;

            var webhookIdentifier = webhookData.Value.ToIdentifier();

            var payload = new WebhookPayload { Content = msg };

            await _discord.CreateMessage(webhookIdentifier, payload);
        }
        catch (Exception e)
        {
            _sawmill.Error($"Error while sending discord watchlist connection message:\n{e}");
        }
    }
}

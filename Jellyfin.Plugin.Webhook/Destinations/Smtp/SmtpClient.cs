using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Jellyfin.Plugin.Webhook.Destinations.Smtp;

/// <summary>
/// Client for the <see cref="SmtpOption"/>.
/// </summary>
public class SmtpClient : BaseClient, IWebhookClient<SmtpOption>
{
    private readonly ILogger<SmtpClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmtpClient"/> class.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger{SmtpClient}"/> interface.</param>
    public SmtpClient(ILogger<SmtpClient> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendAsync(SmtpOption option, Dictionary<string, object> data)
    {
        try
        {
            if (!SendWebhook(_logger, option, data))
            {
                return;
            }

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(option.SenderAddress));
            message.To.Add(MailboxAddress.Parse(option.ReceiverAddress));

            message.Subject = option.GetCompiledSubjectTemplate()(data);
            var body = option.GetMessageBody(data);
            if (!SendMessageBody(_logger, option, body))
            {
                return;
            }

            message.Body = new TextPart(option.IsHtml ? "html" : "plain") { Text = body };

            using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
            var secureSocketOptions = option.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None;
            await smtpClient.ConnectAsync(option.SmtpServer, option.SmtpPort, secureSocketOptions)
                .ConfigureAwait(false);
            if (option.UseCredentials)
            {
                await smtpClient.AuthenticateAsync(option.Username, option.Password)
                    .ConfigureAwait(false);
            }

            await smtpClient.SendAsync(message)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error sending email");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Jellyfin.Plugin.Webhook.Destinations.Smtp
{
    /// <summary>
    /// Client for the <see cref="SmtpOption"/>.
    /// </summary>
    public class SmtpClient : IWebhookClient<SmtpOption>
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
        public async Task SendAsync(SmtpOption options, Dictionary<string, object> data)
        {
            try
            {
                if (options.UserFilter.Length != 0
                    && data.TryGetValue("UserId", out var userIdObj)
                    && userIdObj is Guid userId)
                {
                    if (Array.IndexOf(options.UserFilter, userId) == -1)
                    {
                        _logger.LogDebug("UserId {UserId} not found in user filter, ignoring event", userId);
                        return;
                    }
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(options.SenderAddress, options.SenderAddress));
                message.To.Add(new MailboxAddress(options.ReceiverAddress, options.ReceiverAddress));

                message.Subject = options.GetCompiledSubjectTemplate()(data);
                message.Body = new TextPart(options.IsHtml ? "html" : "plain")
                {
                    Text = options.GetMessageBody(data)
                };

                using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
                await smtpClient.ConnectAsync(options.SmtpServer, options.SmtpPort, options.UseSsl)
                    .ConfigureAwait(false);
                if (options.UseCredentials)
                {
                    await smtpClient.AuthenticateAsync(options.Username, options.Password)
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
}

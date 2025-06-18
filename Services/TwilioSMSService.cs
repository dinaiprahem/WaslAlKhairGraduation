using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using WaslAlkhair.Api.DTOs.SMS;
using WaslAlkhair.Api.Helpers;

namespace WaslAlkhair.Api.Services
{
    public class TwilioSMSService : ISMSService
    {
        private readonly TwilioSettings _twilioSettings;
        private readonly ILogger<TwilioSMSService> _logger;

        public TwilioSMSService(IOptions<TwilioSettings> twilioSettings, ILogger<TwilioSMSService> logger)
        {
            _twilioSettings = twilioSettings.Value;
            _logger = logger;

            // Initialize Twilio client
            TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);
        }

        public async Task<SMSResponseDto> SendAsync(SendSMSDto smsRequest)
        {
            try
            {
                _logger.LogInformation("Sending SMS via Twilio to {Recipients}",
                    string.Join(", ", smsRequest.To));

                // For multiple recipients, send individual messages
                if (smsRequest.To.Count > 1)
                {
                    return await SendBulkSMS(smsRequest);
                }
                else
                {
                    return await SendSingleSMS(smsRequest.To.First(), smsRequest.Text, smsRequest.MediaUrls);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS via Twilio");
                return new SMSResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"Internal error: {ex.Message}",
                    SentAt = DateTime.UtcNow
                };
            }
        }

        public async Task<SMSResponseDto> SendAsync(string mobileNumber, string body)
        {
            return await SendSingleSMS(mobileNumber, body);
        }

        public async Task<SMSResponseDto> SendGroupMMS(List<string> recipients, string text, List<string>? mediaUrls = null, string? subject = null)
        {
            var smsRequest = new SendSMSDto
            {
                To = recipients,
                Text = text,
                MediaUrls = mediaUrls,
                Subject = subject,
                MessageType = mediaUrls?.Any() == true ? "MMS" : "SMS"
            };

            return await SendAsync(smsRequest);
        }

        private async Task<SMSResponseDto> SendSingleSMS(string to, string body, List<string>? mediaUrls = null)
        {
            try
            {
                var messageOptions = new CreateMessageOptions(new PhoneNumber(to))
                {
                    From = new PhoneNumber(_twilioSettings.FromPhoneNumber),
                    Body = body
                };

                // Add media URLs if provided (for MMS)
                if (mediaUrls?.Any() == true)
                {
                    messageOptions.MediaUrl = mediaUrls.Select(url => new Uri(url)).ToList();
                }

                // Add webhook URL if configured
                if (!string.IsNullOrEmpty(_twilioSettings.WebhookUrl))
                {
                    messageOptions.StatusCallback = new Uri(_twilioSettings.WebhookUrl);
                }

                var message = await MessageResource.CreateAsync(messageOptions);

                if (!string.IsNullOrEmpty(message.ErrorMessage))
                {
                    _logger.LogError("Twilio error: {ErrorCode} - {ErrorMessage}", message.ErrorCode, message.ErrorMessage);
                    return new SMSResponseDto
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Twilio error: {message.ErrorCode} - {message.ErrorMessage}",
                        SentAt = DateTime.UtcNow,
                        To = to
                    };
                }

                _logger.LogInformation("SMS sent successfully to {To}. MessageSid: {MessageSid}", to, message.Sid);

                return new SMSResponseDto
                {
                    IsSuccess = true,
                    MessageId = message.Sid,
                    Status = message.Status.ToString(),
                    SentAt = message.DateCreated ?? DateTime.UtcNow,
                    To = message.To?.ToString(),
                    From = message.From?.ToString(),
                    MessageType = mediaUrls?.Any() == true ? "MMS" : "SMS",
                    ProviderData = new Dictionary<string, object>
                    {
                        ["provider"] = "Twilio",
                        ["accountSid"] = message.AccountSid,
                        ["messagingServiceSid"] = message.MessagingServiceSid ?? "",
                        ["numSegments"] = message.NumSegments?.ToString() ?? "1",
                        ["price"] = message.Price?.ToString() ?? "0",
                        ["priceUnit"] = message.PriceUnit ?? "USD"
                    }
                };
            }
            catch (Twilio.Exceptions.ApiException ex)
            {
                _logger.LogError(ex, "Twilio API error sending SMS to {To}: {Code} - {Message}", to, ex.Code, ex.Message);
                return new SMSResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"Twilio API error: {ex.Code} - {ex.Message}",
                    SentAt = DateTime.UtcNow,
                    To = to
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending SMS to {To}", to);
                return new SMSResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"Unexpected error: {ex.Message}",
                    SentAt = DateTime.UtcNow,
                    To = to
                };
            }
        }

        private async Task<SMSResponseDto> SendBulkSMS(SendSMSDto smsRequest)
        {
            var results = new List<SMSResponseDto>();
            var successCount = 0;
            var errors = new List<string>();

            foreach (var recipient in smsRequest.To)
            {
                var result = await SendSingleSMS(recipient, smsRequest.Text, smsRequest.MediaUrls);
                results.Add(result);

                if (result.IsSuccess)
                {
                    successCount++;
                }
                else
                {
                    errors.Add($"{recipient}: {result.ErrorMessage}");
                }
            }

            // Return a summary response for bulk sending
            var allSuccessful = successCount == smsRequest.To.Count;
            return new SMSResponseDto
            {
                IsSuccess = allSuccessful,
                MessageId = $"bulk_{DateTime.UtcNow:yyyyMMddHHmmss}",
                Status = allSuccessful ? "sent" : "partial",
                SentAt = DateTime.UtcNow,
                To = string.Join(", ", smsRequest.To),
                From = _twilioSettings.FromPhoneNumber,
                MessageType = smsRequest.MediaUrls?.Any() == true ? "MMS" : "SMS",
                ErrorMessage = errors.Any() ? string.Join("; ", errors) : null,
                ProviderData = new Dictionary<string, object>
                {
                    ["provider"] = "Twilio",
                    ["totalRecipients"] = smsRequest.To.Count,
                    ["successfulSends"] = successCount,
                    ["failedSends"] = smsRequest.To.Count - successCount,
                    ["detailedResults"] = results
                }
            };
        }

        public async Task<List<SMSResponseDto>> GetReceivedMessagesAsync(DateTime? since = null)
        {
            try
            {
                _logger.LogInformation("Retrieving received messages from Twilio");

                var options = new ReadMessageOptions();

                if (since.HasValue)
                {
                    options.DateSentAfter = since.Value;
                }

                // Get incoming messages (Direction = inbound)
                options.To = new PhoneNumber(_twilioSettings.FromPhoneNumber);

                var messages = await MessageResource.ReadAsync(options);

                return messages
                    .Where(m => m.Direction == MessageResource.DirectionEnum.Inbound)
                    .Select(m => new SMSResponseDto
                    {
                        IsSuccess = true,
                        MessageId = m.Sid,
                        Status = m.Status.ToString(),
                        SentAt = m.DateSent ?? DateTime.UtcNow,
                        To = m.To?.ToString(),
                        From = m.From?.ToString(),
                        MessageType = int.TryParse(m.NumMedia?.ToString(), out int numMedia) && numMedia > 0 ? "MMS" : "SMS",
                        ProviderData = new Dictionary<string, object>
                        {
                            ["provider"] = "Twilio",
                            ["body"] = m.Body ?? "",
                            ["numMedia"] = m.NumMedia?.ToString() ?? "0",
                            ["accountSid"] = m.AccountSid,
                            ["messagingServiceSid"] = m.MessagingServiceSid ?? ""
                        }
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving received messages from Twilio");
                return new List<SMSResponseDto>();
            }
        }
    }
}
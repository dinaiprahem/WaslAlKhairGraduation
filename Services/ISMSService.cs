using Twilio.Rest.Api.V2010.Account;

namespace WaslAlkhair.Api.Services
{
    public interface ISMSService
    {
        MessageResource Send(string mobileNumber, string body);

    }
}

namespace WaslAlkhair.Api.Services
{
    public interface ITokenBlacklist
    {
        Task AddToBlacklistAsync(string token);
        Task<bool> IsTokenBlacklistedAsync(string token);
    }
}

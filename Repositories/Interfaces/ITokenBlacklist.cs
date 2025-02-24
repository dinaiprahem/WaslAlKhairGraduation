namespace WaslAlkhair.Api.Repositories.Interfaces
{
    public interface ITokenBlacklist
    {
        Task AddToBlacklistAsync(string token);
        Task<bool> IsTokenBlacklistedAsync(string token);
    }
}

namespace WaslAlkhair.Api.Services
{
    public class TokenBlacklist : ITokenBlacklist
    {
        private readonly HashSet<string> _blacklistedTokens = new HashSet<string>();

        public Task AddToBlacklistAsync(string token)
        {
            _blacklistedTokens.Add(token);
            return Task.CompletedTask;
        }

        public Task<bool> IsTokenBlacklistedAsync(string token)
        {
            return Task.FromResult(_blacklistedTokens.Contains(token));
        }
    }
}

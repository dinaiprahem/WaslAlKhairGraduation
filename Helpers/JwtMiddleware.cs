using WaslAlkhair.Api.Repositories.Interfaces;

namespace WaslAlkhair.Api.Helpers
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITokenBlacklist _tokenBlacklist;

        public JwtMiddleware(RequestDelegate next, ITokenBlacklist tokenBlacklist)
        {
            _next = next;
            _tokenBlacklist = tokenBlacklist;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token != null && await _tokenBlacklist.IsTokenBlacklistedAsync(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token is invalidated.");
                return;
            }

            await _next(context);
        }
    }
}

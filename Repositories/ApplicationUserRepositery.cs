using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories.Interfaces;

namespace WaslAlkhair.Api.Repositories
{
    public class ApplicationUserRepositery: Repositery<AppUser> , IApplicationUserRepositery
    {
        private readonly AppDbContext context;

        public ApplicationUserRepositery(AppDbContext context) : base(context)
        {
            this.context = context;
        }

       
    }
}

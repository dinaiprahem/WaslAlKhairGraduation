using Google;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.Repositories.Interfaces;

namespace WaslAlkhair.Api.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IApplicationUserRepositery Users { get; private set; }

        public UnitOfWork(AppDbContext context , IApplicationUserRepositery Users)
        {
            _context = context;
            this.Users = Users;
        }

        public async Task<bool> CompleteAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

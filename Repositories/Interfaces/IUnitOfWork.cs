namespace WaslAlkhair.Api.Repositories.Interfaces
{
    public interface IUnitOfWork :IDisposable
    {
       // public IUserRepository UsersRepository { get;  }
        Task<bool> CompleteAsync();
    }
}

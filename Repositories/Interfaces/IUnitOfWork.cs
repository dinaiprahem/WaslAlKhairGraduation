namespace WaslAlkhair.Api.Repositories.Interfaces
{
    public interface IUnitOfWork :IDisposable
    {
        Task<bool> Complete();
    }
}

namespace WaslAlkhair.Api.Repositories.Interfaces
{
    public interface IUnitOfWork :IDisposable
    {
        IApplicationUserRepositery Users { get; }
        Task<bool> CompleteAsync();
    }
}

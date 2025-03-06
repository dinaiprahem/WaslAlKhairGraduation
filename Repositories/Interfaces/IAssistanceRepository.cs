using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Repositories.Interfaces
{
	public interface IAssistanceRepository : IRepositery<Assistance>
	{
		Task<List<Assistance>> GetByUserIdAsync(string userId);  // Get all assistances created by a specific user
		Task<Assistance?> GetByIdAsync(Guid id);  // Get a specific assistance by its id
		Task<List<Assistance>> GetByTypeIdAsync(Guid typeId);  // Get assistances of a specific type
		void Update(Assistance assistance);  // Update a specific assistance
		Task<IEnumerable<Assistance>> GetAssistancesByTypeAsync(Guid assistanceTypeId);
	}
}

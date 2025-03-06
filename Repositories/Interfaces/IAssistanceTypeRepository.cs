using System;
using System.Threading.Tasks;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Repositories.Interfaces
{
	public interface IAssistanceTypeRepository : IRepositery<AssistanceType>
	{
		Task<AssistanceType> GetByNameAsync(string name);
	}
}

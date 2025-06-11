using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories.Interfaces;

namespace WaslAlkhair.Api.Repositories
{
	public class AssistanceTypeRepository : Repositery<AssistanceType>, IAssistanceTypeRepository
	{
		private readonly AppDbContext _context;

		public AssistanceTypeRepository(AppDbContext context) : base(context)
		{
			_context = context;
		}

		// Get AssistanceType by Name 
		public async Task<AssistanceType> GetByNameAsync(string name)
		{
			return await dbset.FirstOrDefaultAsync(at => at.Name == name);
		}
	}
}

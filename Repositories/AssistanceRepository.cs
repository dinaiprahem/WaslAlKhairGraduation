using Microsoft.EntityFrameworkCore;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaslAlkhair.Api.Repositories
{
	public class AssistanceRepository : Repositery<Assistance>, IAssistanceRepository
	{
		private readonly AppDbContext _context;

		public AssistanceRepository(AppDbContext context) : base(context)
		{
			_context = context;
		}

		// Get assistance by ID 
		public async Task<Assistance?> GetByIdAsync(Guid id)
		{
			return await _context.Assistances
				.Include(a => a.AssistanceType)  
				.Include(a => a.CreatedBy)      
				.FirstOrDefaultAsync(a => a.Id == id); 
		}


		// Get assistances created by a specific user 
		public async Task<List<Assistance>> GetByUserIdAsync(string userId)
		{
			return await _context.Assistances
				.Include(a => a.AssistanceType) 
				.Where(a => a.CreatedById == userId)
				.ToListAsync();
		}

		
		public async Task<List<Assistance>> GetByTypeIdAsync(Guid typeId)
		{
			return await _context.Assistances
				.Include(a => a.AssistanceType) 
				.Where(a => a.AssistanceTypeId == typeId)
				.ToListAsync();
		}
		public async Task<IEnumerable<Assistance>> GetAssistancesByTypeAsync(Guid assistanceTypeId)
		{
			return await _context.Assistances
				.Include(a => a.CreatedBy)  // Include the user who created the assistance (AppUser)
				.Where(a => a.AssistanceTypeId == assistanceTypeId && a.IsOpen)  // Filter by open assistances
				.ToListAsync();
		}


		// Update assistance
		public void Update(Assistance assistance)
		{
			_context.Assistances.Update(assistance);
		}

	}
}

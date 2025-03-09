using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WaslAlkhair.Api.Data.Configurations;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.Data
{
	public class AppDbContext : IdentityDbContext<AppUser>
	{
		public DbSet<Opportunity> Opportunities { get; set; }
		public DbSet<OpportunityParticipation> OpportunityParticipations { get; set; }
        public DbSet<DonationOpportunity> DonationOpportunities { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<DonationDistribution> DonationDistributions { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfiguration(new AppUserConfiguration());
			modelBuilder.ApplyConfiguration(new OpportunityConfiguration());
			modelBuilder.ApplyConfiguration(new OpportunityParticipationConfiguration());

            modelBuilder.ApplyConfiguration(new DonationOpportunityConfiguration());
            modelBuilder.ApplyConfiguration(new DonationConfiguration());
            modelBuilder.ApplyConfiguration(new DonationDistributionConfiguration());
            modelBuilder.ApplyConfiguration(new GiftDonationConfiguration());

        }

        public override int SaveChanges()
		{
			ProcessOpportunityStatus();
			return base.SaveChanges();
		}

		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			ProcessOpportunityStatus();
			return await base.SaveChangesAsync(cancellationToken);
		}

		private void ProcessOpportunityStatus()
		{
			foreach (var opportunity in ChangeTracker.Entries<Opportunity>()
				.Where(e => e.State != EntityState.Unchanged)
				.Select(e => e.Entity))
			{
				opportunity.CheckStatus(); 
			}
		}
	}
}

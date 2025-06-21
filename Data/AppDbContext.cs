using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WaslAlkhair.Api.Data.Configurations;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Services;

namespace WaslAlkhair.Api.Data
{
	public class AppDbContext : IdentityDbContext<AppUser>
	{
		public DbSet<Opportunity> Opportunities { get; set; }
		public DbSet<OpportunityParticipation> OpportunityParticipations { get; set; }
        public DbSet<DonationOpportunity> DonationOpportunities { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<DonationDistribution> DonationDistributions { get; set; }
        public DbSet<DonationCategory> DonationCategories { get; set; }
        public DbSet<GiftDonation> GiftDonation { get; set; }
		public DbSet<Assistance> Assistances { get; set; }
		public DbSet<AssistanceType> AssistanceTypes { get; set; }
        public DbSet<UserReview> UserReviews { get; set; }
        public DbSet<LostItem> LostItems { get; set; }



        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfiguration(new AppUserConfiguration());
			modelBuilder.ApplyConfiguration(new OpportunityConfiguration());
			modelBuilder.ApplyConfiguration(new OpportunityParticipationConfiguration());
			modelBuilder.ApplyConfiguration(new AssistanceConfiguration());
			modelBuilder.ApplyConfiguration(new AssistanceTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DonationOpportunityConfiguration());
            modelBuilder.ApplyConfiguration(new DonationConfiguration());
            modelBuilder.ApplyConfiguration(new DonationDistributionConfiguration());
            modelBuilder.ApplyConfiguration(new GiftDonationConfiguration());
            modelBuilder.ApplyConfiguration(new DonationCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new UserReviewConfiguration());
            modelBuilder.ApplyConfiguration(new LostItemConfiguration());
			
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

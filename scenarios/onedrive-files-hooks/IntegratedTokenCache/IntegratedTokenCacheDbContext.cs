using IntegratedTokenCache.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntegratedTokenCache
{
    /// <summary>
    /// DbContext used to access the additional persisted account activity data
    /// </summary>
    public class IntegratedTokenCacheDbContext : DbContext
    {
        public IntegratedTokenCacheDbContext(DbContextOptions<IntegratedTokenCacheDbContext> options) : base(options)
        {
        }

        public DbSet<MsalAccountActivity> MsalAccountActivities { get; set; }

        public DbSet<SubscriptionActivity> SubscriptionActivities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SubscriptionActivity>()
                .HasKey(c => new { c.AccountObjectId, c.AccountTenantId, c.UserPrincipalName});
        }
    }
}
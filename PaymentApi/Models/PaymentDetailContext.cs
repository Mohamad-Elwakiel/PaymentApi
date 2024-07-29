using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace PaymentApi.Models
{
    public class PaymentDetailContext : IdentityDbContext<ApplicationUser>
    {
        public PaymentDetailContext(DbContextOptions options) : base(options)
        {

            
        }
        public DbSet<PaymentDetail> PaymentDetails { get; set; }   
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<StoredProcedureModel>().HasNoKey().ToView(null);
            
            base.OnModelCreating(builder);

            
        }

        public async Task<List<StoredProcedureModel>> ExecuteMyStoredProcedureAsync(MyStoredProcedure spParams)
        {
            var parameters = new[]
            {
            new SqlParameter("@Name", spParams.Name),
            new SqlParameter("@CardNumber", spParams.CardNumber),
            new SqlParameter("@PageIndex", spParams.PageIndex),
            new SqlParameter("@PageSize", spParams.PageSize)
        };

            var result = await Set<StoredProcedureModel>()
                .FromSqlRaw("EXEC SearchUser @Name, @CardNumber, @PageIndex, @PageSize", parameters)
                .ToListAsync();

            return result;
        }
    }
}

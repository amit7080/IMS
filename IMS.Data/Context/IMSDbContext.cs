using IMS.Data.Interface;
using IMS.Data.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IMS.Data.Context
{
    public class IMSDbContext : IdentityDbContext<User>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IMSDbContext(DbContextOptions<IMSDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssignUser>()
               .HasOne(x => x.AssignedHr).WithMany(t => t.AssignedHr).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AssignUser>()
                .HasOne(x => x.AssignedManager).WithMany(t => t.AssignedManager).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AssignUser>()
                .HasOne(x => x.User).WithMany(t => t.Users).OnDelete(DeleteBehavior.Restrict);
            base.OnModelCreating(modelBuilder);
        }
        public override int SaveChanges()
        {
            var modifiedEntites = ChangeTracker.Entries()
                .Where(x => x.Entity is IAuditableEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));
            foreach (var entity in modifiedEntites)
            {
                if (entity.Entity is not IAuditableEntity auditabeEntity)
                {
                    continue;
                }
                var userId = _httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var time = DateTime.Now;
                if (entity.State == EntityState.Added)
                {
                    auditabeEntity.CreatedBy = userId;
                    auditabeEntity.CreationDate = time;
                }
                else
                {
                    Entry(auditabeEntity).Property(x => x.CreationDate).IsModified = false;
                    Entry(auditabeEntity).Property(x => x.CreatedBy).IsModified = false;
                    auditabeEntity.ModifiedBy = userId;
                    auditabeEntity.ModificationDate = time;
                }
            }
            return base.SaveChanges();
        }

        public DbSet<Department> Departments { get; set; }
    }
}

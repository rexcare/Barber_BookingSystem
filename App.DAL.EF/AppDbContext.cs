using App.Domain;
using App.Domain.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF;

public class AppDbContext: IdentityDbContext<AppUser, AppRole, Guid>
{
    public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;
    
    public DbSet<WorkTime> WorkTimes { get; set; } = default!;
    public DbSet<WorkTimeTemplate> WorkTimeTemplates { get; set; } = default!;
    public DbSet<Vacation> Vacations { get; set; } = default!;
    public DbSet<CompanyInfo> CompanyInfos { get; set; } = default!;

    
    
    
    public DbSet<ReservedTime> ReservedTimes { get; set; } = default!;
    public DbSet<Service> Services { get; set; } = default!;
    public DbSet<Customer> Customers { get; set; } = default!;
    public DbSet<Appointment> Appointments { get; set; } = default!;
    
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Remove cascade delete
        // foreach (var relationship in builder.Model
        //              .GetEntityTypes()
        //              .SelectMany(e => e.GetForeignKeys()))
        // {
        //     relationship.DeleteBehavior = DeleteBehavior.Restrict;
        // }


    }

    public override int SaveChanges()
    {
        FixEntities(this);
        
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        FixEntities(this);
        
        return base.SaveChangesAsync(cancellationToken);
    }


    private void FixEntities(AppDbContext context)
    {
        var dateProperties = context.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(DateTime))
            .Select(z => new
            {
                ParentName = z.DeclaringEntityType.Name,
                PropertyName = z.Name
            });

        var editedEntitiesInTheDbContextGraph = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Select(x => x.Entity);
        

        foreach (var entity in editedEntitiesInTheDbContextGraph)
        {
            var entityFields = dateProperties.Where(d => d.ParentName == entity.GetType().FullName);

            foreach (var property in entityFields)
            {
                var prop = entity.GetType().GetProperty(property.PropertyName);

                if (prop == null)
                    continue;

                var originalValue = prop.GetValue(entity) as DateTime?;
                if (originalValue == null)
                    continue;

                prop.SetValue(entity, DateTime.SpecifyKind(originalValue.Value, DateTimeKind.Utc));
            }
        }
    }
}
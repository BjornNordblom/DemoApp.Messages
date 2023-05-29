using System.Reflection;
using Microsoft.EntityFrameworkCore;

public class DataContext : DbContext
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _serviceProvider;

    public DbSet<Outbox> Outbox { get; set; } = default!;

    public DataContext(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _serviceProvider = serviceProvider;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var sqlOptions = optionsBuilder
            .UseLoggerFactory(_loggerFactory)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .UseSqlServer(
                //@"Server=localhost;Database=Hypernova;User Id=sa;Password=s3c-r3t!P@ssw0rd;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;"
                @"Server=localhost;Database=Hypernova;User Id=sa;Password=s3c-r3t!P@ssw0rd;Trusted_Connection=False;MultipleActiveResultSets=True;TrustServerCertificate=True;"
            )
            .Options;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) { }
}

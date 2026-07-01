using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace VectorLLM;

public class AppDbContext : DbContext
{
    public DbSet<Film> Films { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connString = Program.Config.GetConnectionString("DefaultConnection");

        // Ensure this connection string is accurate to your environment
        string connectionString =  connString;
        ;
        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Explicitly map the entity to the table
        modelBuilder.Entity<Film>().ToTable("film");
    }
}
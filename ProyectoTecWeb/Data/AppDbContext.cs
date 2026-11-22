using Microsoft.EntityFrameworkCore;
using ProyectoTecWeb.Models;

namespace ProyectoTecWeb.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ScoreBolivia> BoliviaScores => Set<ScoreBolivia>();

    // Tablas del integrante responsable de Argentina.
    public DbSet<ScoreArgentina> ArgentinaScores => Set<ScoreArgentina>();
    public DbSet<JugadorArgentina> JugadoresArgentina => Set<JugadorArgentina>();
    public DbSet<PartidaArgentina> PartidasArgentina => Set<PartidaArgentina>();
    public DbSet<LogroArgentina> LogrosArgentina => Set<LogroArgentina>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

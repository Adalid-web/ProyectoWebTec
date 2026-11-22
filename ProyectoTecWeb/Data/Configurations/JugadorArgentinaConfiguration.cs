using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoTecWeb.Models;

namespace ProyectoTecWeb.Data.Configurations;

public class JugadorArgentinaConfiguration : IEntityTypeConfiguration<JugadorArgentina>
{
    public void Configure(EntityTypeBuilder<JugadorArgentina> builder)
    {
        builder.ToTable("jugadores_argentina");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(80).IsRequired();
        builder.Property(x => x.Alias).HasColumnName("alias").HasMaxLength(30).IsRequired();
        builder.Property(x => x.FechaRegistro).HasColumnName("fecha_registro")
            .HasDefaultValueSql("NOW()").IsRequired();
        builder.HasIndex(x => x.Alias).IsUnique().HasDatabaseName("ux_jugadores_argentina_alias");
    }
}

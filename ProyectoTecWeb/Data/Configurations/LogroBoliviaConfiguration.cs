using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoTecWeb.Models;

namespace ProyectoTecWeb.Data.Configurations;

public class LogroBoliviaConfiguration : IEntityTypeConfiguration<LogroBolivia>
{
    public void Configure(EntityTypeBuilder<LogroBolivia> builder)
    {
        builder.ToTable("logros_bolivia");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.JugadorId).HasColumnName("jugador_id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(80).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200);
        builder.Property(x => x.FechaOtorgado).HasColumnName("fecha_otorgado")
            .HasDefaultValueSql("NOW()").IsRequired();
        builder.HasOne(x => x.Jugador).WithMany(x => x.Logros)
            .HasForeignKey(x => x.JugadorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.JugadorId).HasDatabaseName("ix_logros_bolivia_jugador_id");
    }
}

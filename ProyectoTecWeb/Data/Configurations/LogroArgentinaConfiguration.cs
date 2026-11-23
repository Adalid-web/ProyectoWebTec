using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoTecWeb.Models;

namespace ProyectoTecWeb.Data.Configurations;

public class LogroArgentinaConfiguration : IEntityTypeConfiguration<LogroArgentina>
{
    public void Configure(EntityTypeBuilder<LogroArgentina> builder)
    {
        builder.ToTable("logros_argentina");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.JugadorId).HasColumnName("jugador_id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(80).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(200);
        builder.Property(x => x.FechaOtorgado).HasColumnName("fecha_otorgado")
            .HasDefaultValueSql("NOW()").IsRequired();
        builder.HasOne(x => x.Jugador).WithMany(x => x.Logros)
            .HasForeignKey(x => x.JugadorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.JugadorId).HasDatabaseName("ix_logros_argentina_jugador_id");
    }
}

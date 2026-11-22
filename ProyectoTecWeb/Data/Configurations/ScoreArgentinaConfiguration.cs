using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoTecWeb.Models;

namespace ProyectoTecWeb.Data.Configurations;

public class ScoreArgentinaConfiguration : IEntityTypeConfiguration<ScoreArgentina>
{
    public void Configure(EntityTypeBuilder<ScoreArgentina> builder)
    {
        builder.ToTable("leaderboard_argentina", table =>
            table.HasCheckConstraint("ck_leaderboard_argentina_score", "score >= 0 AND score <= 100000"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PlayerName).HasColumnName("player_name").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Score).HasColumnName("score").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()").IsRequired();
        builder.HasIndex(x => x.Score).HasDatabaseName("ix_leaderboard_argentina_score").IsDescending();
    }
}

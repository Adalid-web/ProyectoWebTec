using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoTecWeb.Models;

namespace ProyectoTecWeb.Data.Configurations;

public class ScorePeruConfiguration : IEntityTypeConfiguration<ScorePeru>
{
    public void Configure(EntityTypeBuilder<ScorePeru> builder)
    {
        builder.ToTable("leaderboard_peru", table =>
            table.HasCheckConstraint("ck_leaderboard_peru_score", "score >= 0 AND score <= 100000"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PlayerName).HasColumnName("player_name").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Score).HasColumnName("score").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()").IsRequired();
        builder.HasIndex(x => x.Score).HasDatabaseName("ix_leaderboard_peru_score").IsDescending();
    }
}

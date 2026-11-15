namespace ProyectoTecWeb.Models;

public class ScoreBolivia
{
    public int Id { get; set; }

    public string PlayerName { get; set; } = string.Empty;

    public int Score { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

namespace ProyectoTecWeb.Models;

// Tabla de clasificación de Argentina. Mantiene el mismo contrato del juego de Bolivia.
public class ScoreArgentina
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

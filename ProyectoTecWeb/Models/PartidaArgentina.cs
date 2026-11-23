namespace ProyectoTecWeb.Models;

public class PartidaArgentina
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int JugadorId { get; set; }
    public int Puntaje { get; set; }
    public int DuracionSegundos { get; set; } = 30;
    public DateTime JugadoEn { get; set; } = DateTime.UtcNow;

    public JugadorArgentina Jugador { get; set; } = null!;
}

namespace ProyectoTecWeb.Models;

public class LogroArgentina
{
    public int Id { get; set; }
    public int JugadorId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaOtorgado { get; set; } = DateTime.UtcNow;

    public JugadorArgentina Jugador { get; set; } = null!;
}

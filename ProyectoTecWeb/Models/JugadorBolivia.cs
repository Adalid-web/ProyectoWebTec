namespace ProyectoTecWeb.Models;

public class JugadorBolivia
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public ICollection<PartidaBolivia> Partidas { get; set; } = new List<PartidaBolivia>();
    public ICollection<LogroBolivia> Logros { get; set; } = new List<LogroBolivia>();
}

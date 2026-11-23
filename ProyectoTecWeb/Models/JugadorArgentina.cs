namespace ProyectoTecWeb.Models;

public class JugadorArgentina
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public ICollection<PartidaArgentina> Partidas { get; set; } = new List<PartidaArgentina>();
    public ICollection<LogroArgentina> Logros { get; set; } = new List<LogroArgentina>();
}

using System.ComponentModel.DataAnnotations;

namespace ProyectoTecWeb.DTOs;

public class PartidaArgentinaRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El jugador es obligatorio.")]
    public int JugadorId { get; set; }

    [Range(0, 100000, ErrorMessage = "Puntaje fuera de rango.")]
    public int Puntaje { get; set; }

    [Range(1, 300, ErrorMessage = "La duración debe estar entre 1 y 300 segundos.")]
    public int DuracionSegundos { get; set; } = 30;
}

public class PartidaArgentinaResponseDto
{
    public int Id { get; set; }
    public int JugadorId { get; set; }
    public int Puntaje { get; set; }
    public int DuracionSegundos { get; set; }
    public DateTime JugadoEn { get; set; }
}

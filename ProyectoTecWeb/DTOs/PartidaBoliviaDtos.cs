using System.ComponentModel.DataAnnotations;

namespace ProyectoTecWeb.DTOs;

public class PartidaBoliviaRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El jugador es obligatorio.")]
    public int JugadorId { get; set; }

    [Range(0, 100000, ErrorMessage = "El puntaje debe estar entre 0 y 100000.")]
    public int Puntaje { get; set; }

    [Range(1, 300, ErrorMessage = "La duración debe estar entre 1 y 300 segundos.")]
    public int DuracionSegundos { get; set; } = 30;
}

public class PartidaBoliviaResponseDto
{
    public int Id { get; set; }
    public int JugadorId { get; set; }
    public int Puntaje { get; set; }
    public int DuracionSegundos { get; set; }
    public DateTime JugadoEn { get; set; }
}

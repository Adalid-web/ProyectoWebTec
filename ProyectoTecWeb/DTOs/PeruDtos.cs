using System.ComponentModel.DataAnnotations;

namespace ProyectoTecWeb.DTOs;

public class JugadorPeruRequestDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 80 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El alias es obligatorio.")]
    [StringLength(40, MinimumLength = 1, ErrorMessage = "El alias debe tener entre 1 y 40 caracteres.")]
    public string Alias { get; set; } = string.Empty;
}

public class JugadorPeruResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}

public class PartidaPeruRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El jugador es obligatorio.")]
    public int JugadorId { get; set; }

    [Range(0, 100000, ErrorMessage = "El puntaje debe estar entre 0 y 100000.")]
    public int Puntaje { get; set; }

    [Range(1, 300, ErrorMessage = "La duración debe estar entre 1 y 300 segundos.")]
    public int DuracionSegundos { get; set; } = 30;
}

public class PartidaPeruResponseDto
{
    public int Id { get; set; }
    public int JugadorId { get; set; }
    public int Puntaje { get; set; }
    public int DuracionSegundos { get; set; }
    public DateTime JugadoEn { get; set; }
}

public class LogroPeruRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El jugador es obligatorio.")]
    public int JugadorId { get; set; }

    [Required(ErrorMessage = "El nombre del logro es obligatorio.")]
    [StringLength(80, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 80 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres.")]
    public string? Descripcion { get; set; }
}

public class LogroPeruResponseDto
{
    public int Id { get; set; }
    public int JugadorId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaOtorgado { get; set; }
}

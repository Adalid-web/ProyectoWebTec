using System.ComponentModel.DataAnnotations;

namespace ProyectoTecWeb.DTOs;

public class LogroArgentinaRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El jugador es obligatorio.")]
    public int JugadorId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La descripción no puede superar 200 caracteres.")]
    public string? Descripcion { get; set; }
}

public class LogroArgentinaResponseDto
{
    public int Id { get; set; }
    public int JugadorId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaOtorgado { get; set; }
}

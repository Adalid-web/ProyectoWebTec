using System.ComponentModel.DataAnnotations;

namespace ProyectoTecWeb.DTOs;

public class LogroBoliviaRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El jugador es obligatorio.")]
    public int JugadorId { get; set; }

    [Required(ErrorMessage = "El nombre del logro es obligatorio.")]
    [StringLength(80, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 80 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres.")]
    public string? Descripcion { get; set; }
}

public class LogroBoliviaResponseDto
{
    public int Id { get; set; }
    public int JugadorId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaOtorgado { get; set; }
}

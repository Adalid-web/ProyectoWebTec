using System.ComponentModel.DataAnnotations;

namespace ProyectoTecWeb.DTOs;

public class JugadorBoliviaRequestDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 80 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El alias es obligatorio.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El alias debe tener entre 1 y 30 caracteres.")]
    public string Alias { get; set; } = string.Empty;
}

public class JugadorBoliviaResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}

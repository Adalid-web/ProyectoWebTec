using System.ComponentModel.DataAnnotations;

namespace ProyectoTecWeb.DTOs;

public class JugadorArgentinaRequestDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El alias es obligatorio.")]
    [StringLength(30, MinimumLength = 1)]
    public string Alias { get; set; } = string.Empty;
}

public class JugadorArgentinaResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}

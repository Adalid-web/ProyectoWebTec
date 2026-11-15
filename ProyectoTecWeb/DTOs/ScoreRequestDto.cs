using System.ComponentModel.DataAnnotations;

namespace ProyectoTecWeb.DTOs;

public class ScoreRequestDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 30 caracteres.")]
    public string PlayerName { get; set; } = string.Empty;

    [Range(0, 100000, ErrorMessage = "El puntaje debe estar entre 0 y 100000.")]
    public int Score { get; set; }
}

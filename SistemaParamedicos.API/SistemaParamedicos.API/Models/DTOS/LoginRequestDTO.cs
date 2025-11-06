using System.ComponentModel.DataAnnotations;

namespace SistemaParamedicos.API.Models.DTOs
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "El usuario es requerido")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; }
    }
}
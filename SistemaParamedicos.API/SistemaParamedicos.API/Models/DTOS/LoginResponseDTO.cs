namespace SistemaParamedicos.API.Models.DTOs
{
    public class LoginResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UsuarioData? Usuario { get; set; }
    }

    public class UsuarioData
    {
        public string IdUsuarioAcc { get; set; }
        public string Nombre { get; set; }
        public string Usuario { get; set; }
        public string? Area { get; set; }
        public string? Puesto { get; set; }
        public string? Departamento { get; set; }
        public string? AlmacenAsignado { get; set; }
    }
}
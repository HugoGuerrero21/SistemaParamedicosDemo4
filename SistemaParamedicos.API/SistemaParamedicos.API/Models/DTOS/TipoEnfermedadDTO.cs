using SistemaParamedicos.API.Models;

namespace SistemaParamedicos.API.DTOs
{
    public class TipoEnfermedadDTO
    {
        public int IdTipoEnfermedad { get; set; }
        public string NombreEnfermedad { get; set; }
        public string IdUsuarioAcc { get; set; }
    }

    // DTO para crear un nuevo tipo de enfermedad
    public class CrearTipoEnfermedadDTO
    {
        public string NombreEnfermedad { get; set; }
        public string IdUsuarioAcc { get; set; }
    }

    // Extensión para convertir entre Model y DTO
    public static class TipoEnfermedadExtensions
    {
        public static TipoEnfermedadDTO ToDTO(this TipoEnfermedadModel model)
        {
            return new TipoEnfermedadDTO
            {
                IdTipoEnfermedad = model.IdTipoEnfermedad,
                NombreEnfermedad = model.NombreEnfermedad,
                IdUsuarioAcc = model.IdUsuarioAcc
            };
        }

        public static TipoEnfermedadModel ToModel(this CrearTipoEnfermedadDTO dto)
        {
            return new TipoEnfermedadModel
            {
                NombreEnfermedad = dto.NombreEnfermedad,
                IdUsuarioAcc = dto.IdUsuarioAcc
            };
        }
    }
}
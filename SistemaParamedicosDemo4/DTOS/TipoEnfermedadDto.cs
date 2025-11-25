using SistemaParamedicosDemo4.MVVM.Models;

namespace SistemaParamedicosDemo4.DTOS
{
    // DTO para recibir tipos de enfermedad desde la API
    public class TipoEnfermedadDto
    {
        public int IdTipoEnfermedad { get; set; }
        public string NombreEnfermedad { get; set; }
        public string IdUsuarioAcc { get; set; }
    }


    // DTO para crear un nuevo tipo de enfermedad

    public class CrearTipoEnfermedadDto
    {
        public string NombreEnfermedad { get; set; }
        public string IdUsuarioAcc { get; set; }
    }

    // Extensiones para convertir entre DTOs y Models
    public static class TipoEnfermedadExtensions
    {

        // Convierte un DTO de la API a un Model de SQLite
        public static TipoEnfermedadModel ToModel(this TipoEnfermedadDto dto)
        {
            return new TipoEnfermedadModel
            {
                IdTipoEnfermedad = dto.IdTipoEnfermedad,
                NombreEnfermedad = dto.NombreEnfermedad,
                ID_USUARIO_ACC = dto.IdUsuarioAcc
            };
        }

        // Convierte un Model de SQLite a un DTO
        public static TipoEnfermedadDto ToDto(this TipoEnfermedadModel model)
        {
            return new TipoEnfermedadDto
            {
                IdTipoEnfermedad = model.IdTipoEnfermedad,
                NombreEnfermedad = model.NombreEnfermedad,
                IdUsuarioAcc = model.ID_USUARIO_ACC
            };
        }
    }
}
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SistemaParamedicos.API.DTOs
{
    // DTO para medicamentos/productos usados en la consulta
    public class MedicamentoConsultaDto
    {
        // ⭐ SIN [Required] para permitir objetos vacíos que serán filtrados
        public string IdProducto { get; set; }

        public float Cantidad { get; set; }

        public string Observaciones { get; set; }
    }

    // DTO para crear una nueva consulta
    public class CrearConsultaDto
    {
        [Required(ErrorMessage = "El ID del empleado es requerido")]
        public string IdEmpleado { get; set; }

        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public string IdUsuarioAcc { get; set; }

        [Required(ErrorMessage = "El tipo de enfermedad es requerido")]
        public int IdTipoEnfermedad { get; set; }

        [Required(ErrorMessage = "El motivo de consulta es requerido")]
        [MaxLength(500)]
        public string MotivoConsulta { get; set; }

        [Required(ErrorMessage = "La fecha de consulta es requerida")]
        public DateTime FechaConsulta { get; set; }

        // Signos vitales (opcionales)
        public byte? FrecuenciaRespiratoria { get; set; }
        public short? FrecuenciaCardiaca { get; set; }
        public decimal? Temperatura { get; set; }

        [MaxLength(30)]
        public string PresionArterial { get; set; }

        [MaxLength(500)]
        public string Observaciones { get; set; }

        [MaxLength(500)]
        public string UltimaComida { get; set; }

        [Required(ErrorMessage = "El diagnóstico es requerido")]
        [MaxLength(150)]
        public string Diagnostico { get; set; }

        // ⭐ LISTA DE MEDICAMENTOS USADOS (puede estar vacía)
        public List<MedicamentoConsultaDto> Medicamentos { get; set; } = new List<MedicamentoConsultaDto>();
    }

    // DTO para devolver una consulta
    public class ConsultaDto
    {
        public int IdConsulta { get; set; }
        public string IdEmpleado { get; set; }
        public string NombreEmpleado { get; set; }
        public string IdUsuarioAcc { get; set; }
        public string NombreUsuario { get; set; }
        public int IdTipoEnfermedad { get; set; }
        public string NombreTipoEnfermedad { get; set; }
        public string IdMovimiento { get; set; }
        public string MotivoConsulta { get; set; }
        public DateTime FechaConsulta { get; set; }
        public byte? FrecuenciaRespiratoria { get; set; }
        public short? FrecuenciaCardiaca { get; set; }
        public decimal? Temperatura { get; set; }
        public string PresionArterial { get; set; }
        public string Observaciones { get; set; }
        public string UltimaComida { get; set; }
        public string Diagnostico { get; set; }
        public List<MedicamentoConsultaDto> Medicamentos { get; set; }
    }

    // DTO simplificado para listados
    public class ConsultaResumenDto
    {
        [JsonPropertyName("idConsulta")]
        public int IdConsulta { get; set; }

        [JsonPropertyName("idEmpleado")]
        public string IdEmpleado { get; set; }

        [JsonPropertyName("nombreEmpleado")]
        public string NombreEmpleado { get; set; }

        [JsonPropertyName("idUsuarioAcc")]
        public string IdUsuarioAcc { get; set; }

        [JsonPropertyName("nombreUsuario")]
        public string NombreUsuario { get; set; }

        [JsonPropertyName("idTipoEnfermedad")]
        public int IdTipoEnfermedad { get; set; }

        [JsonPropertyName("nombreTipoEnfermedad")]
        public string NombreTipoEnfermedad { get; set; }

        [JsonPropertyName("idMovimiento")]
        public string IdMovimiento { get; set; }

        [JsonPropertyName("motivoConsulta")]
        public string MotivoConsulta { get; set; }

        [JsonPropertyName("diagnostico")]
        public string Diagnostico { get; set; }

        [JsonPropertyName("tipoEnfermedad")]
        public string TipoEnfermedad { get; set; }

        [JsonPropertyName("fechaConsulta")]
        public DateTime FechaConsulta { get; set; }

        [JsonPropertyName("frecuenciaRespiratoria")]
        public byte? FrecuenciaRespiratoria { get; set; }

        [JsonPropertyName("frecuenciaCardiaca")]
        public short? FrecuenciaCardiaca { get; set; }

        [JsonPropertyName("temperatura")]
        public decimal? Temperatura { get; set; }

        [JsonPropertyName("presionArterial")]
        public string PresionArterial { get; set; }

        [JsonPropertyName("observaciones")]
        public string Observaciones { get; set; }

        [JsonPropertyName("ultimaComida")]
        public string UltimaComida { get; set; }

        [JsonPropertyName("medicamentos")]
        public List<MedicamentoConsultaDto> Medicamentos { get; set; }
    }
}
using System.Text.Json.Serialization;

namespace SistemaParamedicos.API.DTOs
{
    public class EstadisticaEnfermedadDto
    {
        [JsonPropertyName("idTipoEnfermedad")]
        public int IdTipoEnfermedad { get; set; }

        [JsonPropertyName("nombreEnfermedad")]
        public string NombreEnfermedad { get; set; }

        [JsonPropertyName("cantidad")]
        public int Cantidad { get; set; }

        [JsonPropertyName("porcentaje")]
        public decimal Porcentaje { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }
    }

    public class EstadisticasResponseDto
    {
        [JsonPropertyName("totalConsultas")]
        public int TotalConsultas { get; set; }

        [JsonPropertyName("periodo")]
        public string Periodo { get; set; }

        [JsonPropertyName("fechaInicio")]
        public DateTime FechaInicio { get; set; }

        [JsonPropertyName("fechaFin")]
        public DateTime FechaFin { get; set; }

        [JsonPropertyName("estadisticas")]
        public List<EstadisticaEnfermedadDto> Estadisticas { get; set; }

        [JsonPropertyName("enfermedadMasComun")]
        public string EnfermedadMasComun { get; set; }

        [JsonPropertyName("cantidadMasComun")]
        public int CantidadMasComun { get; set; }

        [JsonPropertyName("promedioDiario")]
        public decimal PromedioDiario { get; set; }
    }
}
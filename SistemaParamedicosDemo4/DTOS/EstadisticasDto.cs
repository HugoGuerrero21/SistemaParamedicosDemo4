using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SistemaParamedicosDemo4.MVVM.Models;

namespace SistemaParamedicosDemo4.DTOS
{
    // DTO para una estadística individual de un tipo de enfermedad
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

    // DTO principal que contiene todas las estadísticas del período
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

    // DTO para los parámetros de consulta de estadísticas
    public class EstadisticasRequestDto
    {
        public int? Mes { get; set; }
        public int Anio { get; set; }
        public bool VerAnual { get; set; } = false;
    }

    // Modelo local para SQLite
    public class EstadisticaModel
    {
        public int IdTipoEnfermedad { get; set; }
        public string NombreEnfermedad { get; set; }
        public int Cantidad { get; set; }
        public decimal Porcentaje { get; set; }
        public string Color { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public DateTime FechaGeneracion { get; set; }
    }

    // Extensiones
    public static class EstadisticasExtensions
    {
        public static EstadisticaModel ToModel(this EstadisticaEnfermedadDto dto, int mes, int anio)
        {
            return new EstadisticaModel
            {
                IdTipoEnfermedad = dto.IdTipoEnfermedad,
                NombreEnfermedad = dto.NombreEnfermedad,
                Cantidad = dto.Cantidad,
                Porcentaje = dto.Porcentaje,
                Color = dto.Color,
                Mes = mes,
                Anio = anio,
                FechaGeneracion = DateTime.Now
            };
        }
    }
}
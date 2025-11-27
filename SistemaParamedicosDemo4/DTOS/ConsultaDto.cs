using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SistemaParamedicosDemo4.MVVM.Models;

namespace SistemaParamedicosDemo4.DTOS
{
    // ============================================
    // DTO para ENVIAR al crear una nueva consulta
    // ============================================
    public class CrearConsultaDto
    {
        [JsonPropertyName("idEmpleado")]
        public string IdEmpleado { get; set; }

        [JsonPropertyName("idUsuarioAcc")]
        public string IdUsuarioAcc { get; set; }

        [JsonPropertyName("idTipoEnfermedad")]
        public int IdTipoEnfermedad { get; set; }

        [JsonPropertyName("motivoConsulta")]
        public string MotivoConsulta { get; set; }

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

        [JsonPropertyName("diagnostico")]
        public string Diagnostico { get; set; }

        [JsonPropertyName("medicamentos")]
        public List<MedicamentoConsultaDto> Medicamentos { get; set; } = new List<MedicamentoConsultaDto>();
    }

    // ============================================
    // DTO para medicamentos/productos usados
    // ============================================
    public class MedicamentoConsultaDto
    {
        [JsonPropertyName("idProducto")]
        public string IdProducto { get; set; }

        [JsonPropertyName("cantidad")]
        public float Cantidad { get; set; }

        [JsonPropertyName("observaciones")]
        public string Observaciones { get; set; }
    }

    // ============================================
    // DTO para RECIBIR la respuesta del servidor
    // ============================================
    public class ConsultaResponseDto
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

        [JsonPropertyName("diagnostico")]
        public string Diagnostico { get; set; }

        [JsonPropertyName("medicamentos")]
        public List<MedicamentoConsultaDto> Medicamentos { get; set; }
    }

    // ============================================
    // DTO para listados de consultas (resumen)
    // ============================================
    public class ConsultaResumenDto
    {
        [JsonPropertyName("idConsulta")]
        public int IdConsulta { get; set; }

        [JsonPropertyName("idEmpleado")]
        public string IdEmpleado { get; set; }

        [JsonPropertyName("nombreEmpleado")]
        public string NombreEmpleado { get; set; }

        [JsonPropertyName("motivoConsulta")]
        public string MotivoConsulta { get; set; }

        [JsonPropertyName("diagnostico")]
        public string Diagnostico { get; set; }

        [JsonPropertyName("tipoEnfermedad")]
        public string TipoEnfermedad { get; set; }

        [JsonPropertyName("fechaConsulta")]
        public DateTime FechaConsulta { get; set; }
    }

    // ============================================
    // DTO para detalle completo de una consulta
    // ============================================
    public class ConsultaDetalleDto
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

        [JsonPropertyName("motivoConsulta")]
        public string MotivoConsulta { get; set; }

        [JsonPropertyName("fechaConsulta")]
        public DateTime FechaConsulta { get; set; }

        [JsonPropertyName("diagnostico")]
        public string Diagnostico { get; set; }
    }

    // ============================================
    // EXTENSIONES para convertir entre DTOs y Models
    // ============================================
    public static class ConsultaExtensions
    {
        /// <summary>
        /// Convierte un DTO de respuesta a un Model de SQLite
        /// </summary>
        public static ConsultaModel ToModel(this ConsultaResponseDto dto)
        {
            return new ConsultaModel
            {
                IdConsulta = dto.IdConsulta,
                IdEmpleado = dto.IdEmpleado,
                IdUsuarioAcc = dto.IdUsuarioAcc,
                IdTipoEnfermedad = dto.IdTipoEnfermedad,
                IdMovimiento = dto.IdMovimiento,
                MotivoConsulta = dto.MotivoConsulta,
                FechaConsulta = dto.FechaConsulta,
                FrecuenciaRespiratoria = (byte)(dto.FrecuenciaRespiratoria ?? 0),
                FrecuenciaCardiaca = (short)(dto.FrecuenciaCardiaca ?? 0),
                Temperatura = dto.Temperatura?.ToString() ?? string.Empty,
                PresionArterial = dto.PresionArterial ?? string.Empty,
                Observaciones = dto.Observaciones ?? string.Empty,
                UltimaComida = dto.UltimaComida ?? string.Empty,
                Diagnostico = dto.Diagnostico
            };
        }

        /// <summary>
        /// Convierte un Model de SQLite a un DTO para enviar
        /// </summary>
        public static CrearConsultaDto ToDto(this ConsultaModel model)
        {
            return new CrearConsultaDto
            {
                IdEmpleado = model.IdEmpleado,
                IdUsuarioAcc = model.IdUsuarioAcc,
                IdTipoEnfermedad = model.IdTipoEnfermedad,
                MotivoConsulta = model.MotivoConsulta,
                FechaConsulta = model.FechaConsulta,
                FrecuenciaRespiratoria = model.FrecuenciaRespiratoria == 0 ? null : (byte?)model.FrecuenciaRespiratoria,
                FrecuenciaCardiaca = model.FrecuenciaCardiaca == 0 ? null : (short?)model.FrecuenciaCardiaca,
                Temperatura = string.IsNullOrWhiteSpace(model.Temperatura) ? null : decimal.Parse(model.Temperatura),
                PresionArterial = model.PresionArterial,
                Observaciones = model.Observaciones,
                UltimaComida = model.UltimaComida,
                Diagnostico = model.Diagnostico,
                Medicamentos = new List<MedicamentoConsultaDto>()
            };
        }
    }
}
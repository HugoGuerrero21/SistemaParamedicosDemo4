using System;
using System.Text.Json.Serialization;
using SistemaParamedicosDemo4.MVVM.Models;

namespace SistemaParamedicosDemo4.DTOS
{
    public class PuestoDTO
    {
        [JsonPropertyName("idPuesto")]
        public string IdPuesto { get; set; }

        [JsonPropertyName("idDepartamento")]
        public string IdDepartamento { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("fecha")]
        public DateTime Fecha { get; set; }

        public PuestoModel ToPuestoModel()
        {
            return new PuestoModel
            {
                IdPuesto = this.IdPuesto,
                IdDepartamento = this.IdDepartamento ?? "",
                Nombre = this.Nombre,
                Fecha = this.Fecha
            };
        }
    }
}
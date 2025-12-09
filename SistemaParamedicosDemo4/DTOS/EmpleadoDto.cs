using System;
using System.Text.Json.Serialization;
using SistemaParamedicosDemo4.MVVM.Models;

namespace SistemaParamedicosDemo4.DTOS
{
    public class EmpleadoDto
    {
        [JsonPropertyName("idEmpleado")]
        public string IdEmpleado { get; set; }

        [JsonPropertyName("rfid")]
        public string Rfid { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("sexo")]
        public string Sexo { get; set; }

        [JsonPropertyName("telefono")]
        public string Telefono { get; set; }

        [JsonPropertyName("alergias")]
        public string Alergias { get; set; }

        [JsonPropertyName("tipoSangre")]
        public string TipoSangre { get; set; }

        [JsonPropertyName("idPuesto")]
        public string IdPuesto { get; set; }

        [JsonPropertyName("idDepartamento")]
        public string IdDepartamento { get; set; }

        [JsonPropertyName("idArea")]
        public string IdArea { get; set; }

        [JsonPropertyName("nacimiento")]
        public DateTime? Nacimiento { get; set; }

        [JsonPropertyName("foto")]
        public string Foto { get; set; }

        [JsonPropertyName("estado")]
        public string Estado { get; set; }

        //NUEVO: Incluir objeto Puesto completo
        [JsonPropertyName("puesto")]
        public PuestoDTO Puesto { get; set; }

        /// <summary>
        /// Convierte el DTO de la API al modelo local de EmpleadoModel
        /// </summary>
        public EmpleadoModel ToEmpleadoModel()
        {
            return new EmpleadoModel
            {
                IdEmpleado = this.IdEmpleado,
                Nombre = this.Nombre,
                TipoSangre = this.TipoSangre ?? "N/A",
                Sexo = this.Sexo,
                AlergiasSangre = this.Alergias ?? "Ninguna",
                Telefono = this.Telefono ?? "",
                FechaNacimiento = this.Nacimiento ?? DateTime.Now.AddYears(-30),
                IdPuesto = this.IdPuesto,
                NombrePuesto = this.Puesto?.Nombre ?? this.IdPuesto,
                Estatus = this.Estado?? "ACTIVO",
                Foto = this.Foto

            };
        }



    }
}
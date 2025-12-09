using SQLite;
using System;

namespace SistemaParamedicosDemo4.MVVM.Models
{
    public class EmpleadoModel
    {
        [PrimaryKey, MaxLength(30)]
        public string IdEmpleado { get; set; }

        [MaxLength(150)]
        public string Nombre { get; set; }

        [MaxLength(5)]
        public string TipoSangre { get; set; }

        [MaxLength(15)]
        public string Sexo { get; set; }

        [MaxLength(255)]
        public string AlergiasSangre { get; set; }

        [MaxLength(12)]
        public string Telefono { get; set; }

        [MaxLength(20)]
        public string Estatus { get; set; }

        public DateTime FechaNacimiento { get; set; }

        [MaxLength(25)]
        public string IdPuesto { get; set; }

        [MaxLength(500)]
        public string Foto { get; set; }

        //Nombre del puesto (NO se guarda en BD)
        [Ignore]
        public string NombrePuesto { get; set; }

        // ⭐ Propiedades calculadas - NO se guardan en BD
        [Ignore]
        public string Iniciales
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Nombre))
                    return "??";

                var palabras = Nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (palabras.Length >= 2)
                    return $"{palabras[0][0]}{palabras[1][0]}".ToUpper();

                return palabras[0].Length >= 2
                    ? palabras[0].Substring(0, 2).ToUpper()
                    : palabras[0][0].ToString().ToUpper();
            }
        }

        [Ignore]
        public bool TieneFoto => !string.IsNullOrWhiteSpace(Foto) &&
                         (Foto.StartsWith("http://") || Foto.StartsWith("https://"));

        [Ignore]
        public string FotoUrl => TieneFoto ? Foto : null;

        [Ignore]
        public int TotalConsultas { get; set; }


    }
}
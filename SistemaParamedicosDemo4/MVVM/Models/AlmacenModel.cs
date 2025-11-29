using SQLite;

namespace SistemaParamedicosDemo4.MVVM.Models
{
    /// <summary>
    /// Modelo para almacenar información de almacenes
    /// Tabla: CAAD_ALMACENES
    /// </summary>
    [Table("CAAD_ALMACENES")]
    public class AlmacenModel
    {
        [PrimaryKey]
        [MaxLength(45)]
        [Column("ID_ALMACEN")]
        public string IdAlmacen { get; set; }

        [MaxLength(100)]
        [Column("NOMBRE")]
        public string Nombre { get; set; }

        [MaxLength(100)]
        [Column("COLUMNA_STOCK")]
        public string ColumnaStock { get; set; }

        /// Verifica si este es el almacén de paramédicos
        [Ignore]
        public bool EsAlmacenParamedicos => IdAlmacen == "ALM6";


        /// Verifica si este es el almacén de Hermosillo

        [Ignore]
        public bool EsAlmacenHermosillo => IdAlmacen == "ALM1";
    }
}
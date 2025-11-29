using SQLite;
using System;
using System.Collections.Generic;

namespace SistemaParamedicosDemo4.MVVM.Models
{
    /// <summary>
    /// Modelo para almacenar traspasos en SQLite local
    /// </summary>
    [Table("MOAD_TRASPALMACEN")]
    public class TraspasoModel
    {
        [PrimaryKey]
        [MaxLength(25)]
        [Column("ID_TRASPASO")]
        public string IdTraspaso { get; set; }

        [MaxLength(45)]
        [Column("ID_USUARIOO")]
        public string IdUsuarioOrigen { get; set; }

        [MaxLength(45)]
        [Column("ALMACEN_ORIGEN")]
        public string AlmacenOrigen { get; set; }

        [MaxLength(45)]
        [Column("ALMACEN_DESTINO")]
        public string AlmacenDestino { get; set; }

        [MaxLength(30)]
        [Column("ID_EMPLEADO")]
        public string IdEmpleado { get; set; }

        [Column("FECHA_ENVIO")]
        public DateTime FechaEnvio { get; set; }

        [Column("FECHA_RECEPCION")]
        public DateTime? FechaRecepcion { get; set; }

        [Column("FECHA_COMPLETADO")]
        public DateTime? FechaCompletado { get; set; }

        [MaxLength(45)]
        [Column("ID_USUARIOD")]
        public string IdUsuarioDestino { get; set; }

        [Column("STATUS")]
        public byte Status { get; set; }

        [MaxLength(45)]
        [Column("ID_UNIDAD")]
        public string IdUnidad { get; set; }

        [Column("FECHA_CANCELACION")]
        public DateTime? FechaCancelacion { get; set; }

        [MaxLength(45)]
        [Column("ID_USUARIO_CANCELA")]
        public string IdUsuarioCancela { get; set; }

        // Propiedades de navegación
        [Ignore]
        public EmpleadoModel Empleado { get; set; }

        [Ignore]
        public List<TraspasoDetalleModel> Detalles { get; set; } = new List<TraspasoDetalleModel>();
    }

    /// <summary>
    /// Modelo para los detalles de cada traspaso
    /// </summary>
    [Table("MDAD_TRASPALMACEN")]
    public class TraspasoDetalleModel
    {
        [PrimaryKey]
        [MaxLength(25)]
        [Column("ID_TRASPASODETALLE")]
        public string IdTraspasoDetalle { get; set; }

        [MaxLength(25)]
        [Column("ID_TRASPASO")]
        public string IdTraspaso { get; set; }

        [MaxLength(45)]
        [Column("ID_PRODUCTO")]
        public string IdProducto { get; set; }

        [Column("CANTIDAD")]
        public float Cantidad { get; set; }

        [MaxLength(45)]
        [Column("ID_PROVEEDOR")]
        public string IdProveedor { get; set; }

        [Column("CANTIDAD_RECIBIDA")]
        public float? CantidadRecibida { get; set; }

        [MaxLength(45)]
        [Column("SALIDA_PADRE")]
        public string SalidaPadre { get; set; }

        [MaxLength(1000)]
        [Column("ENTRADA_HIJA")]
        public string EntradaHija { get; set; }

        [Column("COMPLETADA")]
        public byte Completada { get; set; }

        [Column("FECHA_COMPLETADO")]
        public DateTime? FechaCompletado { get; set; }

        [MaxLength(200)]
        [Column("MOTIVO_CANCELACION")]
        public string MotivoCancelacion { get; set; }

        // Propiedades de navegación
        [Ignore]
        public ProductoModel Producto { get; set; }

        [Ignore]
        public TraspasoModel Traspaso { get; set; }
    }
}
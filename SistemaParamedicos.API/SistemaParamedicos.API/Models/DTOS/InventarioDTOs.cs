namespace SistemaParamedicos.API.DTOs
{
    public class ExistenciaDTO
    {
        public string Producto { get; set; }
        public string NombreDelProducto { get; set; }
        public string? Descripcion { get; set; }
        public string? Marca { get; set; }
        public string? NumeroDePieza { get; set; }
        public double Entrada { get; set; }
        public double Salida { get; set; }
        public double Existencia { get; set; }
        public string? Foto { get; set; }
    }

    public class MovimientoDTO
    {
        public string IdMovimiento { get; set; }
        public string IdTipoMovimiento { get; set; }
        public string TipoMovimientoNombre { get; set; }
        public string IdAlmacen { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string? IdEmpleado { get; set; }
        public string? NombreEmpleado { get; set; }
        public int Status { get; set; }
        public string IdUsuario { get; set; }
        public List<MovimientoDetalleDTO> Detalles { get; set; }
    }

    public class MovimientoDetalleDTO
    {
        public string IdMovimientoDetalles { get; set; }
        public string IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public float Cantidad { get; set; }
        public string? PrecioFinal { get; set; }
        public int Status { get; set; }
        public float? CantidadUtilizada { get; set; }
    }

    public class CrearSalidaDTO
    {
        public string IdEmpleado { get; set; }
        public string IdUsuario { get; set; }
        public List<DetalleSalidaDTO> Productos { get; set; }
    }

    public class DetalleSalidaDTO
    {
        public string IdProducto { get; set; }
        public float Cantidad { get; set; }
    }

    public class MovimientoResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? IdMovimiento { get; set; }
    }
}
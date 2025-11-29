using System.ComponentModel.DataAnnotations;

namespace SistemaParamedicos.API.DTOs
{
    // DTO para el detalle de un traspaso
    public class TraspasoDetalleDTO
    {
        public string IdTraspasoDetalle { get; set; }
        public string IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public string Descripcion { get; set; }
        public string NumeroPieza { get; set; }
        public decimal Cantidad { get; set; } 
        public decimal CantidadRecibida { get; set; } 
        public decimal CantidadFaltante { get; set; } 
        public decimal CantidadARecibir { get; set; } 
        public byte Completada { get; set; }
        public DateTime? FechaCompletado { get; set; }
        public string? IdProveedor { get; set; }
    }

    // DTO para listar traspasos pendientes
    public class TraspasoPendienteDto
    {
        public string IdTraspaso { get; set; }
        public string AlmacenOrigen { get; set; }
        public string NombreAlmacenOrigen { get; set; }
        public string AlmacenDestino { get; set; }
        public string NombreAlmacenDestino { get; set; }
        public DateTime FechaEnvio { get; set; }
        public string? NombreChofer { get; set; }
        public string? NoEconomico { get; set; }
        public byte Status { get; set; }
        public string StatusTexto { get; set; }
        public List<TraspasoDetalleDTO> Detalles { get; set; } = new List<TraspasoDetalleDTO>();
    }

    // DTO para recibir (completar) un detalle individual
    public class CompletarDetalleDto
    {
        [Required(ErrorMessage = "El ID del detalle es requerido")]
        public string IdTraspasoDetalle { get; set; }

        [Required(ErrorMessage = "La cantidad recibida es requerida")]
        [Range(0.01, float.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public float CantidadRecibida { get; set; }

        [Required(ErrorMessage = "El ID del usuario receptor es requerido")]
        public string IdUsuarioReceptor { get; set; }
    }

    // DTO para recibir múltiples detalles a la vez
    public class CompletarTraspasoDto
    {
        [Required(ErrorMessage = "El ID del traspaso es requerido")]
        public string IdTraspaso { get; set; }

        [Required(ErrorMessage = "El ID del usuario receptor es requerido")]
        public string IdUsuarioReceptor { get; set; }

        [Required(ErrorMessage = "Debe incluir al menos un detalle")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un detalle")]
        public List<DetalleRecepcionDto> Detalles { get; set; } = new List<DetalleRecepcionDto>();
    }

    public class DetalleRecepcionDto
    {
        [Required]
        public string IdTraspasoDetalle { get; set; }

        [Required]
        [Range(0.01, float.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public float CantidadRecibida { get; set; }
    }

    // DTO para respuesta de operaciones
    public class TraspasoResultadoDto
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public string? IdMovimiento { get; set; }
        public Dictionary<string, string>? DetallesEntrada { get; set; }
    }

    public class RechazarDetalleDto
    {
        [Required(ErrorMessage = "El ID del detalle es requerido")]
        public string IdTraspasoDetalle { get; set; }

        [Required(ErrorMessage = "El ID del usuario receptor es requerido")]
        public string IdUsuarioReceptor { get; set; }
    }
}
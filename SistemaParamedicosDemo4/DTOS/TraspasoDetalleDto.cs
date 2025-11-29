using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using SistemaParamedicosDemo4.MVVM.Models;

namespace SistemaParamedicosDemo4.DTOS
{
    /// <summary>
    /// DTO para el detalle de un traspaso (desde la API)
    /// </summary>
    public class TraspasoDetalleDto : INotifyPropertyChanged
    {
        [JsonPropertyName("idTraspasoDetalle")]
        public string IdTraspasoDetalle { get; set; }

        [JsonPropertyName("idProducto")]
        public string IdProducto { get; set; }

        [JsonPropertyName("nombreProducto")]
        public string NombreProducto { get; set; }

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("numeroPieza")]
        public string NumeroPieza { get; set; }

        [JsonPropertyName("cantidad")]
        public float Cantidad { get; set; }

        [JsonPropertyName("cantidadRecibida")]
        public float CantidadRecibida { get; set; }

        [JsonPropertyName("cantidadFaltante")]
        public float CantidadFaltante { get; set; }

        private float _cantidadARecibir;
        [JsonPropertyName("cantidadARecibir")]
        public float CantidadARecibir
        {
            get => _cantidadARecibir;
            set
            {
                _cantidadARecibir = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PuedeCompletar));
                OnPropertyChanged(nameof(ExcedeCantidad));
            }
        }

        [JsonPropertyName("completada")]
        public byte Completada { get; set; }

        [JsonPropertyName("fechaCompletado")]
        public DateTime? FechaCompletado { get; set; }

        [JsonPropertyName("idProveedor")]
        public string IdProveedor { get; set; }

        // Propiedades calculadas
        [JsonIgnore]
        public bool EstaCompletado => Completada == 1;

        [JsonIgnore]
        public bool MostrarBotonesAccion => Completada == 0; // Solo mostrar si NO está completado

        [JsonIgnore]
        public bool PuedeCompletar => Completada == 0 && CantidadARecibir > 0;

        [JsonIgnore]
        public bool ExcedeCantidad => (CantidadRecibida + CantidadARecibir) > Cantidad;

        [JsonIgnore]
        public string ColorEstado
        {
            get
            {
                if (EstaCompletado) return "#28A745"; // Verde
                if (CantidadRecibida > 0) return "#FFC107"; // Amarillo
                return "#DC3545"; // Rojo
            }
        }

        [JsonIgnore]
        public string TextoEstado
        {
            get
            {
                if (Completada == 1) return "Completado";
                if (CantidadRecibida > 0) return "Parcial";
                return "Pendiente";
            }
        }

        [JsonIgnore]
        public string NombreCompletoProducto =>
            !string.IsNullOrEmpty(NumeroPieza)
                ? $"{NombreProducto} ({NumeroPieza})"
                : NombreProducto;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Convierte el DTO a un modelo de SQLite
        /// </summary>
        public TraspasoDetalleModel ToModel()
        {
            return new TraspasoDetalleModel
            {
                IdTraspasoDetalle = this.IdTraspasoDetalle,
                IdProducto = this.IdProducto,
                Cantidad = this.Cantidad,
                CantidadRecibida = this.CantidadRecibida,
                Completada = this.Completada,
                FechaCompletado = this.FechaCompletado,
                IdProveedor = this.IdProveedor
            };
        }
    }

    /// <summary>
    /// DTO para listar traspasos pendientes (desde la API)
    /// </summary>
    public class TraspasoPendienteDto : INotifyPropertyChanged
    {
        [JsonPropertyName("idTraspaso")]
        public string IdTraspaso { get; set; }

        [JsonPropertyName("almacenOrigen")]
        public string AlmacenOrigen { get; set; }

        [JsonPropertyName("nombreAlmacenOrigen")]
        public string NombreAlmacenOrigen { get; set; }

        [JsonPropertyName("almacenDestino")]
        public string AlmacenDestino { get; set; }

        [JsonPropertyName("nombreAlmacenDestino")]
        public string NombreAlmacenDestino { get; set; }

        [JsonPropertyName("fechaEnvio")]
        public DateTime FechaEnvio { get; set; }

        [JsonPropertyName("nombreChofer")]
        public string NombreChofer { get; set; }

        [JsonPropertyName("noEconomico")]
        public string NoEconomico { get; set; }

        [JsonPropertyName("status")]
        public byte Status { get; set; }

        [JsonPropertyName("statusTexto")]
        public string StatusTexto { get; set; }

        [JsonPropertyName("detalles")]
        public List<TraspasoDetalleDto> Detalles { get; set; } = new List<TraspasoDetalleDto>();

        // Propiedades calculadas
        [JsonIgnore]
        public string FechaEnvioFormateada => FechaEnvio.ToString("dd/MM/yyyy HH:mm");

        [JsonIgnore]
        public int TotalProductos => Detalles?.Count ?? 0;

        [JsonIgnore]
        public int ProductosCompletados => Detalles?.Count(d => d.Completada == 1) ?? 0;

        [JsonIgnore]
        public int ProductosPendientes => TotalProductos - ProductosCompletados;

        [JsonIgnore]
        public string ProgresoTexto => $"{ProductosCompletados}/{TotalProductos} completados";

        [JsonIgnore]
        public double PorcentajeProgreso =>
            TotalProductos > 0 ? (double)ProductosCompletados / TotalProductos : 0;

        [JsonIgnore]
        public string ColorStatus
        {
            get
            {
                return Status switch
                {
                    0 => "#FFC107", // Pendiente - Amarillo
                    1 => "#28A745", // Completado - Verde
                    2 => "#DC3545", // Cancelado - Rojo
                    _ => "#6C757D"  // Desconocido - Gris
                };
            }
        }

        private bool _expandido;
        [JsonIgnore]
        public bool Expandido
        {
            get => _expandido;
            set
            {
                _expandido = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IconoExpandir));
            }
        }

        [JsonIgnore]
        public string IconoExpandir => Expandido ? "▼" : "▶";

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Convierte el DTO a un modelo de SQLite
        /// </summary>
        public TraspasoModel ToModel()
        {
            return new TraspasoModel
            {
                IdTraspaso = this.IdTraspaso,
                AlmacenOrigen = this.AlmacenOrigen,
                AlmacenDestino = this.AlmacenDestino,
                FechaEnvio = this.FechaEnvio,
                Status = this.Status
            };
        }
    }

    /// <summary>
    /// DTO para completar un detalle individual (enviar a la API)
    /// </summary>
    public class CompletarDetalleDto
    {
        [JsonPropertyName("idTraspasoDetalle")]
        public string IdTraspasoDetalle { get; set; }

        [JsonPropertyName("cantidadRecibida")]
        public float CantidadRecibida { get; set; }

        [JsonPropertyName("idUsuarioReceptor")]
        public string IdUsuarioReceptor { get; set; }
    }

    /// <summary>
    /// DTO para completar múltiples detalles (enviar a la API)
    /// </summary>
    public class CompletarTraspasoDto
    {
        [JsonPropertyName("idTraspaso")]
        public string IdTraspaso { get; set; }

        [JsonPropertyName("idUsuarioReceptor")]
        public string IdUsuarioReceptor { get; set; }

        [JsonPropertyName("detalles")]
        public List<DetalleRecepcionDto> Detalles { get; set; } = new List<DetalleRecepcionDto>();
    }

    public class DetalleRecepcionDto
    {
        [JsonPropertyName("idTraspasoDetalle")]
        public string IdTraspasoDetalle { get; set; }

        [JsonPropertyName("cantidadRecibida")]
        public float CantidadRecibida { get; set; }
    }

    /// <summary>
    /// DTO para respuesta de operaciones (desde la API)
    /// </summary>
    public class TraspasoResultadoDto
    {
        [JsonPropertyName("exito")]
        public bool Exito { get; set; }

        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; }

        [JsonPropertyName("idMovimiento")]
        public string IdMovimiento { get; set; }

        [JsonPropertyName("detallesEntrada")]
        public Dictionary<string, string> DetallesEntrada { get; set; }
    }

    public class RechazarDetalleDto
    {
        [JsonPropertyName("idTraspasoDetalle")]
        public string IdTraspasoDetalle { get; set; }

        [JsonPropertyName("idUsuarioReceptor")]
        public string IdUsuarioReceptor { get; set; }
    }
}
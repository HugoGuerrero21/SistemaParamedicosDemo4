using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SistemaParamedicosDemo4.MVVM.Models
{
    public class MovimientoDetalleModel : INotifyPropertyChanged
    {
        public string IdMovimientDetalles { get; set; }
        public string IdMovimiento { get; set; }
        public string ClaveProducto { get; set; }

        private double _cantidad;
        public double Cantidad
        {
            get => _cantidad;
            set
            {
                _cantidad = value;
                OnPropertyChanged();
            }
        }

        public string IdLocacion { get; set; }
        public string PrecioFinal { get; set; }
        public byte Status { get; set; }
        public double CantidadUtilizada { get; set; }
        public string IdDetallePadre { get; set; }

        // Propiedad de navegación (no se guarda en BD, solo para uso en la UI)
        private ProductoModel _producto;
        public ProductoModel Producto
        {
            get => _producto;
            set
            {
                _producto = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NombreMedicamento));
            }
        }

        // Propiedad para observaciones (temporal, no se guarda en este modelo)
        private string _observaciones;
        public string Observaciones
        {
            get => _observaciones;
            set
            {
                _observaciones = value;
                OnPropertyChanged();
            }
        }

        // Propiedad calculada para mostrar en la lista
        public string NombreMedicamento => Producto != null
            ? $"{Producto.Nombre} {Producto.Model}"
            : ClaveProducto;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
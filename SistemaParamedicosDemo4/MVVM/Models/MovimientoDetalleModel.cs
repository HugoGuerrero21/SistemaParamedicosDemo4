using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SistemaParamedicosDemo4.MVVM.Models
{
    [Table("t_movimientosdetalles")]
    public class MovimientoDetalleModel : INotifyPropertyChanged
    {
        [PrimaryKey]
        [MaxLength(25)]
        public string IdMovimientoDetalle { get; set; }

        [MaxLength(25)]
        public string IdMovimiento { get; set; }

        [MaxLength(25)]
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

        [MaxLength(45)]
        public string IdLocacion { get; set; }

        [MaxLength(45)]
        public string PrecioFinal { get; set; }

        public byte Status { get; set; }

        public double CantidadUtilizada { get; set; }

        [MaxLength(45)]
        public string IdDetallePadre { get; set; }

        // Propiedades de navegación
        private ProductoModel _producto;
        [Ignore]
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

        [Ignore]
        public string NombreMedicamento
        {
            get
            {
                if (Producto != null)
                {
                    // Retornar el nombre completo con modelo y marca
                    return $"{Producto.Nombre} {Producto.Model} - {Producto.Marca}";
                }
                // Si no hay producto cargado, mostrar solo la clave
                return ClaveProducto ?? "Medicamento desconocido";
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
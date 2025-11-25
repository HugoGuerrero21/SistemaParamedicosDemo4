// MVVM/Models/ProductoModel.cs
using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SistemaParamedicosDemo4.MVVM.Models
{
    public class ProductoModel : INotifyPropertyChanged
    {
        [PrimaryKey]
        [MaxLength(25)]
        public string ProductoId { get; set; }

        [MaxLength(100)]
        public string Nombre { get; set; }

        [MaxLength(45)]
        public string Marca { get; set; }

        [MaxLength(45)]
        public string Model { get; set; }

        [MaxLength(1000)]
        public string Descripcion { get; set; }

        [MaxLength(45)]
        public string NumeroPieza { get; set; }

        [MaxLength(500)]
        public string Foto { get; set; }

        private double _cantidadDisponible;
        public double CantidadDisponible
        {
            get => _cantidadDisponible;
            set
            {
                _cantidadDisponible = value;
                OnPropertyChanged();
            }
        }

        // Propiedad calculada para mostrar en el Picker
        [Ignore]
        public string NombreCompleto => !string.IsNullOrEmpty(Marca)
            ? $"{Nombre} - {Marca}"
            : Nombre;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
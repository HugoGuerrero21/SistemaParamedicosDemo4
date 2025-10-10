using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SistemaParamedicosDemo4.MVVM.Models
{
    public class ProductoModel : INotifyPropertyChanged
    {
        public string ProductoId { get; set; }
        public string Nombre { get; set; }
        public string Marca { get; set; }
        public string Model { get; set; }
        public string Descripcion { get; set; }

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
        public string NombreCompleto => $"{Nombre} {Model} - {Marca}";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
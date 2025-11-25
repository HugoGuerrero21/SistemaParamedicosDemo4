using PropertyChanged;
using SistemaParamedicosDemo4.Data.Repositories;
using SistemaParamedicosDemo4.MVVM.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SistemaParamedicosDemo4.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    [QueryProperty(nameof(Empleado), "Empleado")]
    public class HistorialConsultasViewModel : INotifyPropertyChanged
    {
        #region Repositorios
        private ConsultaRepository _consultaRepo;
        private MovimientoDetalleRepository _movimientoDetalleRepo;
        private ProductoRepository _productoRepo; // ⭐ AGREGAR ESTE
        #endregion

        #region Properties
        private EmpleadoModel _empleado;
        public EmpleadoModel Empleado
        {
            get => _empleado;
            set
            {
                _empleado = value;
                OnPropertyChanged();
                if (value != null)
                {
                    CalcularEdadEmpleado();
                    CargarConsultas();
                }
            }
        }

        public ObservableCollection<ConsultaModelExtendido> Consultas { get; set; }
        public int TotalConsultas { get; set; }
        public bool IsBusy { get; set; }
        public int EdadEmpleado { get; set; }
        #endregion

        #region Commands
        public ICommand VerDetalleCommand { get; }
        public ICommand ToggleExpandidoCommand { get; }
        #endregion

        public HistorialConsultasViewModel()
        {
            _consultaRepo = new ConsultaRepository();
            _movimientoDetalleRepo = new MovimientoDetalleRepository();
            _productoRepo = new ProductoRepository(); // ⭐ INICIALIZAR
            Consultas = new ObservableCollection<ConsultaModelExtendido>();
            VerDetalleCommand = new Command<ConsultaModelExtendido>(VerDetalle);
            ToggleExpandidoCommand = new Command<ConsultaModelExtendido>(OnToggleExpandido);
        }

        private void CalcularEdadEmpleado()
        {
            if (Empleado != null)
            {
                var hoy = DateTime.Today;
                var edad = hoy.Year - Empleado.FechaNacimiento.Year;
                if (Empleado.FechaNacimiento.Date > hoy.AddYears(-edad))
                    edad--;

                EdadEmpleado = edad;
                System.Diagnostics.Debug.WriteLine($"✓ Edad calculada: {EdadEmpleado} años");
            }
        }

        private void CargarConsultas()
        {
            try
            {
                IsBusy = true;
                Consultas.Clear();

                // ✅ CAMBIO: Cargar solo consultas sin medicamentos
                var consultas = _consultaRepo.GetConsultasCompletasPorEmpleado(Empleado.IdEmpleado);

                foreach (var consulta in consultas)
                {
                    var consultaExtendida = new ConsultaModelExtendido
                    {
                        IdConsulta = consulta.IdConsulta,
                        IdEmpleado = consulta.IdEmpleado,
                        Empleado = consulta.Empleado,
                        IdUsuarioAcceso = consulta.IdUsuarioAcceso,
                        UsuariosAcceso = consulta.UsuariosAcceso,
                        IdTipoEnfermedad = consulta.IdTipoEnfermedad,
                        TipoEnfermedad = consulta.TipoEnfermedad,
                        IdMovimiento = consulta.IdMovimiento,
                        FrecuenciaRespiratoria = consulta.FrecuenciaRespiratoria,
                        FrecuenciaCardiaca = consulta.FrecuenciaCardiaca,
                        Temperatura = consulta.Temperatura,
                        PresionArterial = consulta.PresionArterial,
                        Observaciones = consulta.Observaciones,
                        UltimaComida = consulta.UltimaComida,
                        MotivoConsulta = consulta.MotivoConsulta,
                        FechaConsulta = consulta.FechaConsulta,
                        Diagnostico = consulta.Diagnostico,
                        TieneMovimiento = !string.IsNullOrEmpty(consulta.IdMovimiento),
                        EstaExpandido = false,
                        // ✅ NO cargar medicamentos aún
                        Medicamentos = new ObservableCollection<MovimientoDetalleModel>(),
                        TieneMedicamentos = false
                    };

                    consultaExtendida.TieneObservaciones = !string.IsNullOrWhiteSpace(consulta.Observaciones);

                    Consultas.Add(consultaExtendida);
                }

                TotalConsultas = Consultas.Count;
                System.Diagnostics.Debug.WriteLine($"✓ {TotalConsultas} consultas cargadas (sin medicamentos)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar consultas: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void VerDetalle(ConsultaModelExtendido consulta)
        {
            if (consulta == null) return;

            var parametros = new Dictionary<string, object>
            {
                { "IdConsulta", consulta.IdConsulta }
            };

            await Shell.Current.GoToAsync("detalleConsulta", parametros);
        }

        // ✅ NUEVO COMANDO: Cargar medicamentos cuando se expande
        private void OnToggleExpandido(ConsultaModelExtendido consulta)
        {
            if (consulta == null) return;

            if (!consulta.EstaExpandido && !consulta.TieneMedicamentos && !string.IsNullOrEmpty(consulta.IdMovimiento))
            {
                // ✅ Solo cargar medicamentos cuando se expande
                CargarMedicamentosDelayado(consulta);
            }

            consulta.EstaExpandido = !consulta.EstaExpandido;
        }

        private async void CargarMedicamentosDelayado(ConsultaModelExtendido consulta)
        {
            try
            {
                var medicamentos = _movimientoDetalleRepo.GetDetallesByMovimiento(consulta.IdMovimiento);

                // ✅ Cargar el nombre del producto
                foreach (var medicamento in medicamentos)
                {
                    var producto = _productoRepo.GetProductosById(medicamento.ClaveProducto);
                    if (producto != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"✓ Producto: {producto.ProductoId} -> {producto.Nombre}");
                        // ✅ Asignar el producto completo temporalmente
                        medicamento.Producto = producto;
                    }
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    consulta.Medicamentos = new ObservableCollection<MovimientoDetalleModel>(medicamentos);
                    consulta.TieneMedicamentos = medicamentos.Count > 0;
                });

                System.Diagnostics.Debug.WriteLine($"✓ {medicamentos.Count} medicamentos cargados");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ⭐ CLASE EXTENDIDA CON ACORDEÓN Y MEDICAMENTOS
    public class ConsultaModelExtendido : ConsultaModel, INotifyPropertyChanged
    {
        private bool _estaExpandido;
        public bool EstaExpandido
        {
            get => _estaExpandido;
            set
            {
                _estaExpandido = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IconoExpandido));
            }
        }

        public string IconoExpandido => EstaExpandido ? "▼" : "▶";

        public bool TieneMovimiento { get; set; }
        public bool TieneObservaciones { get; set; }
        public bool TieneMedicamentos { get; set; }

        // ⭐ LISTA DE MEDICAMENTOS
        public ObservableCollection<MovimientoDetalleModel> Medicamentos { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
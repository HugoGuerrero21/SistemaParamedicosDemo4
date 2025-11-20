using PropertyChanged;
using SistemaParamedicosDemo4.Data.Repositories;
using SistemaParamedicosDemo4.MVVM.Models;
using SistemaParamedicosDemo4.MVVM.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SistemaParamedicosDemo4.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class EmpleadosListViewModel : IQueryAttributable
    {
        #region Repositorios
        private EmpleadoRepository _empleadoRepo;
        private ConsultaRepository _consultaRepo;
        #endregion

        #region Properties
        public ObservableCollection<EmpleadoModel> EmpleadosFiltrados { get; set; }
        private List<EmpleadoModel> _todosLosEmpleados;
        public string TextoBusqueda { get; set; }
        public int TotalEmpleados { get; set; }
        public bool IsBusy { get; set; }
        #endregion

        #region Commands
        public ICommand BuscarCommand { get; }
        public ICommand MostrarTodosCommand { get; }
        public ICommand FiltrarPorPuestoCommand { get; }
        public ICommand VerHistorialCommand { get; }
        public ICommand AppearingCommand { get; } // ⭐ COMANDO PARA OnAppearing
        #endregion

        public EmpleadosListViewModel()
        {
            _empleadoRepo = new EmpleadoRepository();
            _consultaRepo = new ConsultaRepository();
            EmpleadosFiltrados = new ObservableCollection<EmpleadoModel>();

            // Inicializar Commands
            BuscarCommand = new Command(Buscar);
            MostrarTodosCommand = new Command(MostrarTodos);
            FiltrarPorPuestoCommand = new Command<string>(FiltrarPorPuesto);
            VerHistorialCommand = new Command<EmpleadoModel>(VerHistorial);
            AppearingCommand = new Command(OnAppearing); // ⭐ NUEVO

            MessagingCenter.Subscribe<ConsultaViewModel, string>(this, "ConsultaGuardada", (sender, idEmpleado) =>
            {
                System.Diagnostics.Debug.WriteLine($"📩 Mensaje recibido en VM: actualizar empleado {idEmpleado}");
                RefrescarEmpleados();
            });

            // Cargar empleados inicial
            CargarEmpleados();
        }

        // ⭐ IMPLEMENTACIÓN DE IQueryAttributable
        // Se ejecuta cada vez que se navega a esta vista
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            System.Diagnostics.Debug.WriteLine("🔄 ApplyQueryAttributes - Refrescando empleados...");
            RefrescarEmpleados();
        }

        // ⭐ MÉTODO PARA EL COMANDO APPEARING
        private void OnAppearing()
        {
            System.Diagnostics.Debug.WriteLine("👁️ Vista apareció - Refrescando empleados...");
            RefrescarEmpleados();
        }

        private void CargarEmpleados()
        {
            try
            {
                IsBusy = true;

                var empleados = _empleadoRepo.GetAll();

                _todosLosEmpleados = empleados.Select(e =>
                {
                    // Cargar el total de consultas para cada empleado
                    var consultas = _consultaRepo.GetConsultasByEmpleado(e.IdEmpleado);
                    e.TotalConsultas = consultas.Count;

                    System.Diagnostics.Debug.WriteLine($"Empleado: {e.Nombre} - Consultas: {consultas.Count}");

                    return e;
                }).ToList();

                MostrarTodos();
                TotalEmpleados = _todosLosEmpleados.Count;

                System.Diagnostics.Debug.WriteLine($"✓ {TotalEmpleados} empleados cargados");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar empleados: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void RefrescarEmpleados()
        {
            System.Diagnostics.Debug.WriteLine("🔄 Refrescando lista de empleados...");
            CargarEmpleados();
        }

        private void Buscar()
        {
            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                MostrarTodos();
                return;
            }

            var busqueda = TextoBusqueda.ToLower();

            var empleadosFiltrados = _todosLosEmpleados.Where(e =>
                e.Nombre.ToLower().Contains(busqueda) ||
                e.IdEmpleado.ToLower().Contains(busqueda) ||
                (e.NombrePuesto?.ToLower().Contains(busqueda) ?? false) ||
                (e.IdPuesto?.ToLower().Contains(busqueda) ?? false)
            ).ToList();

            ActualizarLista(empleadosFiltrados);
        }

        private void MostrarTodos()
        {
            ActualizarLista(_todosLosEmpleados);
        }

        private void FiltrarPorPuesto(string puesto)
        {
            var empleadosFiltrados = _todosLosEmpleados
                .Where(e => e.IdPuesto.Equals(puesto, StringComparison.OrdinalIgnoreCase) ||
                            (e.NombrePuesto?.Equals(puesto, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

            ActualizarLista(empleadosFiltrados);
        }

        private void ActualizarLista(List<EmpleadoModel> empleados)
        {
            EmpleadosFiltrados.Clear();
            foreach (var empleado in empleados)
            {
                EmpleadosFiltrados.Add(empleado);
            }
        }

        private async void VerHistorial(EmpleadoModel empleado)
        {
            if (empleado == null) return;

            var parametros = new Dictionary<string, object>
            {
                { "Empleado", empleado }
            };

            await Shell.Current.GoToAsync("historial", parametros);
        }

        // ⭐ DESUSCRIBIRSE CUANDO SE DESTRUYA EL VIEWMODEL
        ~EmpleadosListViewModel()
        {
            MessagingCenter.Unsubscribe<ConsultaViewModel, string>(this, "ConsultaGuardada");
        }
    }
}
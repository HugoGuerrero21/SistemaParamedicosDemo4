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
	public class EmpleadosListViewModel
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

			// Cargar empleados
			CargarEmpleados();
		}

		private void CargarEmpleados()
		{
			try
			{
				IsBusy = true;

				var empleados = _empleadoRepo.GetAll();

				_todosLosEmpleados = empleados.Select(e =>
				{
					// ⭐ CARGAR EL TOTAL DE CONSULTAS PARA CADA EMPLEADO
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

		// ⭐ MÉTODO PÚBLICO PARA REFRESCAR DESDE OTRAS VISTAS
		public void RefrescarEmpleados()
		{
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
				e.IdPuesto.ToLower().Contains(busqueda)
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
				.Where(e => e.IdPuesto.Equals(puesto, StringComparison.OrdinalIgnoreCase))
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

			// Navegar a la vista de historial pasando el empleado
			var parametros = new Dictionary<string, object>
			{
				{ "Empleado", empleado }
			};

			await Shell.Current.GoToAsync("historial", parametros);
		}

		private string ObtenerIniciales(string nombreCompleto)
		{
			if (string.IsNullOrWhiteSpace(nombreCompleto))
				return "??";

			var palabras = nombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries);

			if (palabras.Length >= 2)
				return $"{palabras[0][0]}{palabras[1][0]}".ToUpper();

			return palabras[0].Length >= 2
				? palabras[0].Substring(0, 2).ToUpper()
				: palabras[0][0].ToString().ToUpper();
		}
	}
}
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

		// ⭐ AGREGAR PROPIEDAD PARA EDAD
		public int EdadEmpleado { get; set; }
		#endregion

		#region Commands
		public ICommand VerDetalleCommand { get; }
		#endregion

		public HistorialConsultasViewModel()
		{
			_consultaRepo = new ConsultaRepository();
			Consultas = new ObservableCollection<ConsultaModelExtendido>();
			VerDetalleCommand = new Command<ConsultaModelExtendido>(VerDetalle);
		}

		// ⭐ CALCULAR EDAD DEL EMPLEADO
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
						TieneMovimiento = !string.IsNullOrEmpty(consulta.IdMovimiento)
					};

					Consultas.Add(consultaExtendida);
				}

				TotalConsultas = Consultas.Count;
				System.Diagnostics.Debug.WriteLine($"✓ {TotalConsultas} consultas cargadas para {Empleado.Nombre}");
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

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	// Clase extendida para incluir propiedades adicionales
	public class ConsultaModelExtendido : ConsultaModel
	{
		public bool TieneMovimiento { get; set; }
	}
}
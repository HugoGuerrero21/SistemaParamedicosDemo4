using SistemaParamedicosDemo4.Data;
using SistemaParamedicosDemo4.Data.Repositories;
using SistemaParamedicosDemo4.MVVM.Models;
using SistemaParamedicosDemo4.MVVM.Views;

namespace SistemaParamedicosDemo4
{
	public partial class App : Application
	{
		public App()
		{
			// 1. PRIMERO: Eliminar la BD antigua (solo para desarrollo)
			//DatabaseManager.EliminarBaseDeDatos();

			// 2. SEGUNDO: Inicializar la conexión (crea las tablas)
			var db = DatabaseManager.Instance;

			// 3. TERCERO: Llenar los datos iniciales
			InicializarDatosBase();
			 
			// 4. CUARTO: Mostrar el Login
			MainPage = new NavigationPage(new LoginView());
		}

		private void InicializarDatosBase()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("========== INICIALIZANDO DATOS BASE ==========");

				// 1. Inicializar Usuarios
				//InicializarUsuarios();

				// 2. Inicializar Tipos de Enfermedad
				//InicializarTiposEnfermedad();

				// 3. Inicializar Productos/Medicamentos
				//InicializarProductos();

				// 4. Inicializar Empleados
				//InicializarEmpleados();

				System.Diagnostics.Debug.WriteLine("========== DATOS BASE INICIALIZADOS ==========\n");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ ERROR AL INICIALIZAR DATOS: {ex.Message}");
			}
		}
		//private void InicializarTiposEnfermedad()
		//{
		//	var repositorio = new TipoEnfermedadRepository();
		//	var tipos = repositorio.GetAllTypes();

		//	if (tipos.Count == 0)
		//	{
		//		System.Diagnostics.Debug.WriteLine("Insertando tipos de enfermedad...");

		//		var tiposIniciales = new List<TipoEnfermedadModel>
		//		{
		//			new TipoEnfermedadModel { NombreEnfermedad = "Musculoesquelético" },
		//			new TipoEnfermedadModel { NombreEnfermedad = "Respiratorio" },
		//			new TipoEnfermedadModel { NombreEnfermedad = "Cardiovascular" },
		//			new TipoEnfermedadModel { NombreEnfermedad = "Digestivo" },
		//			new TipoEnfermedadModel { NombreEnfermedad = "Neurológico" },
		//			new TipoEnfermedadModel { NombreEnfermedad = "Otros" }
		//		};

		//		repositorio.InsertarTiposIniciales(tiposIniciales);
		//		System.Diagnostics.Debug.WriteLine("✓ Tipos de enfermedad insertados");
		//	}
		//	else
		//	{
		//		System.Diagnostics.Debug.WriteLine($"✓ Ya existen {tipos.Count} tipos de enfermedad");
		//	}
		//}


	}
}
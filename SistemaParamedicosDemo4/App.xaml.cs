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
			DatabaseManager.EliminarBaseDeDatos();

			// 2. SEGUNDO: Inicializar la conexión (crea las tablas)
			var db = DatabaseManager.Instance;

			// 3. TERCERO: Llenar los datos iniciales
			InicializarDatosBase();

			// 4. CUARTO: Mostrar el Login
			// ⭐ CAMBIO: Ahora mostramos LoginView directamente (sin NavigationPage)
			MainPage = new NavigationPage(new LoginView());
		}

		private void InicializarDatosBase()
		{
			try
			{
				System.Diagnostics.Debug.WriteLine("========== INICIALIZANDO DATOS BASE ==========");

				// 1. Inicializar Usuarios
				InicializarUsuarios();

				// 2. Inicializar Tipos de Enfermedad
				InicializarTiposEnfermedad();

				// 3. Inicializar Productos/Medicamentos
				InicializarProductos();

				// 4. Inicializar Empleados
				InicializarEmpleados();

				System.Diagnostics.Debug.WriteLine("========== DATOS BASE INICIALIZADOS ==========\n");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"❌ ERROR AL INICIALIZAR DATOS: {ex.Message}");
			}
		}

		private void InicializarUsuarios()
		{
			var repositorio = new UsuarioAccesoRepositories();
			var usuarios = repositorio.GetAllUsuarios();

			if (usuarios.Count == 0)
			{
				System.Diagnostics.Debug.WriteLine("Insertando usuarios de prueba...");

				repositorio.InsertarUsuario(new UsuariosAccesoModel
				{
					IdUsuario = "USR001",
					Nombre = "Administrador",
					Usuario = "admin",
					Password = "admin123"
				});

				repositorio.InsertarUsuario(new UsuariosAccesoModel
				{
					IdUsuario = "USR002",
					Nombre = "Juan Pérez",
					Usuario = "paramedico",
					Password = "para123"
				});

                repositorio.InsertarUsuario(new UsuariosAccesoModel
                {
                    IdUsuario = "USR003",
                    Nombre = "Hugo Guerrero",
                    Usuario = "hugo",
                    Password = "para123"
                });

                System.Diagnostics.Debug.WriteLine("✓ Usuarios insertados");
			}
			else
			{
				System.Diagnostics.Debug.WriteLine($"✓ Ya existen {usuarios.Count} usuarios");
			}
		}

		private void InicializarTiposEnfermedad()
		{
			var repositorio = new TipoEnfermedadRepository();
			var tipos = repositorio.GetAllTypes();

			if (tipos.Count == 0)
			{
				System.Diagnostics.Debug.WriteLine("Insertando tipos de enfermedad...");

				var tiposIniciales = new List<TipoEnfermedadModel>
				{
					new TipoEnfermedadModel { NombreEnfermedad = "Musculoesquelético" },
					new TipoEnfermedadModel { NombreEnfermedad = "Respiratorio" },
					new TipoEnfermedadModel { NombreEnfermedad = "Cardiovascular" },
					new TipoEnfermedadModel { NombreEnfermedad = "Digestivo" },
					new TipoEnfermedadModel { NombreEnfermedad = "Neurológico" },
					new TipoEnfermedadModel { NombreEnfermedad = "Otros" }
				};

				repositorio.InsertarTiposIniciales(tiposIniciales);
				System.Diagnostics.Debug.WriteLine("✓ Tipos de enfermedad insertados");
			}
			else
			{
				System.Diagnostics.Debug.WriteLine($"✓ Ya existen {tipos.Count} tipos de enfermedad");
			}
		}

		private void InicializarProductos()
		{
			var repositorio = new ProductoRepository();
			var productos = repositorio.GetAllProductos();

			if (productos.Count == 0)
			{
				System.Diagnostics.Debug.WriteLine("Insertando productos/medicamentos...");

				var productosIniciales = new List<ProductoModel>
				{
					new ProductoModel
					{
						ProductoId = "MED001",
						Nombre = "Paracetamol",
						Marca = "Genérico",
						Model = "500mg",
						Descripcion = "Analgésico y antipirético",
						CantidadDisponible = 150
					},
					new ProductoModel
					{
						ProductoId = "MED002",
						Nombre = "Ibuprofeno",
						Marca = "Genérico",
						Model = "400mg",
						Descripcion = "Antiinflamatorio",
						CantidadDisponible = 80
					},
					new ProductoModel
					{
						ProductoId = "MED003",
						Nombre = "Aspirina",
						Marca = "Bayer",
						Model = "100mg",
						Descripcion = "Antiagregante plaquetario",
						CantidadDisponible = 200
					},
					new ProductoModel
					{
						ProductoId = "MED004",
						Nombre = "Amoxicilina",
						Marca = "Genérico",
						Model = "500mg",
						Descripcion = "Antibiótico",
						CantidadDisponible = 50
					},
					new ProductoModel
					{
						ProductoId = "MED005",
						Nombre = "Omeprazol",
						Marca = "Genérico",
						Model = "20mg",
						Descripcion = "Protector gástrico",
						CantidadDisponible = 120
					}
				};

				repositorio.InsertarProductosIniciales(productosIniciales);
				System.Diagnostics.Debug.WriteLine("✓ Productos insertados");
			}
			else
			{
				System.Diagnostics.Debug.WriteLine($"✓ Ya existen {productos.Count} productos");
			}
		}

		private void InicializarEmpleados()
		{
			var repositorio = new EmpleadoRepository();
			var empleados = repositorio.GetAll();

			if (empleados.Count == 0)
			{
				System.Diagnostics.Debug.WriteLine("Insertando empleados de prueba...");

				repositorio.InsertarEmpleado(new EmpleadoModel
				{
					IdEmpleado = "TRSEMP001",
					Nombre = "Juan Carlos Perez Hernandez",
					TipoSangre = "A+",
					Sexo = "M",
					AlergiasSangre = "Ninguna",
					Telefono = "6621234567",
					FechaNacimiento = new DateTime(1990, 5, 15),
					IdPuesto = "CHOFER"
				});

				repositorio.InsertarEmpleado(new EmpleadoModel
				{
					IdEmpleado = "TRSEMP002",
					Nombre = "Maria Guadalupe Lopez Garcia",
					TipoSangre = "O+",
					Sexo = "F",
					AlergiasSangre = "Penicilina",
					Telefono = "6629876543",
					FechaNacimiento = new DateTime(1985, 8, 22),
					IdPuesto = "OPERADOR"
				});

				repositorio.InsertarEmpleado(new EmpleadoModel
				{
					IdEmpleado = "TRSEMP003",
					Nombre = "Roberto Martinez Sanchez",
					TipoSangre = "B+",
					Sexo = "M",
					AlergiasSangre = "Ninguna",
					Telefono = "6625551234",
					FechaNacimiento = new DateTime(1992, 3, 10),
					IdPuesto = "MECANICO"
				});

				System.Diagnostics.Debug.WriteLine("✓ Empleados insertados");
			}
			else
			{
				System.Diagnostics.Debug.WriteLine($"✓ Ya existen {empleados.Count} empleados");
			}
		}
	}
}
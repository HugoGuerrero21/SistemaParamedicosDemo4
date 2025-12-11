using SQLite;
using SistemaParamedicosDemo4.MVVM.Models;

namespace SistemaParamedicosDemo4.Data
{
    public class DatabaseManager
    {
        private static DatabaseManager _instance;
        private static readonly object _lock = new object();

        public SQLiteConnection Connection { get; private set; }
        public string StatusMessage { get; set; }

        private DatabaseManager()
        {
            try
            {
                Connection = new SQLiteConnection(Constants.DataBasePath, Constants.Flags);

                System.Diagnostics.Debug.WriteLine("📦 Inicializando base de datos...");

                // ⭐ TABLAS DE CATÁLOGOS (primero porque son referencias)
                Connection.CreateTable<AlmacenModel>();
                Connection.CreateTable<PuestoModel>();
                Connection.CreateTable<TipoEnfermedadModel>();
                Connection.CreateTable<ProductoModel>();
                System.Diagnostics.Debug.WriteLine("✓ Tablas de catálogos creadas");

                // ⭐ TABLAS DE USUARIOS Y EMPLEADOS
                Connection.CreateTable<UsuariosAccesoModel>();
                Connection.CreateTable<EmpleadoModel>();
                System.Diagnostics.Debug.WriteLine("✓ Tablas de usuarios y empleados creadas");

                // ⭐ TABLAS DE MOVIMIENTOS
                Connection.CreateTable<MovimientoDetalleModel>();
                System.Diagnostics.Debug.WriteLine("✓ Tabla de movimientos creada");

                // ⭐ TABLAS DE TRASPASOS
                Connection.CreateTable<TraspasoModel>();
                Connection.CreateTable<TraspasoDetalleModel>();
                System.Diagnostics.Debug.WriteLine("✓ Tablas de traspasos creadas");

                // ⭐ TABLA DE CONSULTAS (última porque depende de todo lo anterior)
                Connection.CreateTable<ConsultaModel>();
                System.Diagnostics.Debug.WriteLine("✓ Tabla de consultas creada");

                StatusMessage = "Base de datos inicializada correctamente";
                System.Diagnostics.Debug.WriteLine($"✅ {StatusMessage}");

                // Verificar tablas creadas
                var tablas = Connection.TableMappings.Count();
                System.Diagnostics.Debug.WriteLine($"📊 Total de tablas en BD: {tablas}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al inicializar base de datos: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ {StatusMessage}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        public static DatabaseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DatabaseManager();
                        }
                    }
                }
                return _instance;
            }
        }


        //Reinicia completamente la base de datos
        //⚠️ USAR SOLO EN DESARROLLO - ELIMINA TODOS LOS DATOS

        public void ReiniciarBaseDeDatos()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Reiniciando base de datos...");

                // Eliminar todas las tablas en orden inverso (para evitar problemas de FK)
                Connection.DropTable<ConsultaModel>();
                Connection.DropTable<TraspasoDetalleModel>();
                Connection.DropTable<TraspasoModel>();
                Connection.DropTable<MovimientoDetalleModel>();
                Connection.DropTable<EmpleadoModel>();
                Connection.DropTable<UsuariosAccesoModel>();
                Connection.DropTable<ProductoModel>();
                Connection.DropTable<TipoEnfermedadModel>();
                Connection.DropTable<PuestoModel>();
                Connection.DropTable<AlmacenModel>();

                System.Diagnostics.Debug.WriteLine("✓ Todas las tablas eliminadas");

                // Recrear todas las tablas
                Connection.CreateTable<AlmacenModel>();
                Connection.CreateTable<PuestoModel>();
                Connection.CreateTable<TipoEnfermedadModel>();
                Connection.CreateTable<ProductoModel>();
                Connection.CreateTable<UsuariosAccesoModel>();
                Connection.CreateTable<EmpleadoModel>();
                Connection.CreateTable<MovimientoDetalleModel>();
                Connection.CreateTable<TraspasoModel>();
                Connection.CreateTable<TraspasoDetalleModel>();
                Connection.CreateTable<ConsultaModel>();

                System.Diagnostics.Debug.WriteLine("✅ Base de datos reiniciada correctamente");
                StatusMessage = "Base de datos reiniciada";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al reiniciar BD: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ {StatusMessage}");
            }
        }
    }
}
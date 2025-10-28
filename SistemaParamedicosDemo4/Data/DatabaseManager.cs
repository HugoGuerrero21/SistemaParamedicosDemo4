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

                // Crear todas las tablas necesarias
                Connection.CreateTable<UsuariosAccesoModel>();  // ⭐ AGREGAR ESTA LÍNEA
                Connection.CreateTable<EmpleadoModel>();
                Connection.CreateTable<ProductoModel>();
                Connection.CreateTable<TipoEnfermedadModel>();
                Connection.CreateTable<ConsultaModel>();

                // IMPORTANTE: Solo eliminar si es necesario (desarrollo)
                // Comentar esta línea en producción
                //Connection.DropTable<MovimientoDetalleModel>();
                Connection.CreateTable<MovimientoDetalleModel>();

                StatusMessage = "Base de datos inicializada correctamente";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al inicializar base de datos: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
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

        //public void ResetearTablaMovimientos()
        //{
        //    try
        //    {
        //        Connection.DropTable<MovimientoDetalleModel>();
        //        Connection.CreateTable<MovimientoDetalleModel>();
        //        StatusMessage = "Tabla de movimientos reseteada";
        //        System.Diagnostics.Debug.WriteLine(StatusMessage);
        //    }
        //    catch (Exception ex)
        //    {
        //        StatusMessage = $"Error al resetear tabla: {ex.Message}";
        //        System.Diagnostics.Debug.WriteLine(StatusMessage);
        //    }
        //}

        public static void EliminarBaseDeDatos()
        {
            try
            {
                if (File.Exists(Constants.DataBasePath))
                {
                    File.Delete(Constants.DataBasePath);
                    System.Diagnostics.Debug.WriteLine("✅ Base de datos eliminada");
                }

                _instance = null; // Resetear la instancia
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al eliminar BD: {ex.Message}");
            }
        }
    }
}
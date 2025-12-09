using SistemaParamedicosDemo4.MVVM.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.Data.Repositories
{
    public class TipoEnfermedadRepository
    {
        private SQLiteConnection Connection;
        public string StatusMessage { get; set; }

        public TipoEnfermedadRepository()
        {
            try
            {
                // Usar la conexión compartida del DatabaseManager
                Connection = DatabaseManager.Instance.Connection;
                StatusMessage = "Repositorio de movimientos inicializado";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al inicializar tabla: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }
        }

        public List<TipoEnfermedadModel> GetAllTypes()
        {
            try
            {
                return Connection.Table<TipoEnfermedadModel>().ToList();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener tipos de enfermedad: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return new List<TipoEnfermedadModel>();
            }
        }
        public TipoEnfermedadModel GetById(int id)
        {
            try
            {
                return Connection.Find<TipoEnfermedadModel>(id);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener tipo de enfermedad: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return null;
            }
        }

        public bool InsertarTipo(TipoEnfermedadModel Tipo)
        {
            try
            {
                if (Tipo == null)
                {
                    StatusMessage = "El tipo de enfermedad no puede ser nulo";
                    return false;
                }

                // ⭐ AGREGAR LOG PARA DEBUG
                System.Diagnostics.Debug.WriteLine($"Intentando insertar: {Tipo.NombreEnfermedad}");

                int result = Connection.Insert(Tipo);
                StatusMessage = "Tipo de enfermedad ingresada con éxito";

                // ⭐ OBTENER EL ID GENERADO
                if (result > 0)
                {
                    var insertado = Connection.Table<TipoEnfermedadModel>()
                        .OrderByDescending(t => t.IdTipoEnfermedad)
                        .FirstOrDefault();

                    if (insertado != null)
                    {
                        Tipo.IdTipoEnfermedad = insertado.IdTipoEnfermedad;
                        System.Diagnostics.Debug.WriteLine($"✅ Tipo insertado con ID: {Tipo.IdTipoEnfermedad}");
                    }
                }

                return result > 0;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al insertar tipo: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ {StatusMessage}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public bool InsertarTiposIniciales(List<TipoEnfermedadModel> tipos)
        {
            try
            {
                var TiposExistentes = GetAllTypes();
                if (TiposExistentes.Count > 0)
                {
                    StatusMessage = ("Ya existen valores insertados");
                    return false;
                }
                Connection.InsertAll(tipos);
                StatusMessage = $"{tipos.Count} tipos de enfermedades insertados";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al insertar tipo de enfermedades iniciales: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }

        /// Sincroniza los tipos de enfermedad desde la API
        public void SincronizarTiposEnfermedad(List<TipoEnfermedadModel> tiposDesdeApi)
        {
            try
            {
                Connection.BeginTransaction();

                int insertados = 0;
                int actualizados = 0;

                foreach (var tipoApi in tiposDesdeApi)
                {
                    var tipoExistente = Connection.Find<TipoEnfermedadModel>(tipoApi.IdTipoEnfermedad);

                    if (tipoExistente == null)
                    {
                        // No existe, insertar
                        Connection.Insert(tipoApi);
                        insertados++;
                        System.Diagnostics.Debug.WriteLine($"✓ Tipo insertado: {tipoApi.IdTipoEnfermedad} - {tipoApi.NombreEnfermedad}");
                    }
                    else if (tipoExistente.NombreEnfermedad != tipoApi.NombreEnfermedad)
                    {
                        // Existe pero el nombre cambió, actualizar
                        tipoExistente.NombreEnfermedad = tipoApi.NombreEnfermedad;
                        tipoExistente.IdUsuarioAcc = tipoApi.IdUsuarioAcc;
                        Connection.Update(tipoExistente);
                        actualizados++;
                        System.Diagnostics.Debug.WriteLine($"✓ Tipo actualizado: {tipoApi.IdTipoEnfermedad} - {tipoApi.NombreEnfermedad}");
                    }
                }

                Connection.Commit();
                StatusMessage = $"Tipos sincronizados: {insertados} nuevos, {actualizados} actualizados";
                System.Diagnostics.Debug.WriteLine($"✅ {StatusMessage}");
            }
            catch (Exception ex)
            {
                try
                {
                    Connection.Rollback();
                }
                catch { }

                StatusMessage = $"Error al sincronizar tipos: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ {StatusMessage}");
            }
        }
    }
}
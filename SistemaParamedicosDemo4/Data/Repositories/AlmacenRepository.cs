using SistemaParamedicosDemo4.MVVM.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaParamedicosDemo4.Data.Repositories
{
    public class AlmacenRepository
    {
        private SQLiteConnection Connection;
        public string StatusMessage { get; set; }

        public AlmacenRepository()
        {
            try
            {
                Connection = DatabaseManager.Instance.Connection;
                StatusMessage = "Repositorio de almacenes inicializado";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al inicializar repositorio: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }
        }

        /// <summary>
        /// Obtiene todos los almacenes
        /// </summary>
        public List<AlmacenModel> ObtenerTodos()
        {
            try
            {
                return Connection.Table<AlmacenModel>().ToList();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener almacenes: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return new List<AlmacenModel>();
            }
        }

        /// <summary>
        /// Obtiene un almacén por ID
        /// </summary>
        public AlmacenModel ObtenerPorId(string idAlmacen)
        {
            try
            {
                return Connection.Find<AlmacenModel>(idAlmacen);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener almacén: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return null;
            }
        }

        /// <summary>
        /// Obtiene el almacén de paramédicos (ALM6)
        /// </summary>
        public AlmacenModel ObtenerAlmacenParamedicos()
        {
            try
            {
                return Connection.Find<AlmacenModel>("ALM6");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener almacén de paramédicos: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return null;
            }
        }

        /// <summary>
        /// Obtiene el almacén de Hermosillo (ALM1)
        /// </summary>
        public AlmacenModel ObtenerAlmacenHermosillo()
        {
            try
            {
                return Connection.Find<AlmacenModel>("ALM1");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener almacén de Hermosillo: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return null;
            }
        }

        /// <summary>
        /// Guarda o actualiza un almacén
        /// </summary>
        public bool Guardar(AlmacenModel almacen)
        {
            try
            {
                if (almacen == null)
                {
                    StatusMessage = "El almacén no puede ser nulo";
                    return false;
                }

                var existente = Connection.Find<AlmacenModel>(almacen.IdAlmacen);

                if (existente != null)
                {
                    Connection.Update(almacen);
                    StatusMessage = "Almacén actualizado";
                }
                else
                {
                    Connection.Insert(almacen);
                    StatusMessage = "Almacén guardado";
                }

                System.Diagnostics.Debug.WriteLine($"✓ {StatusMessage}: {almacen.IdAlmacen}");
                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al guardar almacén: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }

        /// <summary>
        /// Guarda múltiples almacenes
        /// </summary>
        public bool GuardarVarios(List<AlmacenModel> almacenes)
        {
            try
            {
                if (almacenes == null || almacenes.Count == 0)
                {
                    StatusMessage = "No hay almacenes para guardar";
                    return false;
                }

                foreach (var almacen in almacenes)
                {
                    Guardar(almacen);
                }

                StatusMessage = $"{almacenes.Count} almacenes guardados";
                System.Diagnostics.Debug.WriteLine($"✓ {StatusMessage}");
                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al guardar almacenes: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }

        /// <summary>
        /// Inicializa los almacenes básicos del sistema si no existen
        /// </summary>
        public void InicializarAlmacenesBasicos()
        {
            try
            {
                var almacenesExistentes = ObtenerTodos();

                if (almacenesExistentes.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("📦 Inicializando almacenes básicos...");

                    var almacenesBasicos = new List<AlmacenModel>
                    {
                        new AlmacenModel
                        {
                            IdAlmacen = "ALM1",
                            Nombre = "Hermosillo",
                            ColumnaStock = "a_hermosillo"
                        },
                        new AlmacenModel
                        {
                            IdAlmacen = "ALM6",
                            Nombre = "Paramédicos",
                            ColumnaStock = "a_paramedicos"
                        }
                    };

                    GuardarVarios(almacenesBasicos);
                    System.Diagnostics.Debug.WriteLine("✅ Almacenes básicos inicializados");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ℹ️ Ya existen {almacenesExistentes.Count} almacenes en BD");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al inicializar almacenes: {ex.Message}");
            }
        }
    }
}
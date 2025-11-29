using SistemaParamedicosDemo4.MVVM.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaParamedicosDemo4.Data.Repositories
{
    public class TraspasoRepository
    {
        private SQLiteConnection Connection;
        public string StatusMessage { get; set; }

        private ProductoRepository _productoRepo;
        private AlmacenRepository _almacenRepo;

        public TraspasoRepository()
        {
            try
            {
                Connection = DatabaseManager.Instance.Connection;
                _productoRepo = new ProductoRepository();
                _almacenRepo = new AlmacenRepository();

                StatusMessage = "Repositorio de traspasos inicializado";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al inicializar repositorio: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }
        }

        /// <summary>
        /// Obtiene todos los traspasos
        /// </summary>
        public List<TraspasoModel> ObtenerTodosTraspasos()
        {
            try
            {
                return Connection.Table<TraspasoModel>().ToList();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener traspasos: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return new List<TraspasoModel>();
            }
        }

        /// <summary>
        /// Obtiene traspasos pendientes (Status = 0)
        /// </summary>
        public List<TraspasoModel> ObtenerTraspasosPendientes()
        {
            try
            {
                return Connection.Table<TraspasoModel>()
                    .Where(t => t.Status == 0)
                    .OrderByDescending(t => t.FechaEnvio)
                    .ToList();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener traspasos pendientes: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return new List<TraspasoModel>();
            }
        }

        /// <summary>
        /// Obtiene un traspaso por ID con sus detalles
        /// </summary>
        public TraspasoModel ObtenerTraspasoPorId(string idTraspaso, bool cargarDetalles = true)
        {
            try
            {
                var traspaso = Connection.Find<TraspasoModel>(idTraspaso);

                if (traspaso != null && cargarDetalles)
                {
                    // Cargar detalles
                    traspaso.Detalles = Connection.Table<TraspasoDetalleModel>()
                        .Where(d => d.IdTraspaso == idTraspaso)
                        .ToList();

                    // Cargar información de productos para cada detalle
                    foreach (var detalle in traspaso.Detalles)
                    {
                        detalle.Producto = _productoRepo.GetProductosById(detalle.IdProducto);
                    }
                }

                return traspaso;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener traspaso: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return null;
            }
        }

        /// <summary>
        /// Guarda o actualiza un traspaso
        /// </summary>
        public bool GuardarTraspaso(TraspasoModel traspaso)
        {
            try
            {
                if (traspaso == null)
                {
                    StatusMessage = "El traspaso no puede ser nulo";
                    return false;
                }

                var existente = Connection.Find<TraspasoModel>(traspaso.IdTraspaso);

                if (existente != null)
                {
                    Connection.Update(traspaso);
                    StatusMessage = "Traspaso actualizado";
                }
                else
                {
                    Connection.Insert(traspaso);
                    StatusMessage = "Traspaso guardado";
                }

                System.Diagnostics.Debug.WriteLine($"✓ {StatusMessage}: {traspaso.IdTraspaso}");
                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al guardar traspaso: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }

        /// <summary>
        /// Guarda o actualiza un detalle de traspaso
        /// </summary>
        public bool GuardarDetalle(TraspasoDetalleModel detalle)
        {
            try
            {
                if (detalle == null)
                {
                    StatusMessage = "El detalle no puede ser nulo";
                    return false;
                }

                var existente = Connection.Find<TraspasoDetalleModel>(detalle.IdTraspasoDetalle);

                if (existente != null)
                {
                    Connection.Update(detalle);
                    StatusMessage = "Detalle actualizado";
                }
                else
                {
                    Connection.Insert(detalle);
                    StatusMessage = "Detalle guardado";
                }

                System.Diagnostics.Debug.WriteLine($"✓ {StatusMessage}: {detalle.IdTraspasoDetalle}");
                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al guardar detalle: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }

        /// <summary>
        /// Guarda múltiples detalles de un traspaso
        /// </summary>
        public bool GuardarDetalles(List<TraspasoDetalleModel> detalles)
        {
            try
            {
                if (detalles == null || detalles.Count == 0)
                {
                    StatusMessage = "No hay detalles para guardar";
                    return false;
                }

                foreach (var detalle in detalles)
                {
                    GuardarDetalle(detalle);
                }

                StatusMessage = $"{detalles.Count} detalles guardados";
                System.Diagnostics.Debug.WriteLine($"✓ {StatusMessage}");
                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al guardar detalles: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }

        /// <summary>
        /// Guarda un traspaso completo con sus detalles
        /// </summary>
        public bool GuardarTraspasoCompleto(TraspasoModel traspaso)
        {
            try
            {
                Connection.BeginTransaction();

                // 1. Guardar el traspaso
                if (!GuardarTraspaso(traspaso))
                {
                    Connection.Rollback();
                    return false;
                }

                // 2. Guardar los detalles
                if (traspaso.Detalles != null && traspaso.Detalles.Count > 0)
                {
                    if (!GuardarDetalles(traspaso.Detalles))
                    {
                        Connection.Rollback();
                        return false;
                    }
                }

                Connection.Commit();
                StatusMessage = "Traspaso completo guardado exitosamente";
                System.Diagnostics.Debug.WriteLine($"✅ {StatusMessage}");
                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    Connection.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en rollback: {rollbackEx.Message}");
                }

                StatusMessage = $"Error al guardar traspaso completo: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ {StatusMessage}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene los detalles de un traspaso
        /// </summary>
        public List<TraspasoDetalleModel> ObtenerDetallesTraspaso(string idTraspaso)
        {
            try
            {
                var detalles = Connection.Table<TraspasoDetalleModel>()
                    .Where(d => d.IdTraspaso == idTraspaso)
                    .ToList();

                // Cargar información de productos
                foreach (var detalle in detalles)
                {
                    detalle.Producto = _productoRepo.GetProductosById(detalle.IdProducto);
                }

                return detalles;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener detalles: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return new List<TraspasoDetalleModel>();
            }
        }

        /// <summary>
        /// Actualiza el estado de un traspaso
        /// </summary>
        public bool ActualizarEstadoTraspaso(string idTraspaso, byte nuevoStatus)
        {
            try
            {
                var traspaso = Connection.Find<TraspasoModel>(idTraspaso);

                if (traspaso == null)
                {
                    StatusMessage = "Traspaso no encontrado";
                    return false;
                }

                traspaso.Status = nuevoStatus;

                if (nuevoStatus == 1) // Completado
                {
                    traspaso.FechaCompletado = DateTime.Now;
                }

                Connection.Update(traspaso);
                StatusMessage = "Estado del traspaso actualizado";
                System.Diagnostics.Debug.WriteLine($"✓ {StatusMessage}: {idTraspaso} -> Status {nuevoStatus}");
                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al actualizar estado: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }

        /// <summary>
        /// Limpia traspasos antiguos (opcional, para mantener la BD limpia)
        /// </summary>
        public bool LimpiarTraspasosAntiguos(int diasAntiguedad = 30)
        {
            try
            {
                var fechaLimite = DateTime.Now.AddDays(-diasAntiguedad);

                var traspasosAntiguos = Connection.Table<TraspasoModel>()
                    .Where(t => t.Status == 1 && t.FechaCompletado < fechaLimite)
                    .ToList();

                foreach (var traspaso in traspasosAntiguos)
                {
                    // Eliminar detalles
                    var detalles = Connection.Table<TraspasoDetalleModel>()
                        .Where(d => d.IdTraspaso == traspaso.IdTraspaso)
                        .ToList();

                    foreach (var detalle in detalles)
                    {
                        Connection.Delete(detalle);
                    }

                    // Eliminar traspaso
                    Connection.Delete(traspaso);
                }

                StatusMessage = $"{traspasosAntiguos.Count} traspasos antiguos eliminados";
                System.Diagnostics.Debug.WriteLine($"✓ {StatusMessage}");
                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al limpiar traspasos: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }
    }
}
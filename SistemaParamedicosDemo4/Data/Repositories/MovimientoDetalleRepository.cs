using SistemaParamedicosDemo4.MVVM.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.Data.Repositories
{
    public class MovimientoDetalleRepository
    {
        private SQLiteConnection Connection;
        public string StatusMessage { get; set; }

        public MovimientoDetalleRepository()
        {
            try
            {
                // Usar la conexión compartida del DatabaseManager
                Connection = DatabaseManager.Instance.Connection;
                StatusMessage = "Repositorio de movimientos inicializado";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }

            catch( Exception ex)
            {
                StatusMessage = $"Se ha producido un error{ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }
        }
        public List<MovimientoDetalleModel> GetAllDetalles()
        {
            try
            {
                return Connection.Table<MovimientoDetalleModel>().ToList();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener detalles: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return new List<MovimientoDetalleModel>();
            }
        }
        public List<MovimientoDetalleModel> GetDetallesByMovimiento(string idMovimiento)
        {
            try
            {
                return Connection.Table<MovimientoDetalleModel>()
                    .Where(d => d.IdMovimiento == idMovimiento)
                    .ToList();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener detalles: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return new List<MovimientoDetalleModel>();
            }
        }
        public MovimientoDetalleModel GetDetalleById(string id)
        {
            try
            {
                return Connection.Find<MovimientoDetalleModel>(id);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener detalle: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return null;
            }
        }
        public bool InsertarDetalle(MovimientoDetalleModel detalle)
        {
            try
            {
                if (detalle == null)
                {
                    StatusMessage = "El detalle no puede ser nulo";
                    return false;
                }

                var existente = Connection.Find<MovimientoDetalleModel>(detalle.IdMovimientoDetalle);
                if (existente != null)
                {
                    StatusMessage = "El detalle ya existe. Se actualizará.";
                    Connection.Update(detalle);
                }
                else
                {
                    Connection.Insert(detalle);
                    StatusMessage = "Detalle insertado correctamente.";
                }

                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al insertar detalle: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }

        public bool InsertarDetalles(List<MovimientoDetalleModel> detalles)
        {
            try
            {
                if (detalles == null || detalles.Count == 0)
                {
                    StatusMessage = "No hay detalles para insertar";
                    return false;
                }

                Connection.InsertAll(detalles);
                StatusMessage = $"{detalles.Count} detalles insertados";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al insertar detalles: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }
        public bool ActualizarDetalle(MovimientoDetalleModel detalle)
        {
            try
            {
                if (detalle == null)
                {
                    StatusMessage = "El detalle no puede ser nulo";
                    return false;
                }

                int result = Connection.Update(detalle);
                StatusMessage = "Detalle actualizado correctamente";
                return result > 0;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al actualizar detalle: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }
    }
}

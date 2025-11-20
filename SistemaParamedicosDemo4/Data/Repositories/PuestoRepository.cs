using SistemaParamedicosDemo4.MVVM.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaParamedicosDemo4.Data.Repositories
{
    public class PuestoRepository
    {
        private SQLiteConnection Connection;
        public string StatusMessage { get; set; }

        public PuestoRepository()
        {
            try
            {
                // Usar la conexión compartida del DatabaseManager
                Connection = DatabaseManager.Instance.Connection;
                StatusMessage = "Repositorio de puestos inicializado";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al inicializar repositorio de puestos: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }
        }

        public List<PuestoModel> GetAll()
        {
            try
            {
                return Connection.Table<PuestoModel>()
                    .OrderBy(p => p.Nombre)
                    .ToList();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener puestos: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return new List<PuestoModel>();
            }
        }

        public PuestoModel GetById(string idPuesto)
        {
            try
            {
                return Connection.Find<PuestoModel>(idPuesto);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener puesto: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return null;
            }
        }

        public bool InsertarPuesto(PuestoModel puesto)
        {
            try
            {
                if (puesto == null)
                {
                    StatusMessage = "El puesto no puede ser nulo";
                    return false;
                }

                int result = Connection.Insert(puesto);
                StatusMessage = "Puesto insertado con éxito";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return result > 0;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al insertar puesto: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }
        public bool ActualizarPuesto(PuestoModel puesto)
        {
            try
            {
                if (puesto == null)
                {
                    StatusMessage = "El puesto no puede ser nulo";
                    return false;
                }

                int result = Connection.Update(puesto);
                StatusMessage = "Puesto actualizado correctamente";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return result > 0;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al actualizar puesto: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }

        public bool SincronizarPuestos(List<PuestoModel> puestos)
        {
            try
            {
                if (puestos == null || puestos.Count == 0)
                {
                    StatusMessage = "No hay puestos para sincronizar";
                    return false;
                }

                Connection.BeginTransaction();

                foreach (var puesto in puestos)
                {
                    // Verificar si existe
                    var existente = Connection.Find<PuestoModel>(puesto.IdPuesto);

                    if (existente != null)
                    {
                        // Actualizar
                        Connection.Update(puesto);
                    }
                    else
                    {
                        // Insertar
                        Connection.Insert(puesto);
                    }
                }

                Connection.Commit();
                StatusMessage = $"{puestos.Count} puestos sincronizados";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return true;
            }
            catch (Exception ex)
            {
                try { Connection.Rollback(); } catch { }
                StatusMessage = $"Error al sincronizar puestos: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }
    }
}
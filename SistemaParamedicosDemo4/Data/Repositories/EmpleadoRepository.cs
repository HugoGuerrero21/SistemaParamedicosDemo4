using SistemaParamedicosDemo4.MVVM.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.Data.Repositories
{
    public class EmpleadoRepository
    {
        private SQLiteConnection Connection;
        public string StatusMessage { get; set; }

        public EmpleadoRepository() 
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
                StatusMessage = $"Error al crear la tabla de empleados{ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }
        }

        public List<EmpleadoModel> GetAll()
        {
            try
            {
                var empleados = Connection.Table<EmpleadoModel>().ToList();

                if (empleados.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ No hay empleados en SQLite");
                    return empleados;
                }

                var puestoRepo = new PuestoRepository();

                // Cargar el nombre del puesto para cada empleado
                foreach (var empleado in empleados)
                {
                    if (!string.IsNullOrEmpty(empleado.IdPuesto))
                    {
                        var puesto = puestoRepo.GetById(empleado.IdPuesto);
                        empleado.NombrePuesto = puesto?.Nombre ?? empleado.IdPuesto;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✓ {empleados.Count} empleados cargados desde SQLite con puestos");
                return empleados;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Imposible extraer todos los empleados: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return new List<EmpleadoModel>();
            }
        }

        public EmpleadoModel GetById(string id)
        {
            try
            {
                var empleado = Connection.Find<EmpleadoModel>(id);

                if (empleado != null && !string.IsNullOrEmpty(empleado.IdPuesto))
                {
                    var puestoRepo = new PuestoRepository();
                    var puesto = puestoRepo.GetById(empleado.IdPuesto);
                    empleado.NombrePuesto = puesto?.Nombre ?? empleado.IdPuesto;
                }

                return empleado;
            }
            catch (Exception ex)
            {
                StatusMessage = "Error al encontrar el id";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return null;
            }
        }
        public List<EmpleadoModel> BuscarPorNombre(string nombre)
        {
            try
            {
                return Connection.Table<EmpleadoModel>()
                    .Where(e => e.Nombre.Contains(nombre))
                    .ToList();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al buscar empleado: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return new List<EmpleadoModel>();
            }
        }

        public bool InsertarEmpleado(EmpleadoModel Empleado) 
        {
            try
            {
                if (Empleado == null)
                {
                    StatusMessage = "El empleado no puede ser nulo";
                    return false;
                }
                else
                {
                    int result = Connection.Insert(Empleado);
                    StatusMessage = "Empleado insertado con exito";
                    return result > 0;
                }
            }
            catch (Exception ex) 
            {
                StatusMessage = $"Error al insertar empleado: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }

        public bool ActualizarEmpleado(EmpleadoModel empleado)
        {
            try
            {
                if (empleado == null)
                {
                    StatusMessage = "El empleado no puede ser nulo";
                    return false;
                }

                int result = Connection.Update(empleado);
                StatusMessage = "Empleado actualizado correctamente";
                return result > 0;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al actualizar empleado: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }

        public bool SincronizarEmpleados(List<EmpleadoModel> empleados)
        {
            try
            {
                if (empleados == null || empleados.Count == 0)
                {
                    StatusMessage = "No hay empleados para sincronizar";
                    return false;
                }

                Connection.BeginTransaction();

                foreach (var empleado in empleados)
                {
                    // Verificar si existe
                    var existente = Connection.Find<EmpleadoModel>(empleado.IdEmpleado);

                    if (existente != null)
                    {
                        // Actualizar
                        Connection.Update(empleado);
                    }
                    else
                    {
                        // Insertar
                        Connection.Insert(empleado);
                    }
                }

                Connection.Commit();
                StatusMessage = $"{empleados.Count} empleados sincronizados";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return true;
            }
            catch (Exception ex)
            {
                try { Connection.Rollback(); } catch { }
                StatusMessage = $"Error al sincronizar: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }
    }
}

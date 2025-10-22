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
                return Connection.Table<EmpleadoModel>().ToList();
            }
            catch (Exception ex) 
            {
                StatusMessage = "Imposible extraer todos los empleados";
                return new List<EmpleadoModel>();
            }
        }

        public EmpleadoModel GetById(string id)
        {
            try
            {
                return Connection.Find<EmpleadoModel>(id);
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
    }
}

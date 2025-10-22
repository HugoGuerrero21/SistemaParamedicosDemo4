using SistemaParamedicosDemo4.MVVM.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaParamedicosDemo4.Data.Repositories
{
    public class ConsultaRepository
    {
        private SQLiteConnection Connection;
        public string StatusMessage { get; set; }

        // Referencias a otros repositorios para cargar objetos relacionados
        private EmpleadoRepository _empleadoRepo;
        private TipoEnfermedadRepository _tipoEnfermedadRepo;
        private ProductoRepository _productoRepo;
        private MovimientoDetalleRepository _movimientoDetalleRepo;

        public ConsultaRepository()
        {
            try
            {
                // Usar la conexión compartida del DatabaseManager
                Connection = DatabaseManager.Instance.Connection;

                // ⭐ INICIALIZAR TODOS LOS REPOSITORIOS
                _empleadoRepo = new EmpleadoRepository();
                _tipoEnfermedadRepo = new TipoEnfermedadRepository();
                _productoRepo = new ProductoRepository();
                _movimientoDetalleRepo = new MovimientoDetalleRepository();

                StatusMessage = "Repositorio de consultas inicializado";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al inicializar tabla: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }
        }

        public List<ConsultaModel> GetAllConsultas()
        {
            try
            {
                return Connection.Table<ConsultaModel>().ToList();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener consultas: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return new List<ConsultaModel>();
            }
        }

        public ConsultaModel GetConsultaById(int id, bool cargarRelaciones = true)
        {
            try
            {
                var consulta = Connection.Find<ConsultaModel>(id);

                if (consulta != null && cargarRelaciones)
                {
                    // Cargar el empleado
                    consulta.Empleado = _empleadoRepo.GetById(consulta.IdEmpleado);

                    // Cargar el tipo de enfermedad
                    consulta.TipoEnfermedad = _tipoEnfermedadRepo.GetById(consulta.IdTipoEnfermedad);
                }

                return consulta;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener consulta: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return null;
            }
        }

        public List<ConsultaModel> GetConsultasByEmpleado(string idEmpleado)
        {
            try
            {
                return Connection.Table<ConsultaModel>()
                    .Where(c => c.IdEmpleado == idEmpleado)
                    .ToList();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener consultas: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return new List<ConsultaModel>();
            }
        }

        public List<ConsultaModel> GetConsultasByFecha(DateTime fecha)
        {
            try
            {
                return Connection.Table<ConsultaModel>()
                    .Where(c => c.FechaConsulta.Date == fecha.Date)
                    .ToList();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener consultas: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return new List<ConsultaModel>();
            }
        }

        public int InsertarConsulta(ConsultaModel consulta)
        {
            try
            {
                if (consulta == null)
                {
                    StatusMessage = "La consulta no puede ser nula";
                    return 0;
                }

                int result = Connection.Insert(consulta);

                if (result > 0)
                {
                    // Obtener el ID autogenerado
                    var ultimaConsulta = Connection.Table<ConsultaModel>()
                        .OrderByDescending(c => c.IdConsulta)
                        .FirstOrDefault();

                    StatusMessage = $"Consulta insertada con ID: {ultimaConsulta?.IdConsulta}";
                    System.Diagnostics.Debug.WriteLine(StatusMessage);

                    return ultimaConsulta?.IdConsulta ?? 0;
                }

                return 0;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al insertar consulta: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return 0;
            }
        }

        public bool GuardarConsultaCompleta(
            ConsultaModel consulta,
            List<MovimientoDetalleModel> medicamentos)
        {
            try
            {
                Connection.BeginTransaction();

                // 1. Insertar la consulta
                int idConsulta = InsertarConsulta(consulta);

                if (idConsulta == 0)
                {
                    Connection.Rollback();
                    StatusMessage = "Error al insertar consulta";
                    return false;
                }

                // 2. Si hay medicamentos, insertarlos
                if (medicamentos != null && medicamentos.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Insertando {medicamentos.Count} detalles de medicamentos...");

                    bool detallesInsertados = _movimientoDetalleRepo.InsertarDetalles(medicamentos);

                    if (!detallesInsertados)
                    {
                        Connection.Rollback();
                        StatusMessage = $"Error al insertar detalles de medicamentos: {_movimientoDetalleRepo.StatusMessage}";
                        System.Diagnostics.Debug.WriteLine(StatusMessage);
                        return false;
                    }

                    // 3. Actualizar el stock de los productos
                    foreach (var detalle in medicamentos)
                    {
                        System.Diagnostics.Debug.WriteLine($"Actualizando stock de {detalle.ClaveProducto} en -{detalle.Cantidad}");

                        bool stockActualizado = _productoRepo.ActualizarStock(
                            detalle.ClaveProducto,
                            -detalle.Cantidad); // Negativo porque se está usando

                        if (!stockActualizado)
                        {
                            Connection.Rollback();
                            StatusMessage = $"Error al actualizar stock del producto {detalle.ClaveProducto}";
                            System.Diagnostics.Debug.WriteLine(StatusMessage);
                            return false;
                        }
                    }
                }

                Connection.Commit();
                StatusMessage = "Consulta guardada correctamente con todos sus detalles";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
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

                StatusMessage = $"Error al guardar consulta completa: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ {StatusMessage}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public bool ActualizarConsulta(ConsultaModel consulta)
        {
            try
            {
                if (consulta == null)
                {
                    StatusMessage = "La consulta no puede ser nula";
                    return false;
                }

                int result = Connection.Update(consulta);
                StatusMessage = "Consulta actualizada correctamente";
                return result > 0;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al actualizar consulta: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }
    }
}
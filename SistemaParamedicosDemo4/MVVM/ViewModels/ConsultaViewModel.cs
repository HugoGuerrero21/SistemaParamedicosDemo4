using PropertyChanged;
using SistemaParamedicosDemo4.MVVM.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SistemaParamedicosDemo4.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ConsultaViewModel : INotifyPropertyChanged
    {
        #region Properties

        // Modelo principal de la consulta
        public ConsultaModel Consulta { get; set; }
        public EmpleadoModel EmpleadoSeleccionado { get; set; }
        public int Edad { get; set; }

        // Signos vitales
        public string TensionArterial { get; set; }
        public string Temperatura { get; set; }
        public short FrecuenciaCardiaca { get; set; }
        public byte FrecuenciaRespiratoria { get; set; }
        public string ObservacionesSignos { get; set; }

        // Consulta
        public string MotivoConsulta { get; set; }
        public string Diagnostico { get; set; }
        public TipoEnfermedadModel TipoEnfermedadSeleccionado { get; set; }
        public string Tratamiento { get; set; }
        public string UltimaComida { get; set; }

        // Material/Medicamentos
        public bool SeUtilizoMaterial { get; set; }
        public ProductoModel MedicamentoSeleccionado { get; set; }
        public double CantidadDisponible { get; set; }
        public double CantidadMedicamento { get; set; }
        public string ObservacionesMedicamento { get; set; }

        public bool TieneMedicamentosAgregados => MedicamentosAgregados?.Count > 0;

        // Colecciones
        public ObservableCollection<TipoEnfermedadModel> TiposEnfermedad { get; set; }
        public ObservableCollection<ProductoModel> Medicamentos { get; set; }
        public ObservableCollection<MovimientoDetalleModel> MedicamentosAgregados { get; set; }


        #endregion

        #region Commands

        public ICommand AgregarMedicamentoCommand { get; }
        public ICommand EliminarMedicamentoCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }

        #endregion

        public ConsultaViewModel()
        {
            Consulta = new ConsultaModel { FechaConsulta = DateTime.Now };

            TiposEnfermedad = new ObservableCollection<TipoEnfermedadModel>();
            Medicamentos = new ObservableCollection<ProductoModel>();
            MedicamentosAgregados = new ObservableCollection<MovimientoDetalleModel>();


            // Cuando cambie la colección, notificar que la propiedad calculada cambió
            MedicamentosAgregados.CollectionChanged += (s, e) =>
            {
                this.OnPropertyChanged(nameof(TieneMedicamentosAgregados));
            };

            // PRIMERO inicializa los Commands
            AgregarMedicamentoCommand = new Command(AgregarMedicamento, CanAgregarMedicamento);
            EliminarMedicamentoCommand = new Command<MovimientoDetalleModel>(EliminarMedicamento);
            GuardarCommand = new Command(Guardar, CanGuardar);
            CancelarCommand = new Command(Cancelar);

            // LUEGO configura el PropertyChanged para actualizar los Commands
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MedicamentoSeleccionado))
                {
                    ActualizarCantidadDisponible();
                    ((Command)AgregarMedicamentoCommand).ChangeCanExecute();
                }
                else if (e.PropertyName == nameof(CantidadMedicamento) ||
                         e.PropertyName == nameof(SeUtilizoMaterial) ||
                         e.PropertyName == nameof(CantidadDisponible))
                {
                    ((Command)AgregarMedicamentoCommand).ChangeCanExecute();
                }
                else if (e.PropertyName == nameof(MotivoConsulta) ||
                         e.PropertyName == nameof(Diagnostico) ||
                         e.PropertyName == nameof(TipoEnfermedadSeleccionado))
                {
                    ((Command)GuardarCommand).ChangeCanExecute();
                }
            };

            AgregarMedicamentoCommand = new Command(AgregarMedicamento, CanAgregarMedicamento);
            EliminarMedicamentoCommand = new Command<MovimientoDetalleModel>(EliminarMedicamento);
            GuardarCommand = new Command(Guardar, CanGuardar);
            CancelarCommand = new Command(Cancelar);

            CargarDatosIniciales();
            // Establecer explícitamente el valor inicial
        }

        private void CargarDatosIniciales()
        {
            // Cargar tipos de enfermedad
            TiposEnfermedad.Add(new TipoEnfermedadModel { IdTipoEnfermedad = 1, NombreEnfermedad = "Musculoesquelético" });
            TiposEnfermedad.Add(new TipoEnfermedadModel { IdTipoEnfermedad = 2, NombreEnfermedad = "Respiratorio" });
            TiposEnfermedad.Add(new TipoEnfermedadModel { IdTipoEnfermedad = 3, NombreEnfermedad = "Cardiovascular" });
            TiposEnfermedad.Add(new TipoEnfermedadModel { IdTipoEnfermedad = 4, NombreEnfermedad = "Digestivo" });
            TiposEnfermedad.Add(new TipoEnfermedadModel { IdTipoEnfermedad = 5, NombreEnfermedad = "Neurológico" });
            TiposEnfermedad.Add(new TipoEnfermedadModel { IdTipoEnfermedad = 6, NombreEnfermedad = "Otros" });

            // Cargar medicamentos con cantidad disponible
            Medicamentos.Add(new ProductoModel
            {
                ProductoId = "MED001",
                Nombre = "Paracetamol",
                Marca = "Genérico",
                Model = "500mg",
                CantidadDisponible = 150
            });
            Medicamentos.Add(new ProductoModel
            {
                ProductoId = "MED002",
                Nombre = "Ibuprofeno",
                Marca = "Genérico",
                Model = "400mg",
                CantidadDisponible = 80
            });
            Medicamentos.Add(new ProductoModel
            {
                ProductoId = "MED003",
                Nombre = "Aspirina",
                Marca = "Bayer",
                Model = "100mg",
                CantidadDisponible = 200
            });
            Medicamentos.Add(new ProductoModel
            {
                ProductoId = "MED004",
                Nombre = "Amoxicilina",
                Marca = "Genérico",
                Model = "500mg",
                CantidadDisponible = 50
            });

            // Cargar empleado de ejemplo
            EmpleadoSeleccionado = new EmpleadoModel
            {
                IdEmpleado = "TRSEMP001",
                Nombre = "Juan Carlos Perez Hernandez",
                TipoSangre = "A+",
                SexoSangre = "M",
                AlergiasSangre = "Ninguna",
                Telefono = "6621234567",
                FechaNacimiento = new DateTime(1990, 5, 15),
                IdPuesto = "CHOFER"
            };
        }

        private void CalcularEdad()
        {
            if (EmpleadoSeleccionado != null)
            {
                var hoy = DateTime.Today;
                var edad = hoy.Year - EmpleadoSeleccionado.FechaNacimiento.Year;
                if (EmpleadoSeleccionado.FechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
                Edad = edad;
            }
        }

        private void ActualizarCantidadDisponible()
        {
            if (MedicamentoSeleccionado != null)
            {
                CantidadDisponible = MedicamentoSeleccionado.CantidadDisponible;
            }
            else
            {
                CantidadDisponible = 0;
            }
        }

        private bool CanAgregarMedicamento()
        {
            return SeUtilizoMaterial &&
                   MedicamentoSeleccionado != null &&
                   CantidadMedicamento > 0 &&
                   CantidadMedicamento <= CantidadDisponible;
        }

        private async void AgregarMedicamento()
        {
            if (MedicamentoSeleccionado != null && CantidadMedicamento > 0)
            {
                // Validar que no exceda la cantidad disponible
                if (CantidadMedicamento > CantidadDisponible)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"La cantidad solicitada ({CantidadMedicamento}) excede la cantidad disponible ({CantidadDisponible})",
                        "OK");
                    return;
                }

                // Verificar si el medicamento ya fue agregado
                var medicamentoExistente = MedicamentosAgregados
                    .FirstOrDefault(m => m.ClaveProducto == MedicamentoSeleccionado.ProductoId);

                if (medicamentoExistente != null)
                {
                    bool actualizar = await Application.Current.MainPage.DisplayAlert(
                        "Medicamento existente",
                        "Este medicamento ya ha sido agregado. ¿Desea aumentar la cantidad?",
                        "Sí",
                        "No");

                    if (actualizar)
                    {
                        // Validar que la suma no exceda lo disponible
                        double nuevaCantidad = medicamentoExistente.Cantidad + CantidadMedicamento;
                        double cantidadActualDisponible = MedicamentoSeleccionado.CantidadDisponible + medicamentoExistente.Cantidad;

                        if (nuevaCantidad > cantidadActualDisponible)
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Error",
                                $"La cantidad total ({nuevaCantidad}) excedería la cantidad disponible ({cantidadActualDisponible})",
                                "OK");
                            return;
                        }

                        // Actualizar la cantidad
                        medicamentoExistente.Cantidad = nuevaCantidad;
                        medicamentoExistente.Observaciones = ObservacionesMedicamento ?? medicamentoExistente.Observaciones;

                        // Actualizar inventario
                        MedicamentoSeleccionado.CantidadDisponible -= CantidadMedicamento;
                    }

                    LimpiarCamposMedicamento();
                    return;
                }

                // Agregar el medicamento
                var movimientoDetalle = new MovimientoDetalleModel
                {
                    IdMovimientDetalles = Guid.NewGuid().ToString(),
                    ClaveProducto = MedicamentoSeleccionado.ProductoId,
                    Producto = MedicamentoSeleccionado, // Asignar la referencia al producto
                    Cantidad = CantidadMedicamento,
                    Observaciones = string.IsNullOrWhiteSpace(ObservacionesMedicamento)
                        ? "Sin observaciones"
                        : ObservacionesMedicamento,
                    Status = 1
                };

                MedicamentosAgregados.Add(movimientoDetalle);

                // Actualizar cantidad disponible en el producto
                MedicamentoSeleccionado.CantidadDisponible -= CantidadMedicamento;

                // Limpiar campos
                LimpiarCamposMedicamento();

                await Application.Current.MainPage.DisplayAlert(
                    "Éxito",
                    "Medicamento agregado correctamente",
                    "OK");
            }
        }

        private async void EliminarMedicamento(MovimientoDetalleModel medicamento)
        {
            if (medicamento != null)
            {
                bool respuesta = await Application.Current.MainPage.DisplayAlert(
                    "Confirmar",
                    $"¿Desea eliminar {medicamento.NombreMedicamento}?",
                    "Sí",
                    "No");

                if (respuesta)
                {
                    // Devolver la cantidad al inventario
                    var productoOriginal = Medicamentos
                        .FirstOrDefault(p => p.ProductoId == medicamento.ClaveProducto);

                    if (productoOriginal != null)
                    {
                        productoOriginal.CantidadDisponible += medicamento.Cantidad;
                    }

                    MedicamentosAgregados.Remove(medicamento);
                }
            }
        }

        private void LimpiarCamposMedicamento()
        {
            MedicamentoSeleccionado = null;
            CantidadMedicamento = 0;
            ObservacionesMedicamento = string.Empty;
            CantidadDisponible = 0;
        }

        private bool CanGuardar()
        {
            return EmpleadoSeleccionado != null &&
                   !string.IsNullOrWhiteSpace(MotivoConsulta) &&
                   !string.IsNullOrWhiteSpace(Diagnostico) &&
                   TipoEnfermedadSeleccionado != null;
        }

        private async void Guardar()
        {
            try
            {
                // Validar que si se marcó el checkbox, debe haber medicamentos agregados
                if (SeUtilizoMaterial && MedicamentosAgregados.Count == 0)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Advertencia",
                        "Ha indicado que se utilizó material pero no ha agregado ningún medicamento.",
                        "OK");
                    return;
                }

                // Actualizar el modelo de consulta con los datos del formulario
                Consulta.IdEmpleado = EmpleadoSeleccionado.IdEmpleado;
                Consulta.Empleado = EmpleadoSeleccionado;
                Consulta.FechaConsulta = DateTime.Now;
                Consulta.MotivoConsulta = MotivoConsulta;
                Consulta.Diagnostico = Diagnostico;
                Consulta.IdTipoEnfermedad = TipoEnfermedadSeleccionado.IdTipoEnfermedad;
                Consulta.TipoEnfermedad = TipoEnfermedadSeleccionado;
                Consulta.FrecuenciaCardiaca = FrecuenciaCardiaca;
                Consulta.FrecuenciaRespiratoria = FrecuenciaRespiratoria;
                Consulta.Temperatura = Temperatura;
                Consulta.PresionArterial = TensionArterial;
                Consulta.Observaciones = ObservacionesSignos;
                Consulta.UltimaComida = UltimaComida;

                // Si se utilizó material, generar un ID de movimiento
                if (SeUtilizoMaterial && MedicamentosAgregados.Count > 0)
                {
                    Consulta.IdMovimiento = Guid.NewGuid().ToString();

                    // Asociar el IdMovimiento a todos los detalles
                    foreach (var detalle in MedicamentosAgregados)
                    {
                        detalle.IdMovimiento = Consulta.IdMovimiento;
                    }
                }

                // Aquí deberías guardar en la base de datos
                // Ejemplo:
                // await _consultaService.GuardarConsulta(Consulta);
                // if (MedicamentosAgregados.Count > 0)
                // {
                //     await _movimientoService.GuardarMovimientoDetalles(MedicamentosAgregados);
                // }

                await Application.Current.MainPage.DisplayAlert(
                    "Éxito",
                    "Consulta guardada correctamente",
                    "OK");

                // Navegar hacia atrás o limpiar el formulario
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error al guardar: {ex.Message}",
                    "OK");
            }
        }

        private async void Cancelar()
        {
            bool respuesta = await Application.Current.MainPage.DisplayAlert(
                "Cancelar",
                "¿Está seguro que desea cancelar? Se perderán todos los cambios.",
                "Sí",
                "No");

            if (respuesta)
            {
                // Devolver las cantidades al inventario
                foreach (var medicamento in MedicamentosAgregados)
                {
                    var productoOriginal = Medicamentos
                        .FirstOrDefault(p => p.ProductoId == medicamento.ClaveProducto);

                    if (productoOriginal != null)
                    {
                        productoOriginal.CantidadDisponible += medicamento.Cantidad;
                    }
                }

                LimpiarFormulario();
            }
        }

        private bool CamposModificados()
        {
            return !string.IsNullOrWhiteSpace(MotivoConsulta) ||
                   !string.IsNullOrWhiteSpace(Diagnostico) ||
                   !string.IsNullOrWhiteSpace(TensionArterial) ||
                   !string.IsNullOrWhiteSpace(Temperatura) ||
                   FrecuenciaCardiaca > 0 ||
                   FrecuenciaRespiratoria > 0 ||
                   MedicamentosAgregados.Count > 0;
        }

        private void LimpiarFormulario()
        {
            TensionArterial = string.Empty;
            Temperatura = string.Empty;
            FrecuenciaCardiaca = 0;
            FrecuenciaRespiratoria = 0;
            ObservacionesSignos = string.Empty;
            MotivoConsulta = string.Empty;
            Diagnostico = string.Empty;
            TipoEnfermedadSeleccionado = null;
            Tratamiento = string.Empty;
            UltimaComida = string.Empty;
            SeUtilizoMaterial = false;
            MedicamentosAgregados.Clear();
            LimpiarCamposMedicamento();

            // Reiniciar el modelo de consulta
            Consulta = new ConsultaModel
            {
                FechaConsulta = DateTime.Now
            };
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
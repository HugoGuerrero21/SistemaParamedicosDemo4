using PropertyChanged;
using SistemaParamedicosDemo4.Data.Repositories;
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
        #region Repositorios
        private TipoEnfermedadRepository _tipoEnfermedadRepo;
        private ProductoRepository _productoRepo;
        private ConsultaRepository _consultaRepo;
        private EmpleadoRepository _empleadoRepo;
        #endregion

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

        // ⭐ PROPIEDADES PARA HISTORIAL
        public ObservableCollection<ConsultaResumenModel> UltimasConsultas { get; set; }
        public bool TieneConsultasPrevias => UltimasConsultas?.Count > 0;
        public bool HistorialExpandido { get; set; }

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

        public ICommand AgregarMedicamentoCommand { get; }
        public ICommand EliminarMedicamentoCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand ToggleHistorialCommand { get; }

        #endregion

        public ConsultaViewModel()
        {
            // Inicializar repositorios
            _tipoEnfermedadRepo = new TipoEnfermedadRepository();
            _productoRepo = new ProductoRepository();
            _consultaRepo = new ConsultaRepository();
            _empleadoRepo = new EmpleadoRepository();

            Consulta = new ConsultaModel { FechaConsulta = DateTime.Now };

            TiposEnfermedad = new ObservableCollection<TipoEnfermedadModel>();
            Medicamentos = new ObservableCollection<ProductoModel>();
            MedicamentosAgregados = new ObservableCollection<MovimientoDetalleModel>();
            UltimasConsultas = new ObservableCollection<ConsultaResumenModel>();

            // Cuando cambie la colección, notificar que la propiedad calculada cambió
            MedicamentosAgregados.CollectionChanged += (s, e) =>
            {
                this.OnPropertyChanged(nameof(TieneMedicamentosAgregados));
            };

            // Inicializar Commands
            AgregarMedicamentoCommand = new Command(AgregarMedicamento, CanAgregarMedicamento);
            EliminarMedicamentoCommand = new Command<MovimientoDetalleModel>(EliminarMedicamento);
            GuardarCommand = new Command(Guardar, CanGuardar);
            CancelarCommand = new Command(Cancelar);
            ToggleHistorialCommand = new Command(() => HistorialExpandido = !HistorialExpandido);

            // Configurar PropertyChanged
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

            // Cargar datos desde SQLite
            CargarDatosDesdeBaseDatos();
        }

        private void CargarDatosDesdeBaseDatos()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Cargando datos desde SQLite...");

                var tiposEnfermedad = _tipoEnfermedadRepo.GetAllTypes();
                foreach (var tipo in tiposEnfermedad)
                {
                    TiposEnfermedad.Add(tipo);
                }
                System.Diagnostics.Debug.WriteLine($"✓ {TiposEnfermedad.Count} tipos de enfermedad cargados");

                var productos = _productoRepo.GetProductoConsStock();
                foreach (var producto in productos)
                {
                    Medicamentos.Add(producto);
                }
                System.Diagnostics.Debug.WriteLine($"✓ {Medicamentos.Count} productos cargados");

                var empleados = _empleadoRepo.GetAll();
                if (empleados.Count > 0)
                {
                    EmpleadoSeleccionado = empleados[0];
                    CalcularEdad();
                    System.Diagnostics.Debug.WriteLine($"✓ Empleado cargado: {EmpleadoSeleccionado.Nombre}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar datos: {ex.Message}");
            }
        }

        private void CalcularEdad()
        {
            if (EmpleadoSeleccionado != null)
            {
                var hoy = DateTime.Today;
                var edad = hoy.Year - EmpleadoSeleccionado.FechaNacimiento.Year;
                if (EmpleadoSeleccionado.FechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
                Edad = edad;

                CargarUltimasConsultas();
            }
        }

        private void CargarUltimasConsultas()
        {
            try
            {
                if (EmpleadoSeleccionado == null) return;

                UltimasConsultas.Clear();

                var consultas = _consultaRepo.GetConsultasByEmpleado(EmpleadoSeleccionado.IdEmpleado)
                    .OrderByDescending(c => c.FechaConsulta)
                    .Take(5)
                    .ToList();

                foreach (var consulta in consultas)
                {
                    consulta.TipoEnfermedad = _tipoEnfermedadRepo.GetById(consulta.IdTipoEnfermedad);

                    var resumen = new ConsultaResumenModel
                    {
                        FechaConsulta = consulta.FechaConsulta,
                        MotivoConsulta = consulta.MotivoConsulta,
                        Diagnostico = consulta.Diagnostico,
                        TipoEnfermedad = consulta.TipoEnfermedad?.NombreEnfermedad ?? "Sin clasificar"
                    };

                    UltimasConsultas.Add(resumen);
                }

                System.Diagnostics.Debug.WriteLine($"✓ {UltimasConsultas.Count} consultas previas cargadas");
                OnPropertyChanged(nameof(TieneConsultasPrevias));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar últimas consultas: {ex.Message}");
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
                if (CantidadMedicamento > CantidadDisponible)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"La cantidad solicitada ({CantidadMedicamento}) excede la cantidad disponible ({CantidadDisponible})",
                        "OK");
                    return;
                }

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

                        medicamentoExistente.Cantidad = nuevaCantidad;
                        medicamentoExistente.Observaciones = ObservacionesMedicamento ?? medicamentoExistente.Observaciones;
                        MedicamentoSeleccionado.CantidadDisponible -= CantidadMedicamento;
                    }

                    LimpiarCamposMedicamento();
                    return;
                }

                var movimientoDetalle = new MovimientoDetalleModel
                {
                    IdMovimientoDetalle = Guid.NewGuid().ToString(),
                    ClaveProducto = MedicamentoSeleccionado.ProductoId,
                    Producto = MedicamentoSeleccionado,
                    Cantidad = CantidadMedicamento,
                    Observaciones = string.IsNullOrWhiteSpace(ObservacionesMedicamento)
                        ? "Sin observaciones"
                        : ObservacionesMedicamento,
                    Status = 1
                };

                MedicamentosAgregados.Add(movimientoDetalle);
                MedicamentoSeleccionado.CantidadDisponible -= CantidadMedicamento;
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
                if (SeUtilizoMaterial && MedicamentosAgregados.Count == 0)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Advertencia",
                        "Ha indicado que se utilizó material pero no ha agregado ningún medicamento.",
                        "OK");
                    return;
                }

                var idEmpleadoGuardado = EmpleadoSeleccionado?.IdEmpleado;

                Consulta.IdUsuarioAcceso = Preferences.Get("IdUsuario", string.Empty);
                Consulta.IdEmpleado = EmpleadoSeleccionado.IdEmpleado;
                Consulta.FechaConsulta = DateTime.Now;
                Consulta.MotivoConsulta = MotivoConsulta;
                Consulta.Diagnostico = Diagnostico;
                Consulta.IdTipoEnfermedad = TipoEnfermedadSeleccionado.IdTipoEnfermedad;
                Consulta.FrecuenciaCardiaca = FrecuenciaCardiaca;
                Consulta.FrecuenciaRespiratoria = FrecuenciaRespiratoria;
                Consulta.Temperatura = Temperatura;
                Consulta.PresionArterial = TensionArterial;
                Consulta.Observaciones = ObservacionesSignos;
                Consulta.UltimaComida = UltimaComida;

                if (SeUtilizoMaterial && MedicamentosAgregados.Count > 0)
                {
                    Consulta.IdMovimiento = Guid.NewGuid().ToString();

                    foreach (var detalle in MedicamentosAgregados)
                    {
                        detalle.IdMovimiento = Consulta.IdMovimiento;
                    }
                }

                bool guardado = _consultaRepo.GuardarConsultaCompleta(
                    Consulta,
                    MedicamentosAgregados.ToList());

                if (guardado)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Éxito",
                        "Consulta guardada correctamente en la base de datos",
                        "OK");

                    System.Diagnostics.Debug.WriteLine("✓ Consulta guardada en SQLite");

                    if (!string.IsNullOrEmpty(idEmpleadoGuardado))
                    {
                        MessagingCenter.Send(this, "ConsultaGuardada", idEmpleadoGuardado);
                        System.Diagnostics.Debug.WriteLine($"✓ Mensaje enviado para actualizar empleado {idEmpleadoGuardado}");
                    }

                    LimpiarFormulario();

                    Medicamentos.Clear();
                    var productos = _productoRepo.GetProductoConsStock();
                    foreach (var producto in productos)
                    {
                        Medicamentos.Add(producto);
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"Error al guardar: {_consultaRepo.StatusMessage}",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error al guardar: {ex.Message}",
                    "OK");
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
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

            Consulta = new ConsultaModel
            {
                FechaConsulta = DateTime.Now
            };

            // ⭐ REFRESCAR LAS ÚLTIMAS CONSULTAS
            CargarUltimasConsultas();
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

// ⭐ MODELO PARA MOSTRAR RESUMEN DE CONSULTAS
public class ConsultaResumenModel
{
    public DateTime FechaConsulta { get; set; }
    public string MotivoConsulta { get; set; }
    public string Diagnostico { get; set; }
    public string TipoEnfermedad { get; set; }
    public string FechaFormateada => FechaConsulta.ToString("dd/MM/yyyy HH:mm");
}
using PropertyChanged;
using SistemaParamedicosDemo4.Data.Repositories;
using SistemaParamedicosDemo4.MVVM.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SistemaParamedicosDemo4.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    [QueryProperty(nameof(IdConsulta), "IdConsulta")]
    public class DetalleConsultaViewModel
    {
        #region Repositorios
        private ConsultaRepository _consultaRepo;
        #endregion

        #region Properties
        private int _idConsulta;
        public int IdConsulta
        {
            get => _idConsulta;
            set
            {
                _idConsulta = value;
                CargarDetalleConsulta();
            }
        }

        public ConsultaModel Consulta { get; set; }
        public ObservableCollection<MovimientoDetalleModel> Medicamentos { get; set; }
        public int TotalMedicamentos { get; set; }
        public bool TieneMedicamentos { get; set; }
        public bool TieneObservacionesSignos { get; set; }
        public bool TieneUltimaComida { get; set; }

        // ⭐ Propiedades para el header del empleado
        public int EdadEmpleado { get; set; }
        public int TotalConsultasEmpleado { get; set; }
        #endregion

        #region Commands
        public ICommand CerrarCommand { get; }
        #endregion

        public DetalleConsultaViewModel()
        {
            _consultaRepo = new ConsultaRepository();
            Medicamentos = new ObservableCollection<MovimientoDetalleModel>();
            CerrarCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private void CargarDetalleConsulta()
        {
            try
            {
                var detalleCompleto = _consultaRepo.GetConsultaCompleta(IdConsulta);

                if (detalleCompleto == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ No se pudo cargar el detalle de la consulta");
                    return;
                }

                Consulta = detalleCompleto.Consulta;

                // ⭐ CALCULAR EDAD DEL EMPLEADO
                if (Consulta.Empleado != null)
                {
                    var hoy = DateTime.Today;
                    var edad = hoy.Year - Consulta.Empleado.FechaNacimiento.Year;
                    if (Consulta.Empleado.FechaNacimiento.Date > hoy.AddYears(-edad))
                        edad--;

                    EdadEmpleado = edad;

                    // ⭐ OBTENER TOTAL DE CONSULTAS DEL EMPLEADO
                    var todasLasConsultas = _consultaRepo.GetConsultasByEmpleado(Consulta.IdEmpleado);
                    TotalConsultasEmpleado = todasLasConsultas.Count;

                    System.Diagnostics.Debug.WriteLine($"✓ Empleado: {Consulta.Empleado.Nombre}");
                    System.Diagnostics.Debug.WriteLine($"✓ Edad: {EdadEmpleado} años");
                    System.Diagnostics.Debug.WriteLine($"✓ Total consultas: {TotalConsultasEmpleado}");
                }

                // Cargar medicamentos
                Medicamentos.Clear();
                if (detalleCompleto.Medicamentos != null && detalleCompleto.Medicamentos.Count > 0)
                {
                    foreach (var medicamento in detalleCompleto.Medicamentos)
                    {
                        Medicamentos.Add(medicamento);
                    }
                    TotalMedicamentos = Medicamentos.Count;
                    TieneMedicamentos = true;
                }
                else
                {
                    TieneMedicamentos = false;
                }

                // Validar propiedades opcionales
                TieneObservacionesSignos = !string.IsNullOrWhiteSpace(Consulta.Observaciones);
                TieneUltimaComida = !string.IsNullOrWhiteSpace(Consulta.UltimaComida);

                System.Diagnostics.Debug.WriteLine($"✓ Detalle de consulta #{IdConsulta} cargado");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar detalle: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }
    }
}
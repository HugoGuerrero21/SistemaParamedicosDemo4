using SistemaParamedicosDemo4.MVVM.Views;

namespace SistemaParamedicosDemo4
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrar rutas de navegación
            Routing.RegisterRoute("empleados", typeof(EmpleadosListView));
            Routing.RegisterRoute("consulta", typeof(ConsultaView));
            Routing.RegisterRoute("historial", typeof(HistorialConsultasView));
            Routing.RegisterRoute("detalleConsulta", typeof(DetalleConsultaView));
            Routing.RegisterRoute("inventario", typeof(InventarioView));

            // ⭐ REGISTRAR LA NUEVA RUTA
            Routing.RegisterRoute("tiposEnfermedad", typeof(GestionTiposEnfermedadView));
        }

        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert(
                "Cerrar Sesión",
                "¿Está seguro que desea cerrar sesión?",
                "Sí",
                "No");

            if (confirmar)
            {
                // Regresar al Login
                Application.Current.MainPage = new NavigationPage(new LoginView());
            }
        }
    }
}
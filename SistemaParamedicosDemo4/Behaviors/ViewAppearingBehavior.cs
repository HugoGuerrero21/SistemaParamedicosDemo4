using System.Windows.Input;

namespace SistemaParamedicosDemo4.Behaviors
{
    public class ViewAppearingBehavior : Behavior<ContentPage>
    {
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ViewAppearingBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttachedTo(ContentPage bindable)
        {
            base.OnAttachedTo(bindable);
            System.Diagnostics.Debug.WriteLine("🔧 ViewAppearingBehavior adjuntado a la página");
            bindable.Appearing += OnAppearing;
        }

        protected override void OnDetachingFrom(ContentPage bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.Appearing -= OnAppearing;
        }

        private void OnAppearing(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("🎯 ViewAppearingBehavior.OnAppearing EJECUTADO");
            if (Command != null)
            {
                System.Diagnostics.Debug.WriteLine("🎯 Ejecutando comando...");
                Command.Execute(null);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Command es NULL!");
            }
        }
    }
}
using System.Text.RegularExpressions;

namespace SistemaParamedicosDemo4.Behaviors
{
    /// <summary>
    /// Behavior que solo permite números enteros
    /// </summary>
    public class NumericValidationBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(entry);
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not Entry entry) return;

            if (string.IsNullOrEmpty(e.NewTextValue))
                return;

            // Solo permitir números
            if (!int.TryParse(e.NewTextValue, out _))
            {
                entry.Text = e.OldTextValue;
            }
        }
    }

    /// <summary>
    /// Behavior que solo permite números decimales (temperatura)
    /// </summary>
    public class DecimalValidationBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(entry);
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not Entry entry) return;

            if (string.IsNullOrEmpty(e.NewTextValue))
                return;

            // Permitir números y un solo punto decimal
            var regex = new Regex(@"^[0-9]*\.?[0-9]*$");
            if (!regex.IsMatch(e.NewTextValue))
            {
                entry.Text = e.OldTextValue;
            }
        }
    }

    /// <summary>
    /// Behavior para tensión arterial (formato: 120/80)
    /// </summary>
    public class TensionArterialBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(entry);
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not Entry entry) return;

            if (string.IsNullOrEmpty(e.NewTextValue))
                return;

            // Permitir formato: números/números (ejemplo: 120/80)
            var regex = new Regex(@"^[0-9]*\/?[0-9]*$");
            if (!regex.IsMatch(e.NewTextValue))
            {
                entry.Text = e.OldTextValue;
            }
        }
    }
}
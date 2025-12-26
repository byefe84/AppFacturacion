using FacturacionDobleEje.Data;
using FacturacionDobleEje.Models;
using FacturacionDobleEje.Models.FacturacionConstruccion.Models;
using FacturacionDobleEje.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace FacturacionDobleEje
{
    public partial class MainWindow : Window
    {
        private readonly MockRepository _repo;
        private ObservableCollection<InvoiceLine> _lineas;
        private Invoice _currentInvoice;

        public MainWindow()
        {
            _repo = new MockRepository();
            _currentInvoice = new Invoice { Client = _repo.Clients.First() };
            _lineas = new ObservableCollection<InvoiceLine>();

            InitializeComponent();

            dgMateriales.ItemsSource = _repo.Materials;
            cbClientes.ItemsSource = _repo.Clients;
            dgLineas.ItemsSource = _lineas;
            cbClientes.SelectedIndex = 0;

            UpdateTotals();
        }

        private void BtnAddSelected_Click(object sender, RoutedEventArgs e)
        {
            var mat = dgMateriales.SelectedItem as Material;
            if (mat == null)
            {
                MessageBox.Show("Selecciona un material", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Pedimos cantidad con un dialogo simple
            var input = Microsoft.VisualBasic.Interaction.InputBox($"Cantidad de '{mat.Name}' a añadir:", "Cantidad", "1");
            if (string.IsNullOrWhiteSpace(input)) return;

            if (!TryParseDecimalFlexible(input, out var cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Introduce una cantidad válida.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Si ya existe la línea para ese material, sumamos
            var existing = _lineas.FirstOrDefault(l => l.Material.Id == mat.Id);
            if (existing != null)
            {
                existing.Quantity += cantidad;
            }
            else
            {
                _lineas.Add(new InvoiceLine { Material = mat, Quantity = cantidad });
            }

            dgLineas.Items.Refresh();
            UpdateTotals();
        }

        private void NormalizeDecimalInput(object sender, TextChangedEventArgs e)
        {
            var txt = sender as TextBox;
            if (txt == null) return;

            string original = txt.Text;
            string normalized = original.Replace('.', ',');

            if (original != normalized)
            {
                int caret = txt.CaretIndex;
                txt.Text = normalized;

                // Ajustar posición del cursor si se reemplazó el carácter
                txt.CaretIndex = Math.Min(caret, txt.Text.Length);
            }
        }

        private void dg_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                e.Handled = true;

                var textBox = e.OriginalSource as TextBox;
                if (textBox != null)
                {
                    int caret = textBox.CaretIndex;
                    textBox.Text = textBox.Text.Insert(caret, ",");
                    textBox.CaretIndex = caret + 1;
                }
            }
        }

        private void dg_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                text = text.Replace('.', ',');

                DataObject newDataObject = new DataObject(text);
                e.DataObject = newDataObject;
            }
        }

        private void UpdateTotals()
        {
            // ============================
            // 0. Verificar que _repo y _currentInvoice existen
            // ============================
            if (_repo == null || _currentInvoice == null)
                return;

            // ============================
            // 1. Leer IVA con parseo flexible
            // ============================
            if (!TryParseDecimalFlexible(txtIVA.Text, out var ivaPercent))
            {
                ivaPercent = 21m;
                if (txtIVA != null) txtIVA.Text = "21";
            }
            _currentInvoice.VATType = ivaPercent / 100m;

            // ============================
            // 2. Leer descuento global
            // ============================
            decimal discountPercent = 0m;
            if (TryParseDecimalFlexible(txtDiscount.Text, out var d))
                discountPercent = d / 100m;

            // ============================
            // 3. Recalcular descuento por línea
            // ============================
            if (_lineas != null)
            {
                foreach (var line in _lineas)
                {
                    var gross = line.GrossAmount;
                    line.DiscountAmount = Math.Round(gross * discountPercent, 2, MidpointRounding.AwayFromZero);
                }

                if (dgLineas != null) dgLineas.Items.Refresh();
            }

            // ============================
            // 4. Calcular totales
            // ============================
            _currentInvoice.Lines = _lineas?.ToList() ?? new List<InvoiceLine>();

            decimal subtotal = _currentInvoice.Subtotal;
            decimal ivaTotal = Math.Round(subtotal * _currentInvoice.VATType, 2, MidpointRounding.AwayFromZero);
            decimal total = subtotal + ivaTotal;

            // ============================
            // 5. Actualizar UI
            // ============================
            if (tbSubtotal != null) tbSubtotal.Text = $"Subtotal: {subtotal:F2} €";
            if (tbIVA != null) tbIVA.Text = $"IVA ({ivaPercent:F2}%): {ivaTotal:F2} €";
            if (tbTotal != null) tbTotal.Text = $"Total: {total:F2} €";
        }

        private void TxtDiscount_Changed(object sender, TextChangedEventArgs e)
        {
            var txt = sender as TextBox;
            if (txt == null) return;

            // 1. Normalizar "." → ","
            string original = txt.Text;
            string normalized = original.Replace('.', ',');

            if (original != normalized)
            {
                int caret = txt.CaretIndex;
                txt.Text = normalized;
                txt.CaretIndex = Math.Min(caret + (normalized.Length - original.Length), txt.Text.Length);
            }

            // 2. Recalcular totales en tiempo real
            UpdateTotals();
        }

        private void TxtIVA_Changed(object sender, TextChangedEventArgs e)
        {
            var txt = sender as TextBox;
            if (txt == null) return;

            // 1. Normalizar "." → ","
            string original = txt.Text;
            string normalized = original.Replace('.', ',');

            if (original != normalized)
            {
                int caret = txt.CaretIndex;
                txt.Text = normalized;
                txt.CaretIndex = Math.Min(caret + (normalized.Length - original.Length), txt.Text.Length);
            }

            // 2. Recalcular totales
            UpdateTotals();
        }

        private bool TryParseDecimalFlexible(string input, out decimal value)
        {
            value = 0m;
            if (string.IsNullOrWhiteSpace(input)) return false;

            // 1) Intentar con la cultura actual (ej. es-ES usa ',')
            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out value))
                return true;

            // 2) Intentar con la cultura invariante (usa '.')
            var normalized = input.Replace(',', '.');
            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
                return true;

            // 3) Intentar reemplazando '.' por ',' y volver a probar con cultura actual
            normalized = input.Replace('.', ',');
            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.CurrentCulture, out value))
                return true;

            // Fallback: remover espacios y probar una vez más con invariante
            normalized = input.Replace(" ", "").Replace(',', '.');
            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        private void BtnGenerarPdf_Click(object sender, RoutedEventArgs e)
        {
            var cliente = cbClientes.SelectedItem as Client;
            if (cliente == null)
            {
                MessageBox.Show("Selecciona un cliente.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_lineas.Any())
            {
                MessageBox.Show("Añade alguna línea al presupuesto.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentInvoice.Client = cliente;

            // Guardar como PDF
            var dlg = new SaveFileDialog
            {
                FileName = $"Presupuesto_{_currentInvoice.Reference}.pdf",
                Filter = "PDF Files|*.pdf"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var pdfGen = new PdfGenerator();
                    pdfGen.GenerateInvoicePdf(_currentInvoice, dlg.FileName);

                    MessageBox.Show("PDF generado correctamente.", "Listo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generando PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnNuevoMaterial_Click(object sender, RoutedEventArgs e)
        {
            // 1. Pedir nombre
            string nombre = Microsoft.VisualBasic.Interaction.InputBox(
                "Introduce el nombre del material:",
                "Nuevo material",
                "");

            if (string.IsNullOrWhiteSpace(nombre))
                return;

            // 2. Pedir precio
            string strPrecio = Microsoft.VisualBasic.Interaction.InputBox(
                "Introduce el precio por unidad (€):",
                "Nuevo material",
                "0,00");

            if (!TryParseDecimalFlexible(strPrecio, out decimal precio) || precio < 0)
            {
                MessageBox.Show("El precio introducido no es válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 3. Pedir unidad de medida
            string unidad = Microsoft.VisualBasic.Interaction.InputBox(
                "Introduce la unidad (m³, m², kg, ud, etc.):",
                "Nuevo material",
                "ud");

            if (string.IsNullOrWhiteSpace(unidad))
                unidad = "ud";

            // 4. Crear nuevo ID (mock, ya que no hay BD real)
            int newId = _repo.Materials.Any()
                ? _repo.Materials.Max(m => m.Id) + 1
                : 1;

            // 5. Crear y añadir material
            var newMaterial = new Material
            {
                Id = newId,
                Name = nombre,
                UnitPrice = precio,
                Unit = unidad
            };

            _repo.Materials.Add(newMaterial);

            // 6. Refrescar DataGrid
            dgMateriales.Items.Refresh();

            MessageBox.Show("Material añadido correctamente.", "Listo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnNuevoCliente_Click(object sender, RoutedEventArgs e)
        {
            // 1. Pedir datos uno por uno
            string nombre = Microsoft.VisualBasic.Interaction.InputBox(
                "Introduce el nombre del cliente:",
                "Nuevo cliente",
                "");

            if (string.IsNullOrWhiteSpace(nombre))
                return;

            string direccion = Microsoft.VisualBasic.Interaction.InputBox(
                "Introduce la dirección del cliente:",
                "Nuevo cliente",
                "");

            if (string.IsNullOrWhiteSpace(direccion))
                direccion = "";

            string nif = Microsoft.VisualBasic.Interaction.InputBox(
                "Introduce el NIF del cliente:",
                "Nuevo cliente",
                "");

            if (string.IsNullOrWhiteSpace(nif))
                nif = "";

            string telefono = Microsoft.VisualBasic.Interaction.InputBox(
                "Introduce el teléfono del cliente:",
                "Nuevo cliente",
                "");

            if (string.IsNullOrWhiteSpace(telefono))
                telefono = "";

            string email = Microsoft.VisualBasic.Interaction.InputBox(
                "Introduce el email del cliente:",
                "Nuevo cliente",
                "");

            if (string.IsNullOrWhiteSpace(email))
                email = "";

            // 2. Crear nuevo ID
            int newId = _repo.Clients.Any()
                ? _repo.Clients.Max(c => c.Id) + 1
                : 1;

            // 3. Crear cliente
            var newClient = new Client
            {
                Id = newId,
                Name = nombre,
                Adress = direccion,
                NIF = nif,
                PhoneNumber = telefono,
                Email = email
            };

            // 4. Añadirlo al repositorio
            _repo.Clients.Add(newClient);

            // 5. Actualizar ComboBox
            cbClientes.Items.Refresh();
            cbClientes.SelectedItem = newClient;

            MessageBox.Show("Cliente creado correctamente.", "Listo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            _lineas.Clear();
            dgLineas.Items.Refresh();
            UpdateTotals();
        }

        // Si el usuario modifica cantidad directamente en DataGrid, recalcular
        private void dgLineas_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // Ejecutar UpdateTotals después de que WPF aplique el binding.
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    UpdateTotals();
                }), DispatcherPriority.Background);
            }
        }
    }
}
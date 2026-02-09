using FacturacionDobleEje.Models;
using FacturacionDobleEje.Models.FacturacionConstruccion.Models;
using FacturacionDobleEje.Repositories;
using FacturacionDobleEje.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace FacturacionDobleEje
{
    public partial class MainWindow : Window
    {
        private readonly MaterialRepository _materialRepo;
        private readonly ClientRepository _clientRepo;
        private readonly AppDbContext _appDbContext;
        private ObservableCollection<QuoteLine> _lineas;
        private Quote _currentQuote;

        public MainWindow()
        {
            _appDbContext = new AppDbContext();
            _materialRepo = new MaterialRepository(_appDbContext);
            _clientRepo = new ClientRepository(_appDbContext);
            _currentQuote = new Quote { Status = "Pendiente", Client = _clientRepo.GetAll().First() };
            _lineas = new ObservableCollection<QuoteLine>();

            InitializeComponent();
            dgMateriales.ItemsSource = _materialRepo.GetAll();
            cbClientes.ItemsSource = _clientRepo.GetAll();
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
                existing.Quantity += (int)cantidad;
            }
            else
            {
                _lineas.Add(new QuoteLine { Quote = _currentQuote, Material = mat, Quantity = (int)cantidad });
            }

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
            if (_currentQuote == null)
                return;

            if (!TryParseDecimalFlexible(txtIVA.Text, out var ivaPercent))
            {
                ivaPercent = 21m;
                txtIVA.Text = "21";
            }

            _currentQuote.VatType = ivaPercent / 100m;

            _currentQuote.Lines = _lineas;

            decimal subtotal = _currentQuote.Subtotal;
            decimal ivaTotal = Math.Round(subtotal * _currentQuote.VatType, 2, MidpointRounding.AwayFromZero);
            decimal total = subtotal + ivaTotal;

            if (tbSubtotal != null)
                tbSubtotal.Text = $"Subtotal: {subtotal:F2} €";

            if (tbIVA != null)
                tbIVA.Text = $"IVA ({ivaPercent:F2}%): {ivaTotal:F2} €";

            if (tbTotal != null)
                tbTotal.Text = $"Total: {total:F2} €";
        }

        private void TxtDiscount_Changed(object sender, TextChangedEventArgs e)
        {
            var txt = sender as TextBox;
            if (txt == null) return;

            string normalized = txt.Text.Replace('.', ',');
            txt.Text = normalized;
            txt.CaretIndex = txt.Text.Length;

            if (!TryParseDecimalFlexible(txt.Text, out var percent))
                percent = 0;

            foreach (var line in _lineas)
            {
                line.DiscountPercent = percent;
            }

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

            // 1) Intentar con la cultura invariante (usa '.')
            var normalized = input.Replace(',', '.');
            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
                return true;

            // 2) Intentar con la cultura actual (ej. es-ES usa ',')
            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out value))
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

            _currentQuote.Client = cliente;

            // Guardar como PDF
            var dlg = new SaveFileDialog
            {
                FileName = $"Presupuesto_{_currentQuote.Reference}.pdf",
                Filter = "PDF Files|*.pdf"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    //Generación del PDF de Presupuesto
                    var pdfGen = new PdfGenerator();
                    pdfGen.GenerateInvoicePdf(_currentQuote, dlg.FileName);

                    //Generación del PDF de Gastos/Beneficios
                    string internalPath = Path.Combine(Path.GetDirectoryName(dlg.FileName)!,$"Interno_{_currentQuote.Reference}.pdf");
                    pdfGen.GenerateInternalCostsPdf(_currentQuote, internalPath);

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

            if (string.IsNullOrWhiteSpace(nombre)) return;

            // 2. Pedir referencia
            string referencia = Microsoft.VisualBasic.Interaction.InputBox(
                "Introduce la referencia del material (Si la tiene):",
                "Nuevo material",
                "");

            if (string.IsNullOrWhiteSpace(referencia)) referencia = "";

            // 3. Pedir precio
            string strPrecio = Microsoft.VisualBasic.Interaction.InputBox(
                "Introduce el precio por unidad (€):",
                "Nuevo material",
                "0,00");
            if (string.IsNullOrWhiteSpace(strPrecio)) return;

            if (!TryParseDecimalFlexible(strPrecio, out decimal precio) || precio < 0)
            {
                MessageBox.Show("El precio introducido no es válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 4. Pedir unidad de medida
            string unidad = Microsoft.VisualBasic.Interaction.InputBox(
                "Introduce la unidad (m³, m², kg, ud, etc.):",
                "Nuevo material",
                "ud");

            if (string.IsNullOrWhiteSpace(unidad)) unidad = "ud";

            // 5. Crear y añadir material
            var newMaterial = new Material
            {
                Code = referencia,
                Name = nombre,
                UnitPrice = (decimal)precio,
                Unit = unidad
            };

            _materialRepo.Add(newMaterial);

            // 6. Refrescar DataGrid
            dgMateriales.ItemsSource = _materialRepo.GetAll();
            dgMateriales.Items.Refresh();

            MessageBox.Show("Material añadido correctamente.", "Listo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnNuevoCliente_Click(object sender, RoutedEventArgs e)
        {

            string nombre = Microsoft.VisualBasic.Interaction.InputBox(
                "Introduce el nombre del cliente:",
                "Nuevo cliente",
                "");
            if (string.IsNullOrWhiteSpace(nombre)) return;

            string direccion = Microsoft.VisualBasic.Interaction.InputBox(
                "Introduce la dirección del cliente:",
                "Nuevo cliente",
                "");
            if (string.IsNullOrWhiteSpace(direccion)) return;

            string docType = Microsoft.VisualBasic.Interaction.InputBox(
                "El cliente tiene CIF o NIF?:",
                "Nuevo cliente",
                "");
            if (string.IsNullOrWhiteSpace(docType)) docType = "CIF";

            string nif = Microsoft.VisualBasic.Interaction.InputBox(
                "Introduce el " + docType + " del cliente:",
                "Nuevo cliente",
                "");
            if (string.IsNullOrWhiteSpace(nif)) return;

            string telefono = Microsoft.VisualBasic.Interaction.InputBox(
                "Introduce el teléfono del cliente:",
                "Nuevo cliente",
                "");
            if (string.IsNullOrWhiteSpace(telefono)) return;

            string email = Microsoft.VisualBasic.Interaction.InputBox(
                "Introduce el email del cliente:",
                "Nuevo cliente",
                "");
            if (string.IsNullOrWhiteSpace(email)) return;


            var newClient = new Client
            {
                Name = nombre,
                Address = direccion,
                DocType = docType,
                Document = nif,
                PhoneNumber = telefono,
                Email = email
            };

            _clientRepo.Add(newClient);

            // Actualizar ComboBox
            cbClientes.ItemsSource = _clientRepo.GetAll();
            cbClientes.Items.Refresh();
            cbClientes.SelectedItem = newClient;

            MessageBox.Show("Cliente creado correctamente.", "Listo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            _lineas.Clear();
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

        private void dgMateriales_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    var material = e.Row.Item as Material;
                    if (material != null)
                    {
                        _materialRepo.Update(material);
                    }

                }), DispatcherPriority.Background);
            }
        }

        private void BtnGenerarPdfFactura_Click(object sender, RoutedEventArgs e)
        {
            var cliente = cbClientes.SelectedItem as Client;
            if (cliente == null)
            {
                MessageBox.Show("Selecciona un cliente.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_lineas.Any())
            {
                MessageBox.Show("Añade alguna línea a la factura.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Pedir número de factura al usuario
            string facturaRef = Microsoft.VisualBasic.Interaction.InputBox(
                "Introduce el número de factura:",
                "Número de factura",
                $"F-{DateTime.Now:yyyyMMddHHmmss}");

            if (string.IsNullOrWhiteSpace(facturaRef))
            {
                MessageBox.Show("Añade un número de referencia a la factura.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentQuote.Client = cliente;

            // Guardar como PDF
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Factura_{facturaRef}.pdf",
                Filter = "PDF Files|*.pdf"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    // Creamos copia temporal del presupuesto para no modificar la original
                    var facturaCopy = new Quote
                    {
                        Reference = facturaRef,          // Usamos la referencia de factura
                        Date = _currentQuote.Date,      // Mantenemos la fecha
                        Client = _currentQuote.Client,
                        Lines = _lineas,
                        Status = "Pendiente",
                        VatType = _currentQuote.VatType
                    };

                    //Generación del PDF de Factura
                    var pdfGen = new PdfGenerator();
                    pdfGen.GenerateInvoicePdf(facturaCopy, dlg.FileName, isFactura: true); // nuevo parámetro opcional

                    //Generación del PDF de Gastos/Beneficios
                    string internalPath = Path.Combine(Path.GetDirectoryName(dlg.FileName)!, $"Interno_{facturaRef}.pdf");
                    pdfGen.GenerateInternalCostsPdf(facturaCopy, internalPath);

                    MessageBox.Show("Factura generada correctamente.", "Listo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generando PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EliminarLinea_Click(object sender, RoutedEventArgs e)
        {
            if (dgLineas.SelectedItem is QuoteLine linea)
            {
                linea.Quote.Lines.Remove(linea);

                UpdateTotals();
            }
        }
    }
}
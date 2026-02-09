using FacturacionDobleEje.Models;
using FacturacionDobleEje.Repositories;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;
using System.IO;

namespace FacturacionDobleEje.Services
{
    public class PdfGenerator
    {
        private readonly CompanyRepository _companyRepository;

        public PdfGenerator()
        {
            _companyRepository = new CompanyRepository();
        }

        public void GenerateInvoicePdf(Quote quote, string outputPath, bool isFactura = false)
        {
            var empresa = _companyRepository.GetAll().First();

            var doc = new Document();
            string tipoDoc = isFactura ? "Factura" : "Presupuesto";
            doc.Info.Title = $"{tipoDoc} Nº: {quote.Reference}";
            doc.Info.Author = quote.Client.Name;

            // Estilos generales
            var style = doc.Styles["Normal"];
            style?.Font.Name = "Arial";
            style?.Font.Size = 12;

            var section = doc.AddSection();
            if (!string.IsNullOrEmpty(empresa.WatermarkPath) && File.Exists(empresa.WatermarkPath))
            {
                var watermark = section.Headers.Primary.AddImage(empresa.WatermarkPath);

                watermark.RelativeVertical = RelativeVertical.Page;
                watermark.RelativeHorizontal = RelativeHorizontal.Page;

                watermark.Top = ShapePosition.Center;
                watermark.Left = ShapePosition.Center;

                watermark.Width = Unit.FromCentimeter(15);
                watermark.LockAspectRatio = true;

                watermark.WrapFormat.Style = WrapStyle.None;
            }
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);

            // Cabecera empresa

            if (empresa.LogoPath != null)
            {
                var image = section.AddImage(empresa.LogoPath);
                image.Width = Unit.FromCentimeter(3);
                image.LockAspectRatio = true;
                image.Left = ShapePosition.Left;
            }

            var header = section.AddParagraph();
            header.Format.SpaceAfter = Unit.FromCentimeter(0.5);
            header.Format.Font.Size = 14;
            header.AddLineBreak();
            header.AddLineBreak();
            header.AddFormattedText(empresa.Name, TextFormat.Bold);
            header.AddLineBreak();
            header.AddText(empresa.Address);
            header.AddLineBreak();
            header.AddText(empresa.DocType + ": " + empresa.Document);
            header.AddLineBreak();
            header.AddText($"Tel: {empresa.PhoneNumber} - {empresa.Email}");
            header.AddLineBreak();
            header.AddText($"Número de cuenta: " + empresa.Account);

            // Info cliente y factura
            var tableInfo = section.AddTable();
            tableInfo.Format.SpaceAfter = Unit.FromCentimeter(0.5);
            tableInfo.Borders.Visible = false;
            tableInfo.AddColumn(Unit.FromCentimeter(9));
            tableInfo.AddColumn(Unit.FromCentimeter(7));
            var row = tableInfo.AddRow();

            var pLeft = row.Cells[0].AddParagraph();
            pLeft.Format.Font.Size = 14;
            pLeft.AddFormattedText("Cliente:", TextFormat.Bold);
            pLeft.AddLineBreak();
            pLeft.AddText(quote.Client.Name);
            pLeft.AddLineBreak();
            pLeft.AddText(quote.Client.Address);
            pLeft.AddLineBreak();
            pLeft.AddText(quote.Client.DocType + ": " + quote.Client.Document);

            var pRight = row.Cells[1].AddParagraph();
            pRight.Format.Alignment = ParagraphAlignment.Right;
            pRight.Format.Font.Size = 14;
            pRight.AddText($"Nº {tipoDoc}: ");
            pRight.AddLineBreak();
            pRight.AddText(quote.Reference);
            pRight.AddLineBreak();
            pRight.AddText("Fecha: " + quote.Date.ToString("dd/MM/yyyy"));
            section.AddParagraph().AddLineBreak();

            bool showDiscountColumn = quote.Lines.Any(l => l.DiscountAmount > 0);

            var table = section.AddTable();
            table.Borders.Width = 0;

            if (showDiscountColumn)
            {
                table.AddColumn(Unit.FromCentimeter(3.5)); // Nombre
                table.AddColumn(Unit.FromCentimeter(4.25));   // Descripción
                table.AddColumn(Unit.FromCentimeter(2));   // Cantidad
                table.AddColumn(Unit.FromCentimeter(2.25)); // Precio unitario
                table.AddColumn(Unit.FromCentimeter(3.5)); // Descuento
                table.AddColumn(Unit.FromCentimeter(2.5)); // Importe
            }
            else
            {
                table.AddColumn(Unit.FromCentimeter(3.5));
                table.AddColumn(Unit.FromCentimeter(5.5));
                table.AddColumn(Unit.FromCentimeter(2.5));
                table.AddColumn(Unit.FromCentimeter(3));
                table.AddColumn(Unit.FromCentimeter(3.5));
            }

            var hdr = table.AddRow();
            hdr.Shading.Color = Colors.LightGray;
            hdr.TopPadding = 3;
            hdr.BottomPadding = 3;
            hdr.Borders.Top.Width = 1;
            hdr.Borders.Bottom.Width = 1;

            hdr.Cells[0].AddParagraph("Material");
            hdr.Cells[1].AddParagraph("Descripción");
            hdr.Cells[2].AddParagraph("Cant.");
            hdr.Cells[3].AddParagraph("P. unit (€)");

            int colIndex = 4;

            if (showDiscountColumn)
            {
                hdr.Cells[colIndex].AddParagraph("Descuento (%)");
                colIndex++;
            }

            hdr.Cells[colIndex].AddParagraph("Importe (€)");

            foreach (var line in quote.Lines)
            {
                var r = table.AddRow();

                r.Cells[0].AddParagraph(line.Material.Name);
                r.Cells[1].AddParagraph(line.Material.Description ?? "");
                r.Cells[2].AddParagraph(line.Quantity.ToString("0.##"));
                var pUnit = r.Cells[3].AddParagraph(line.SaleUnitPrice.ToString("0.00"));
                pUnit.Format.Alignment = ParagraphAlignment.Right;

                int c = 4;

                if (showDiscountColumn)
                {
                    var pDiscount = r.Cells[c].AddParagraph(line.DiscountPercent != 0 ? line.DiscountPercent.ToString() : "");
                    pDiscount.Format.Alignment = ParagraphAlignment.Center;
                    c++;
                }

                // importe neto
                var pImporte = r.Cells[c].AddParagraph(line.Amount.ToString("0.00"));
                pImporte.Format.Alignment = ParagraphAlignment.Right;

                r.Borders.Bottom.Width = 0.5;
                r.Borders.Bottom.Style = BorderStyle.DashSmallGap;
                r.Borders.Bottom.Color = Colors.Gray;
            }


            // Totales
            section.AddParagraph().AddLineBreak();
            section.AddParagraph().Format.SpaceBefore = Unit.FromCentimeter(0.7);
            var totTable = section.AddTable();
            totTable.AddColumn(Unit.FromCentimeter(11));
            totTable.AddColumn(Unit.FromCentimeter(5));

            var rSub = totTable.AddRow();
            rSub.Cells[0].AddParagraph("").Format.Alignment = ParagraphAlignment.Right;
            rSub.Cells[1].AddParagraph("Subtotal: " + quote.Subtotal.ToString("0.00") + " €");

            var rIva = totTable.AddRow();
            rIva.Cells[0].AddParagraph("").Format.Alignment = ParagraphAlignment.Right;
            rIva.Cells[1].AddParagraph($"IVA ({quote.VatType * 100:0}%): {quote.Vat:0.00} €");

            var rTotal = totTable.AddRow();
            rTotal.Cells[0].AddParagraph("").Format.Alignment = ParagraphAlignment.Right;
            rTotal.Cells[1].AddParagraph("Total: " + quote.Total.ToString("0.00") + " €");
            rTotal.Cells[1].Format.Font.Bold = true;

            rSub.Cells[1].Format.Font.Size = 12;
            rIva.Cells[1].Format.Font.Size = 12;
            rTotal.Cells[1].Format.Font.Size = 13;

            // Renderizar y guardar
            var renderer = new PdfDocumentRenderer() 
            { 
                Document = doc 
            }; 
            renderer.RenderDocument();
            renderer.PdfDocument.Save(outputPath);
        }

        public void GenerateInternalCostsPdf(Quote quote, string outputPath)
        {
            var empresa = _companyRepository.GetAll().First();

            var doc = new Document();
            doc.Info.Title = $"Hoja interna - {quote.Reference}";
            doc.Info.Author = empresa.Name;

            var style = doc.Styles["Normal"];
            style.Font.Name = "Arial";
            style.Font.Size = 11;

            var section = doc.AddSection();

            // Título
            var title = section.AddParagraph($"HOJA INTERNA DE GASTOS Y BENEFICIOS");
            title.Format.Font.Size = 16;
            title.Format.Font.Bold = true;
            title.Format.SpaceAfter = Unit.FromCentimeter(0.5);

            section.AddParagraph($"Documento: {quote.Reference}");
            section.AddParagraph($"Fecha: {quote.Date:dd/MM/yyyy}");
            section.AddParagraph($"Cliente: {quote.Client.Name}");
            section.AddParagraph().AddLineBreak();

            // ---- TABLA ----

            var table = section.AddTable();
            table.Borders.Width = 0.5;

            table.AddColumn(Unit.FromCentimeter(9));
            table.AddColumn(Unit.FromCentimeter(4));
            table.AddColumn(Unit.FromCentimeter(4));

            // Header
            var hdr = table.AddRow();
            hdr.Shading.Color = Colors.LightGray;

            hdr.Cells[0].AddParagraph("Material");
            hdr.Cells[1].AddParagraph("Gastos (€)").Format.Alignment = ParagraphAlignment.Right;
            hdr.Cells[2].AddParagraph("Beneficio (€)").Format.Alignment = ParagraphAlignment.Right;

            decimal totalCost = 0;
            decimal totalProfit = 0;

            foreach (var line in quote.Lines)
            {
                decimal cost = line.Quantity * line.UnitPrice;
                decimal profit = (line.Quantity * (line.SaleUnitPrice - line.UnitPrice)) - line.DiscountAmount;

                totalCost += cost;
                totalProfit += profit;

                var row = table.AddRow();

                row.Cells[0].AddParagraph(line.Material.Name);

                var pCost = row.Cells[1].AddParagraph(cost.ToString("0.00"));
                pCost.Format.Alignment = ParagraphAlignment.Right;

                var pProfit = row.Cells[2].AddParagraph(profit.ToString("0.00"));
                pProfit.Format.Alignment = ParagraphAlignment.Right;
            }

            // ---- FILA TOTAL ----

            var totalRow = table.AddRow();
            totalRow.Format.Font.Bold = true;
            totalRow.Borders.Top.Width = 1;

            totalRow.Cells[0].AddParagraph("TOTAL");

            totalRow.Cells[1].AddParagraph(totalCost.ToString("0.00"))
                .Format.Alignment = ParagraphAlignment.Right;

            decimal percent = totalCost == 0 ? 0 : (totalProfit / totalCost) * 100m;

            totalRow.Cells[2].AddParagraph($"{totalProfit:0.00} ({percent:0.##}%)")
                .Format.Alignment = ParagraphAlignment.Right;

            // Render
            var renderer = new PdfDocumentRenderer() { Document = doc };
            renderer.RenderDocument();
            renderer.PdfDocument.Save(outputPath);
        }
    }
}

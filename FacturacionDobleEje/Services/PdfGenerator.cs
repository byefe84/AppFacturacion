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

        public void GenerateInvoicePdf(Quote quote, string outputPath)
        {
            var doc = new Document();
            doc.Info.Title = $"Presupuesto Nº: " + quote.Reference;
            doc.Info.Author = quote.Client.Name;

            // Estilos generales
            var style = doc.Styles["Normal"];
            style?.Font.Name = "Arial";
            style?.Font.Size = 12;

            var section = doc.AddSection();
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);

            // Cabecera empresa
            var empresa = _companyRepository.GetAll().First();

            if (empresa.LogoPath != null)
            {
                // Combinar la ruta base de la app con la ruta relativa del logo
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, empresa.LogoPath);

                var image = section.AddImage(logoPath);
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
            pRight.AddText("Nº Presupuesto: ");
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
                table.AddColumn(Unit.FromCentimeter(6));  // descripción
                table.AddColumn(Unit.FromCentimeter(2));  // cantidad
                table.AddColumn(Unit.FromCentimeter(3));  // p. unit
                table.AddColumn(Unit.FromCentimeter(3));  // descuento
                table.AddColumn(Unit.FromCentimeter(3));  // importe
            }
            else
            {
                table.AddColumn(Unit.FromCentimeter(8));    // descripción más ancha
                table.AddColumn(Unit.FromCentimeter(2.5));  // cantidad
                table.AddColumn(Unit.FromCentimeter(3.5));  // p. unit
                table.AddColumn(Unit.FromCentimeter(3));    // importe
            }

            var hdr = table.AddRow();
            hdr.Shading.Color = Colors.LightGray;
            hdr.TopPadding = 3;
            hdr.BottomPadding = 3;
            hdr.Borders.Top.Width = 1;
            hdr.Borders.Bottom.Width = 1;

            hdr.Cells[0].AddParagraph("Descripción");
            hdr.Cells[1].AddParagraph("Cant.");
            hdr.Cells[2].AddParagraph("P. unit (€)");

            int colIndex = 3;

            if (showDiscountColumn)
            {
                hdr.Cells[colIndex].AddParagraph("Descuento (€)");
                colIndex++;
            }

            hdr.Cells[colIndex].AddParagraph("Importe (€)");

            foreach (var line in quote.Lines)
            {
                var r = table.AddRow();

                r.Cells[0].AddParagraph(line.Material.Name);
                r.Cells[1].AddParagraph(line.Quantity.ToString("0.##"));
                r.Cells[2].AddParagraph(line.UnitPrice.ToString("0.00"));

                int c = 3;

                if (showDiscountColumn)
                {
                    r.Cells[c].AddParagraph(line.DiscountAmount.ToString("0.00"));
                    c++;
                }

                // importe neto
                r.Cells[c].AddParagraph((line.Amount - line.DiscountAmount).ToString("0.00"));

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
    }
}

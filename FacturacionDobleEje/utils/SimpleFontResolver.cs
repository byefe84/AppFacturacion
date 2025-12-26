using PdfSharp.Fonts;
using System.IO;

namespace FacturacionDobleEje.utils
{
    
    public class SimpleFontResolver : IFontResolver
    {
        public byte[] GetFont(string faceName)
        {
            // Elegimos una fuente que SI existe en todos los Windows
            return File.ReadAllBytes(@"C:\Windows\Fonts\arial.ttf");
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool bold, bool italic)
        {
            // Todas las fuentes pedidas se resuelven como Arial
            return new FontResolverInfo("Arial");
        }
    }
}

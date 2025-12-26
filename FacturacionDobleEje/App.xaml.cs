using FacturacionDobleEje.utils;
using System.Windows;

namespace FacturacionDobleEje
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            PdfSharp.Fonts.GlobalFontSettings.FontResolver = new SimpleFontResolver();
        }
    }

}

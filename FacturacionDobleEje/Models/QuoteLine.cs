using FacturacionDobleEje.Models.FacturacionConstruccion.Models;

namespace FacturacionDobleEje.Models
{
    public class QuoteLine : ObservableObject
    {
        public long Id { get; set; }
        public required Quote Quote { get; set; }
        public required Material Material { get; set; }
        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (SetField(ref _quantity, value))
                {
                    OnPropertyChanged(nameof(GrossAmount));
                    OnPropertyChanged(nameof(DiscountAmount));
                    OnPropertyChanged(nameof(Amount));
                }
            }
        }
        public decimal UnitPrice => Material.UnitPrice;
        private decimal _discountPercent;
        public decimal DiscountPercent
        {
            get => _discountPercent;
            set
            {
                if (SetField(ref _discountPercent, value))
                {
                    OnPropertyChanged(nameof(DiscountAmount));
                    OnPropertyChanged(nameof(Amount));
                }
            }
        }
        public decimal GrossAmount => Quantity * UnitPrice;
        public decimal DiscountAmount => Math.Round(GrossAmount * (DiscountPercent / 100m), 2, MidpointRounding.AwayFromZero);
        public decimal Amount => GrossAmount - DiscountAmount;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}

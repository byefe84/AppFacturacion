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
                    Recalculate();
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
                    Recalculate();
            }
        }

        private decimal _profitPercent;
        public decimal ProfitPercent
        {
            get => _profitPercent;
            set
            {
                if (SetField(ref _profitPercent, value))
                    Recalculate();
            }
        }

        public decimal SaleUnitPrice => UnitPrice * (1 + ProfitPercent / 100m);
        public decimal GrossAmount => Quantity * SaleUnitPrice;
        public decimal DiscountAmount => GrossAmount * (DiscountPercent / 100m);
        public decimal Amount => GrossAmount - DiscountAmount;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        private void Recalculate()
        {
            OnPropertyChanged(nameof(SaleUnitPrice));
            OnPropertyChanged(nameof(GrossAmount));
            OnPropertyChanged(nameof(DiscountAmount));
            OnPropertyChanged(nameof(Amount));
        }
    }
}

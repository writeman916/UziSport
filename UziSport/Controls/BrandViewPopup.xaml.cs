using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UziSport.DAL;
using UziSport.Model;

namespace UziSport.Controls
{
    public partial class BrandViewPopup : ContentView
    {
        public event EventHandler? Saved;
        public event EventHandler? Canceled;

        public ObservableCollection<BrandInfo> Brands { get; } = new();

        private List<BrandInfo> _deletedBrands { get; } = new();

        public BrandViewPopup()
        {
            InitializeComponent();
            BindingContext = this;

            IsVisible = false;
            Opacity = 0;
        }

        public async Task ShowAsync()
        {
            IsVisible = true;
            Opacity = 0;

            // Load Brand
            var brands = await BrandDAL.Instance.GetBrandsAsync();
            Brands.Clear();
            foreach (var b in brands)
                Brands.Add(b);


            await this.FadeTo(1, 150);
        }

        public async Task HideAsync()
        {
            await this.FadeTo(0, 150);
            IsVisible = false;
        }

        private async void CancelButton_Clicked(object sender, EventArgs e)
        {
            Canceled?.Invoke(this, EventArgs.Empty);
            await HideAsync();
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            var dal = new BrandDAL();

            foreach (var brand in _deletedBrands)
            {
                await dal.DeleteItemAsync(brand);
            }

            foreach (var brand in Brands)
            {
                await dal.SaveItemAsync(brand);
            }

            Saved?.Invoke(this, EventArgs.Empty);
            await HideAsync();
        }

        private void DeleteBrandButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is BrandInfo brand)
            {
                // Nếu Brands là ObservableCollection<BrandInfo> trong BindingContext
                if (BindingContext is not null)
                {
                    var brandsProp = BindingContext
                        .GetType()
                        .GetProperty("Brands");

                    if (brandsProp?.GetValue(BindingContext) is ICollection<BrandInfo> brands)
                    {
                        _deletedBrands.Add(brand);
                        brands.Remove(brand);
                    }
                }
            }
        }

        private void BrandNameEntry_Completed(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BrandNameEntry.Text))
                return;

            if (Brands.Any(X => X.BrandName.Equals(BrandNameEntry.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                this.BrandExistsLabel.IsVisible = true;
                return;
            }

            Brands.Add(new BrandInfo
            {
                BrandName = BrandNameEntry.Text.Trim(),
            });

            BrandNameEntry.Text = string.Empty;
            this.BrandExistsLabel.IsVisible = false;
        }
    }
}

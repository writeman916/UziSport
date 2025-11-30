using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UziSport.DAL;
using UziSport.Model;

namespace UziSport.Controls
{
    public partial class SupplierViewPopup : ContentView
    {
        public event EventHandler? Saved;
        public event EventHandler? Canceled;

        public ObservableCollection<SupplierInfo> Suppliers { get; } = new();

        private List<SupplierInfo> _deletedSuppliers { get; } = new();

        public SupplierViewPopup()
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

            // Load Supplier
            var suppliers = await SupplierDAL.Instance.GetSuppliersAsync();
            Suppliers.Clear();
            foreach (var b in suppliers)
                Suppliers.Add(b);


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
            var dal = new SupplierDAL();

            foreach (var supplier in _deletedSuppliers)
            {
                await dal.DeleteItemAsync(supplier);
            }

            foreach (var supplier in Suppliers)
            {
                await dal.SaveItemAsync(supplier);
            }

            Saved?.Invoke(this, EventArgs.Empty);
            await HideAsync();
        }

        private void DeleteSupplierButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is SupplierInfo supplier)
            {
                // Nếu Suppliers là ObservableCollection<SupplierInfo> trong BindingContext
                if (BindingContext is not null)
                {
                    var suppliersProp = BindingContext
                        .GetType()
                        .GetProperty("Suppliers");

                    if (suppliersProp?.GetValue(BindingContext) is ICollection<SupplierInfo> suppliers)
                    {
                        _deletedSuppliers.Add(supplier);
                        suppliers.Remove(supplier);
                    }
                }
            }
        }

        private void SupplierNameEntry_Completed(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SupplierNameEntry.Text))
                return;

            if (Suppliers.Any(X => X.SupplierName.Equals(SupplierNameEntry.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                this.SupplierExistsLabel.IsVisible = true;
                return;
            }

            Suppliers.Add(new SupplierInfo
            {
                SupplierName = SupplierNameEntry.Text.Trim(),
            });

            SupplierNameEntry.Text = string.Empty;
            this.SupplierExistsLabel.IsVisible = false;
        }
    }
}

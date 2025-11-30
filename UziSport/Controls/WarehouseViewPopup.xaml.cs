using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UziSport.DAL;
using UziSport.Model;

namespace UziSport.Controls
{
    public partial class WarehouseViewPopup : ContentView
    {
        public event EventHandler? Saved;
        public event EventHandler? Canceled;

        public ObservableCollection<WarehouseInfo> Warehouses { get; } = new();

        private List<WarehouseInfo> _deletedWarehouses { get; } = new();

        public WarehouseViewPopup()
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

            // Load Warehouse
            var warehouses = await WarehouseDAL.Instance.GetWarehousesAsync();
            Warehouses.Clear();
            foreach (var b in warehouses)
                Warehouses.Add(b);


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
            var dal = new WarehouseDAL();

            foreach (var warehouse in _deletedWarehouses)
            {
                await dal.DeleteItemAsync(warehouse);
            }

            foreach (var warehouse in Warehouses)
            {
                await dal.SaveItemAsync(warehouse);
            }

            Saved?.Invoke(this, EventArgs.Empty);
            await HideAsync();
        }

        private void DeleteWarehouseButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is WarehouseInfo warehouse)
            {
                // Nếu Warehouses là ObservableCollection<WarehouseInfo> trong BindingContext
                if (BindingContext is not null)
                {
                    var warehousesProp = BindingContext
                        .GetType()
                        .GetProperty("Warehouses");

                    if (warehousesProp?.GetValue(BindingContext) is ICollection<WarehouseInfo> warehouses)
                    {
                        _deletedWarehouses.Add(warehouse);
                        warehouses.Remove(warehouse);
                    }
                }
            }
        }

        private void WarehouseNameEntry_Completed(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(WarehouseNameEntry.Text))
                return;

            if (Warehouses.Any(X => X.WarehouseName.Equals(WarehouseNameEntry.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                this.WarehouseExistsLabel.IsVisible = true;
                return;
            }

            Warehouses.Add(new WarehouseInfo
            {
                WarehouseName = WarehouseNameEntry.Text.Trim(),
            });

            WarehouseNameEntry.Text = string.Empty;
            this.WarehouseExistsLabel.IsVisible = false;
        }
    }
}

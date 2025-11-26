using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UziSport.DAL;
using UziSport.Model;

namespace UziSport.Controls;

public partial class CatalogViewPopup : ContentView
{
    public event EventHandler? Saved;
    public event EventHandler? Canceled;

    public ObservableCollection<CatalogInfo> Catalogs { get; } = new();

    public List<CatalogInfo> DeletedCatalogs { get; } = new();

    public CatalogViewPopup()
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

        // Load Catalog
        var catalogs = await CatalogDAL.Instance.GetCatalogsAsync();
        Catalogs.Clear();
        foreach (var b in catalogs)
            Catalogs.Add(b);


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
        var dal = new CatalogDAL();

        foreach (var Catalog in DeletedCatalogs)
        {
            await dal.DeleteItemAsync(Catalog);
        }

        foreach (var Catalog in Catalogs.Where(x => x.CatalogId == 0))
        {
            await dal.SaveItemAsync(Catalog);
        }

        Saved?.Invoke(this, EventArgs.Empty);
        await HideAsync();
    }

    private void DeleteCatalogButton_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is CatalogInfo Catalog)
        {
            // Nếu Catalogs là ObservableCollection<CatalogInfo> trong BindingContext
            if (BindingContext is not null)
            {
                var CatalogsProp = BindingContext
                    .GetType()
                    .GetProperty("Catalogs");

                if (CatalogsProp?.GetValue(BindingContext) is ICollection<CatalogInfo> Catalogs)
                {
                    DeletedCatalogs.Add(Catalog);
                    Catalogs.Remove(Catalog);
                }
            }
        }
    }

    private void CatalogNameEntry_Completed(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CatalogNameEntry.Text))
            return;

        if (Catalogs.Any(x => x.CatalogName.Equals(CatalogNameEntry.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            this.CatalogExistsLabel.IsVisible = true;
            return;
        }

        Catalogs.Add(new CatalogInfo
        {
            CatalogName = CatalogNameEntry.Text.Trim(),
        });

        CatalogNameEntry.Text = string.Empty;
        this.CatalogExistsLabel.IsVisible = false;

    }
}
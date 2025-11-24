using System.Collections.ObjectModel;
using UziSport.DAL;
using UziSport.Model;

namespace UziSport;

public partial class ProductManage : ContentPage
{
    public ObservableCollection<ProductComboCostInfo> ComboCosts { get; } = new();

    public int NextNo => ComboCosts.Count == 0 ? 1 : ComboCosts.Count + 1;

    private int _productId = 0;

    public ProductManage()
    {
        this.InitializeComponent();
        BindingContext = this;
    }

    private void AddButton_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NewCostEntry.Text))
            return;

        if (!decimal.TryParse(NewCostEntry.Text, out var value))
            return;

        ComboCosts.Add(new ProductComboCostInfo
        {
            ProductId = _productId,
            Cost = value,
            CreateAt = DateTime.Now,
            CreateBy = "admin"
        });

        NewCostEntry.Text = string.Empty;
    }

    private void DeleteButton_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is ProductComboCostInfo item)
        {
            ComboCosts.Remove(item);
        }
    }

    private void NewCostEntry_Completed(object sender, EventArgs e)
    {
        AddButton_Clicked(null, e);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var catalogs = await CatalogDAL.Instance.GetCatalogsAsync();
        CatalogPicker.ItemsSource = catalogs.Select(x => x.CatalogName).ToList();

        var brands = await BrandDAL.Instance.GetBrandsAsync();
        BrandPicker.ItemsSource =  brands.Select(x => x.BrandName).ToList();

    }
}
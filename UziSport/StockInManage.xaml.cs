using System.Collections.ObjectModel;
using UziSport.DAL;
using UziSport.Model;

namespace UziSport;

public partial class StockInManage : ContentPage
{
    private ObservableCollection<ProductViewInfo> _products;

    public StockInManage()
	{
		InitializeComponent();
	}

    private async void BtnTao_Clicked(object sender, EventArgs e)
    {
		await this.NewStockInPopup.ShowAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_products == null)
        {
            await LoadProductsAsync();
        }
    }

    private async Task LoadProductsAsync()
    {
        // L?y list s?n ph?m 1 l?n ? ?ây
        var productDal = new ProductDAL();
        var list = await productDal.GetProductsAsync();

        _products = new ObservableCollection<ProductViewInfo>(list);

        // Gán cho popup, t? gi? popup xài l?i list này
        NewStockInPopup.Products = _products;
    }
}
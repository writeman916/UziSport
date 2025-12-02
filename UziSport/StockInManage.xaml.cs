using System.Collections.ObjectModel;
using UziSport.Controls;
using UziSport.DAL;
using UziSport.Model;

namespace UziSport;

public partial class StockInManage : ContentPage
{
    private ObservableCollection<ProductViewInfo> _products;

    private List<StockInViewInfo> _stockInInfos = new();

    public List<StockInViewInfo> StockInInfos
    {
        get => _stockInInfos;
        set
        {
            if (_stockInInfos != value)
            {
                _stockInInfos = value;
                OnPropertyChanged(nameof(StockInInfos));
            }
        }
    }

    private StockInDAL _stockInDal = new StockInDAL();

    public StockInManage()
	{
		InitializeComponent();
        this.BindingContext = this;
    }

    private async void BtnTao_Clicked(object sender, EventArgs e)
    {
		await this.NewStockInPopup.ShowAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            if (_products == null)
            {
                await LoadProductsAsync();
            }

            StockInInfos = await _stockInDal.GetAllStockInAsync();
        }
        catch (Exception)
        {

            throw;
        }
    }

    private async Task LoadProductsAsync()
    {
        var productDal = new ProductDAL();
        var list = await productDal.GetProductsAsync();

        _products = new ObservableCollection<ProductViewInfo>(list);

        NewStockInPopup.Products = _products;
    }

    private async void StockInItem_DoubleTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Layout layout || layout.BindingContext is not StockInViewInfo item)
            return;

        var detailDal = new StockInDetailDAL();
        var details = await detailDal.GetDetailByStockInIdAsync(item.StockInId);

        NewStockInPopup.StockInDetailInfos.Clear();

        foreach (var d in details)
            NewStockInPopup.StockInDetailInfos.Add(d);

        NewStockInPopup.CurrentStockInInfo = item;

        await NewStockInPopup.ShowAsync();
    }

    private async void NewStockInPopup_Closed(object sender, EventArgs e)
    {
        if( sender is NewStockInPopup popup && popup.Result == NewStockInPopup.StockInPopupResults.Saved)
        {
            StockInInfos = await _stockInDal.GetAllStockInAsync();
        }
    }
}
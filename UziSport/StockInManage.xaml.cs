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

        if (_products == null)
        {
            await LoadProductsAsync();
        }

        StockInInfos = await _stockInDal.GetAllStockInAsync();
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

    private async void StockInItem_DoubleTapped(object sender, TappedEventArgs e)
    {
        // Lấy StockInViewInfo tương ứng dòng được double click
        if (sender is not Layout layout || layout.BindingContext is not StockInViewInfo item)
            return;

        // Load chi tiết nhập kho
        var detailDal = new StockInDetailDAL();
        var details = await detailDal.GetDetailByStockInIdAsync(item.StockInId);

        // Đổ dữ liệu vào popup
        NewStockInPopup.StockInDetailInfos.Clear();
        foreach (var d in details)
            NewStockInPopup.StockInDetailInfos.Add(d);

        // Gán header hiện tại cho popup
        NewStockInPopup.CurrentStockInInfo = item;

        // Hiển thị popup
        await NewStockInPopup.ShowAsync();
    }

    private async void NewStockInPopup_Closed(object sender, EventArgs e)
    {
        if( sender is NewStockInPopup popup && popup.Result == NewStockInPopup.StockInPopupResults.Saved)
        {
            await LoadProductsAsync();

            StockInInfos = await _stockInDal.GetAllStockInAsync();
        }
    }
}
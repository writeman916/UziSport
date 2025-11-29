using System.Collections.ObjectModel;
using UziSport.DAL;
using UziSport.Model;

namespace UziSport.Controls;

public partial class NewStockInPopup : ContentView
{
    public static readonly BindableProperty ProductsProperty =
            BindableProperty.Create(
                nameof(Products),
                typeof(ObservableCollection<ProductViewInfo>),
                typeof(NewStockInPopup),
                defaultValue: null);

    public ObservableCollection<ProductViewInfo> Products
    {
        get => (ObservableCollection<ProductViewInfo>)GetValue(ProductsProperty);
        set => SetValue(ProductsProperty, value);
    }

    public ImportStatus ImportStatus { get; set; } = ImportStatus.InProgress;
    public ObservableCollection<WarehouseInfo> Warehouses { get; } = new();
    public ObservableCollection<SupplierInfo> Suppliers { get; } = new();
    public StockInViewInfo CurrentStockInInfo { get; set; } = new StockInViewInfo();
    public string SearchString { get; }


    private List<ProductViewInfo> _searchResults = new();
    public List<ProductViewInfo> SearchResults
    {
        get => _searchResults;
        set
        {
            if (_searchResults != value)
            {
                _searchResults = value;
                OnPropertyChanged(nameof(SearchResults));
            }
        }
    }

    private ObservableCollection<StockInDetailViewInfo> _stockInDetailInfos = new();
    public ObservableCollection<StockInDetailViewInfo> StockInDetailInfos
    {
        get => _stockInDetailInfos;
        set
        {
            if (_stockInDetailInfos != value)
            {
                _stockInDetailInfos = value;
                OnPropertyChanged(nameof(StockInDetailInfos));
            }
        }
    }

    private StockInDetailViewInfo? _pendingFocusItem;
    private List<StockInDetailViewInfo> _deletedStockInDetailInfos = new();

    public NewStockInPopup()
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

        // Load Warehouses
        var warehouses = await WarehouseDAL.Instance.GetWarehousesAsync();
        Warehouses.Clear();
        foreach (var c in warehouses)
            Warehouses.Add(c);

        // Load Suppliers
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

    private void SearchEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (Products == null)
            return;

        var text = e.NewTextValue;

        if (string.IsNullOrWhiteSpace(text))
        {
            SearchResults = Products.ToList();
            return;
        }

        text = text.Trim();

        var filtered = Products.Where(p =>
               (!string.IsNullOrEmpty(p.ProductCode) && p.ProductCode.Contains(text, StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrEmpty(p.ProductName) && p.ProductName.Contains(text, StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrEmpty(p.CatalogName) && p.CatalogName.Contains(text, StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrEmpty(p.BrandName) && p.BrandName.Contains(text, StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrEmpty(p.Specification) && p.Specification.Contains(text, StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrEmpty(p.Note) && p.Note.Contains(text, StringComparison.OrdinalIgnoreCase))
        ).ToList();

        SearchResults = filtered;
    }

    private void SearchResultButton_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is ProductViewInfo item)
        {
            // Xử lý chọn item ở đây
            // Ví dụ:
            // ViewModel.SelectedSearchResult = item;
        }
    }
    private void BtnThem_Clicked(object sender, EventArgs e)
    {
        if(SearchResults.Count == 0)
            return;

        var firstItem = SearchResults[0];

        var newDetail = new StockInDetailViewInfo()
        {
            ProductId = firstItem.ProductId,
            ProductName = firstItem.ProductName,
            ProductCode = firstItem.ProductCode,
            Specification = firstItem.Specification,
            BrandName = firstItem.BrandName,
            CatalogName = firstItem.CatalogName,
            UnitCost = firstItem.Cost,
            CreateAt = DateTime.Now,
            CreateBy = Environment.UserName
        };

        StockInDetailInfos.Add(newDetail);

        // Ghi nhớ item vừa thêm để lát nữa focus vào ô Quantity của nó
        _pendingFocusItem = newDetail;
    }

    private void QuantityEntry_Loaded(object sender, EventArgs e)
    {
        if (sender is not Entry entry)
            return;

#if WINDOWS
        // Gắn handler bắt Ctrl+N cho TextBox nền của Entry (Windows)
        if (entry.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.TextBox textBox)
        {
            textBox.KeyDown -= DetailTextBox_KeyDown;
            textBox.KeyDown += DetailTextBox_KeyDown;
        }
#endif

        if (_pendingFocusItem != null && entry.BindingContext == _pendingFocusItem)
        {
            _pendingFocusItem = null; // chỉ focus một lần cho item vừa thêm
            entry.Focus();
        }
    }

#if WINDOWS
    // Bắt Ctrl + N trên Windows khi đang ở ô Số lượng
    private void DetailTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.N)
        {
            var ctrlState = Microsoft.UI.Input.InputKeyboardSource
                .GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);

            bool isCtrlDown = ctrlState.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

            if (isCtrlDown)
            {
                e.Handled = true;
                SearchEntry?.Focus();
            }
        }
    }
#endif

    private void DeleteStockInDetailButton_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is StockInDetailViewInfo stockInDetail)
        {
            if (BindingContext is not null)
            {
                var StockInDetailInfosProp = BindingContext
                    .GetType()
                    .GetProperty("StockInDetailInfos");

                if (StockInDetailInfosProp?.GetValue(BindingContext) is ICollection<StockInDetailViewInfo> StockInDetailInfos)
                {
                    if(stockInDetail.StockInDetailId != 0)
                        _deletedStockInDetailInfos.Add(stockInDetail);

                    StockInDetailInfos.Remove(stockInDetail);
                }
            }
        }
    }

    private void CostOrQuantity_Completed(object sender, EventArgs e)
    {
        RecalculateTotalAmount();
    }

    private void RecalculateTotalAmount()
    {
        if (StockInDetailInfos == null || StockInDetailInfos.Count == 0)
        {
            CurrentStockInInfo.TotalAmount = 0;
            TotalAmountEntry.Value = 0;
            return;
        }

        var total = StockInDetailInfos.Sum(x => x.UnitCost * x.Quantity);

        CurrentStockInInfo.TotalAmount = total;

        TotalAmountEntry.Value = Convert.ToInt32(total);
    }

    private void CostEntry_Loaded(object sender, EventArgs e)
    {
        if (sender is not Entry entry)
            return;

#if WINDOWS
        // Gắn handler bắt Ctrl+N cho TextBox nền của Entry (Windows)
        if (entry.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.TextBox textBox)
        {
            textBox.KeyDown -= DetailTextBox_KeyDown;
            textBox.KeyDown += DetailTextBox_KeyDown;
        }
#endif

    }

    private void CostOrQuanity_UnFocused(object sender, FocusEventArgs e)
    {
        RecalculateTotalAmount();
    }
}
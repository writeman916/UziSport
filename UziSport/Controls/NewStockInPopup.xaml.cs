using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using UziSport.DAL;
using UziSport.Model;

namespace UziSport.Controls;

public partial class NewStockInPopup : ContentView
{
    public event EventHandler? Closed;

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

    private ImportStatus _importStatus = ImportStatus.InProgress;
    public ImportStatus ImportStatus
    {
        get => _importStatus;
        set
        {
            if (_importStatus != value)
            {
                _importStatus = value;
                OnPropertyChanged(nameof(ImportStatus));
                OnPropertyChanged(nameof(IsInProgress));
                OnPropertyChanged(nameof(IsDetailReadOnly));
            }
        }
    }

    // Dùng cho binding trong XAML
    public bool IsInProgress => ImportStatus == ImportStatus.InProgress;
    public bool IsDetailReadOnly => !IsInProgress;

    public ObservableCollection<WarehouseInfo> Warehouses { get; } = new();
    public ObservableCollection<SupplierInfo> Suppliers { get; } = new();
    public StockInViewInfo CurrentStockInInfo { get; set; } = new StockInViewInfo();

    private List<ProductViewInfo> _searchResults = new();
    public List<ProductViewInfo> SearchResults
    {
        get => _searchResults;
        set
        {
            var newValue = value ?? new List<ProductViewInfo>();

            if (_searchResults != newValue)
            {
                _searchResults = newValue;
                OnPropertyChanged(nameof(SearchResults));
                OnPropertyChanged(nameof(HasSearchResults));
            }
        }
    }

    // Dùng cho việc ẩn/hiện dropdown
    public bool HasSearchResults => SearchResults != null && SearchResults.Count > 0;


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

    public enum StockInPopupResults
    {
        Saved,
        Cancel,
        Delete,
    }

    private enum ProcessMode
    {
        New,
        Edit
    }

    public StockInPopupResults Result { get; private set; }

    private StockInDetailViewInfo? _pendingFocusItem;
    private List<StockInDetailViewInfo> _deletedStockInDetailInfos = new();

    public NewStockInPopup()
	{
		InitializeComponent();
        BindingContext = this;

        IsVisible = false;
        Opacity = 0;
    }

    private bool CheckInputs()
    {
        if (this.WareHousePicker.SelectedIndex == -1)
        {
            _ = AppToast.ShowAsync(Controls.ToastView.ToastKind.Error, "Vui lòng chọn kho !", 3000);
            this.WareHousePicker.Focus();
            return false;
        }

        if (StockInDetailInfos == null || StockInDetailInfos.Count == 0)
        {
            _ = AppToast.ShowAsync(Controls.ToastView.ToastKind.Error, "Chưa có sản phẩm nhập kho !", 3000);
            this.SearchEntry.Focus();
            return false;
        }

        return true;
    }


    public async Task ShowAsync()
    {
        IsVisible = true;
        Opacity = 0;

        this.ClearInputs(true);

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

        if (CurrentStockInInfo.StockInId == 0)
        {
            // Phiếu mới: mặc định InProgress
            ImportStatus = ImportStatus.InProgress;

            this.StockInCodeEntry.Text = GenerateNewStockInCode();
        }
        else
        {
            OnPropertyChanged(nameof(CurrentStockInInfo));

            // Set WarehousesPicker
            var warehouse = Warehouses.FirstOrDefault(c => c.WarehouseId == CurrentStockInInfo.WarehouseId);
            if (warehouse != null)
                WareHousePicker.SelectedItem = warehouse;

            // Set SupplierPicker
            var supplier = Suppliers.FirstOrDefault(c => c.SupplierId == CurrentStockInInfo.SupplierId);
            if (supplier != null)
                SupplierPicker.SelectedItem = supplier;

            // Đồng bộ ImportStatus với dữ liệu đang có
            ImportStatus = CurrentStockInInfo.ImportStatus ?? ImportStatus.InProgress;
        }

        // Enable/disable header + nút Lưu
        this.SetEnableAllControl(IsInProgress);

        await this.FadeTo(1, 150);
    }

    private void SetEnableAllControl(bool isEnable)
    {
        this.StockInDatePicker.IsEnabled = isEnable;
        this.WareHousePicker.IsEnabled = isEnable;
        this.SupplierPicker.IsEnabled = isEnable;
        this.NoteEntry.IsEnabled = isEnable;
        this.StatusPicker.IsEnabled = isEnable;
        this.SaveButton.IsEnabled = isEnable;
        this.BtnThem.IsEnabled = isEnable;
        this.SearchEntry.IsEnabled = isEnable;
    }

    public async Task HideAsync()
    {
        this.ClearInputs();
        Closed?.Invoke(this, EventArgs.Empty);

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
            SearchResults = new List<ProductViewInfo>();
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
        )
        .Take(20)
        .ToList();

        SearchResults = filtered;
    }

    private void SearchEntry_Completed(object sender, EventArgs e)
    {
        // Khi người dùng nhấn Enter trong ô search
        if (HasSearchResults)
        {
            var firstItem = SearchResults[0];
            NewStockInDetailViewInfos(firstItem);
        }
    }


    private void NewStockInDetailViewInfos(ProductViewInfo product)
    {
        var newDetail = new StockInDetailViewInfo
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            ProductCode = product.ProductCode,
            Specification = product.Specification,
            BrandName = product.BrandName,
            CatalogName = product.CatalogName,
            UnitCost = product.Cost,
            CreateAt = DateTime.Now,
            CreateBy = Environment.UserName
        };

        StockInDetailInfos.Add(newDetail);

        _pendingFocusItem = newDetail;

        this.SearchEntry.Text = string.Empty;
        this.SearchResults = new List<ProductViewInfo>();
    }

    private void SearchResultButton_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.BindingContext is not ProductViewInfo item)
            return;

        NewStockInDetailViewInfos(item);
    }


    private void ClearInputs(bool isScreenOnly = false)
    {
        this.SearchEntry.Text = string.Empty;
        this.SearchResults = new List<ProductViewInfo>(); // để HasSearchResults update
        this.StockInCodeEntry.Text = string.Empty;
        this.StockInDatePicker.Date = DateTime.Now;
        this.WareHousePicker.SelectedIndex = 0;
        this.SupplierPicker.SelectedIndex = -1;
        this.NoteEntry.Text = string.Empty;
        this.TotalAmountEntry.Value = 0;

        if (isScreenOnly)
        {
            this.StockInDetailInfos.Clear();
            CurrentStockInInfo = new StockInViewInfo();
            OnPropertyChanged(nameof(CurrentStockInInfo));
        }
    }

    private void BtnThem_Clicked(object sender, EventArgs e)
    {
        if(SearchResults.Count == 0)
            return;

        var firstItem = SearchResults[0];

        this.NewStockInDetailViewInfos(firstItem);
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

    private static string RemoveDiacriticsAndNonAlphanumeric(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var normalizedString = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var ch in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);

            if (unicodeCategory != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string GenerateNewStockInCode()
    {
        var userName = Environment.UserName ?? "USER";

        var userPart = RemoveDiacriticsAndNonAlphanumeric(userName);

        if (string.IsNullOrWhiteSpace(userPart))
            userPart = "USER";

        var timePart = DateTime.Now.ToString("yyyyMMddHHmmss");

        return $"{userPart}_{timePart}";
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

    private async void BtnLuu_Clicked(object sender, EventArgs e)
    {
        if (ImportStatus != ImportStatus.InProgress)
            return;

        if(CheckInputs() == false)
            return;

        RecalculateTotalAmount();

        //GetScreen
        var saveStockInInfo = CurrentStockInInfo;

        if (WareHousePicker.SelectedItem is WarehouseInfo selectedWarehouse)
            CurrentStockInInfo.WarehouseId = selectedWarehouse.WarehouseId;

        if (SupplierPicker.SelectedItem is SupplierInfo selectedSupplier)
            CurrentStockInInfo.SupplierId = selectedSupplier.SupplierId;

        saveStockInInfo.Status = (int)ImportStatus;

        if (saveStockInInfo.StockInId == 0)
        {
            saveStockInInfo.CreateAt = DateTime.Now;
            saveStockInInfo.CreateBy = Environment.UserName;
        }else
        {
            saveStockInInfo.UpdateAt = DateTime.Now;
            saveStockInInfo.UpdateBy = Environment.UserName;
        }

        //Get StockInDetailInfos
        saveStockInInfo.StockInDetailInfos = StockInDetailInfos.ToList();

        //Save StockInInfo
        var _stockInDal = new StockInDAL();

        await _stockInDal.SaveItemAsync(saveStockInInfo);

        if(ImportStatus == ImportStatus.Completed)
        {
            // Cập nhật giá theo combo nếu có
            var productComboCostDAL = new ProductComboCostDAL();

            foreach(var detail in saveStockInInfo.StockInDetailInfos)
            {
                if (detail.Cost != detail.UnitCost)
                {
                    await productComboCostDAL.InsertItemByProductId(detail.ProductId, new ProductComboCostInfo()
                    {
                        ProductId = detail.ProductId,
                        StockDetailId = detail.StockInDetailId,
                        Cost = detail.UnitCost.GetValueOrDefault(),
                        CreateAt = DateTime.Now,
                        CreateBy = Environment.UserName
                    });
                }
            }
        }

        //Close Popup
        this.Result = StockInPopupResults.Saved;

        await this.HideAsync();
    }

    private async void BtnCancel_Clicked(object sender, EventArgs e)
    {
        this.Result = StockInPopupResults.Cancel;
        await HideAsync();
    }

    private async void BtnAddWarehouse_Clicked(object sender, EventArgs e)
    {
        await WarehousePopup.ShowAsync();
    }

    private async void Warehouses_Saved(object sender, EventArgs e)
    {
        var warehouses = await WarehouseDAL.Instance.GetWarehousesAsync(forceRefresh: true);
        Warehouses.Clear();
        foreach (var b in warehouses)
            Warehouses.Add(b);
    }

    private async void BtnAddSupplier_Clicked(object sender, EventArgs e)
    {
        await SupplierPopup.ShowAsync();
    }

    private async void Suppliers_Saved(object sender, EventArgs e)
    {
        var suppliers = await SupplierDAL.Instance.GetSuppliersAsync(forceRefresh: true);
        Suppliers.Clear();
        foreach (var b in suppliers)
            Suppliers.Add(b);
    }
}
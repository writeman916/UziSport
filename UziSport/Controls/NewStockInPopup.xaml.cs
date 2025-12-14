using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using UziSport.DAL;
using UziSport.Model;
using UziSport.Services;

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
            }
        }
    }

    public bool IsInProgress => ImportStatus == ImportStatus.InProgress;

    public bool IsDetailReadOnly => _isReadOnlyMode;

    public bool CanEditDetails => !_isReadOnlyMode;

    private void SetReadOnlyMode(bool isReadOnly)
    {
        if (_isReadOnlyMode != isReadOnly)
        {
            _isReadOnlyMode = isReadOnly;
            OnPropertyChanged(nameof(IsDetailReadOnly));
            OnPropertyChanged(nameof(CanEditDetails));
        }
    }
    public ObservableCollection<CatalogInfo> Catalogs { get; } = new();
    public ObservableCollection<BrandInfo> Brands { get; } = new();

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
    private bool _isReadOnlyMode; // true = chỉ xem, false = cho phép chỉnh sửa

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

        // Chỉ clear phần search, không đụng vào dữ liệu đang edit
        ClearInputs(isScreenOnly: true);

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

        // Load Catalog
        var catalogs = await CatalogDAL.Instance.GetCatalogsAsync();
        Catalogs.Clear();
        foreach (var c in catalogs)
            Catalogs.Add(c);

        // Load Brand
        var brands = await BrandDAL.Instance.GetBrandsAsync();
        Brands.Clear();
        foreach (var b in brands)
            Brands.Add(b);

        bool isEdit = CurrentStockInInfo.StockInId != 0;

        if (!isEdit)
        {
            // Phiếu mới
            ImportStatus = ImportStatus.InProgress;

            // Ngày mặc định là hôm nay
            StockInDatePicker.Date = DateTime.Now;

            // Mã phiếu mới
            this.StockInCodeEntry.Text = GenerateNewStockInCode();
        }
        else
        {
            // Phiếu đã có -> bind lại thông tin lên UI
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

            RecalculateTotalAmount();
        }

        // Màn hình chỉ đọc nếu:
        //  - đang mở ở chế độ EDIT
        //  - và trạng thái BAN ĐẦU của phiếu != InProgress (tức là đã Hoàn thành hoặc Hủy)
        bool readOnly = isEdit && ImportStatus != ImportStatus.InProgress;

        SetReadOnlyMode(readOnly);

        // Enable/disable header + nút Lưu, Search... theo chế độ
        SetEnableAllControl(!readOnly);

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

        this.NewProducFrame.IsEnabled = isEnable;
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
            BrandId = product.BrandId,
            CatalogId = product.CatalogId,
            CatalogName = product.CatalogName,
            Price = product.Price ?? 0m,
            UnitCost = product.Cost,
            CreateAt = DateTime.Now,
            CreateBy = Constants.AdminCode
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
        // Luôn clear phần tìm kiếm
        this.SearchEntry.Text = string.Empty;
        this.SearchResults = new List<ProductViewInfo>(); // để HasSearchResults update

        if (isScreenOnly)
        {
            // Không đụng đến dữ liệu đang edit
            return;
        }

        // Clear hoàn toàn khi đóng popup
        this.StockInCodeEntry.Text = string.Empty;
        this.StockInDatePicker.Date = DateTime.Now;
        this.WareHousePicker.SelectedIndex = 0;
        this.SupplierPicker.SelectedIndex = -1;
        this.NoteEntry.Text = string.Empty;
        this.TotalAmountEntry.Value = 0;

        this.StockInDetailInfos.Clear();
        CurrentStockInInfo = new StockInViewInfo();
        OnPropertyChanged(nameof(CurrentStockInInfo));

        // Reset trạng thái
        SetReadOnlyMode(false);
        ImportStatus = ImportStatus.InProgress;
    }

    private void ClearNewProduct()
    {
        this.BarcodeEntry.Text = string.Empty;
        this.ProductNameEntry.Text = string.Empty;
        this.CatalogPicker.SelectedIndex = -1;
        this.BrandPicker.SelectedIndex = -1;
        this.SpecificationEntry.Text = string.Empty;   
        this.CostEntry.Text = string.Empty;
        this.PriceEntry.Text = string.Empty;
        this.BrandPicker.SelectedIndex = -1;
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
                    if (stockInDetail.StockInDetailId != 0 && stockInDetail.ProductId != 0)
                    {
                        stockInDetail.Deleted = true;
                        _deletedStockInDetailInfos.Add(stockInDetail);
                    }

                    StockInDetailInfos.Remove(stockInDetail);
                }

                RecalculateTotalAmount();
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

    private static string GenerateNewStockInCode()
    {
        var timePart = DateTime.Now.ToString("yyyyMMddHHmmss");

        return $"{Constants.AdminCode}_{timePart}";
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
        if (_isReadOnlyMode)
            return;

        if (CheckInputs() == false)
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
            saveStockInInfo.CreateBy = Constants.AdminCode;
        }else
        {
            saveStockInInfo.UpdateAt = DateTime.Now;
            saveStockInInfo.UpdateBy = Constants.AdminCode;
        }

        //Get StockInDetailInfos
        saveStockInInfo.StockInDetailInfos = StockInDetailInfos.ToList();

        foreach(var detail in saveStockInInfo.StockInDetailInfos.Where(x => x.ProductId == 0))
        {
            var productDAL = new ProductDAL();
            
            var newProduct = new ProductViewInfo()
            {
                ProductCode = detail.ProductCode,
                ProductName = detail.ProductName,
                Specification = detail.Specification,
                Cost = detail.UnitCost,
                BrandId = detail.BrandId,
                CatalogId = detail.CatalogId,
                Price = detail.Price,
                CreateAt = DateTime.Now,
                CreateBy = Constants.AdminCode
            };

            var newProductId = await productDAL.SaveItemAsync(newProduct);
            detail.ProductId = newProductId;
        }

        saveStockInInfo.StockInDetailInfos.AddRange(_deletedStockInDetailInfos);

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
                        CreateBy = Constants.AdminCode
                    });
                }
            }
        }

        //Close Popup
        this.Result = StockInPopupResults.Saved;
        ProductStateService.NeedReloadProducts = true;

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

    private void BtnNewProduct_Clicked(object sender, EventArgs e)
    {
        var newProduct = new ProductViewInfo()
        {
            ProductCode = this.BarcodeEntry.Text?.Trim(),
            ProductName = this.ProductNameEntry.Text?.Trim(),
            Specification = this.SpecificationEntry.Text?.Trim(),
            Cost = this.CostEntry.Value,
            Price = this.PriceEntry.Value,
        };

        if (CatalogPicker.SelectedItem is CatalogInfo selectedCatalog)
        {
            newProduct.CatalogId = selectedCatalog.CatalogId;
            newProduct.CatalogName = selectedCatalog.CatalogName;
        }

        if (BrandPicker.SelectedItem is BrandInfo selectedBrand)
        {
            newProduct.BrandId = selectedBrand.BrandId;
            newProduct.BrandName = selectedBrand.BrandName;
        }

        this.NewStockInDetailViewInfos(newProduct);

        ClearNewProduct();
    }
}
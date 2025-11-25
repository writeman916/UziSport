using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using UziSport.Model;
using UziSport.DAL;

namespace UziSport;

public partial class ProductManage : ContentPage
{
    public ObservableCollection<ProductComboCostInfo> ComboCosts { get; set; } = new();
    public ObservableCollection<CatalogInfo> Catalogs { get; } = new();
    public ObservableCollection<BrandInfo> Brands { get; } = new();
    public ProductViewInfo CurrentProduct { get; set; } = new ProductViewInfo();

    private List<ProductViewInfo> _allProductInfos = new();

    public List<ProductViewInfo> AllProductInfos
    {
        get => _allProductInfos;
        set
        {
            if (_allProductInfos != value)
            {
                _allProductInfos = value;
                OnPropertyChanged(nameof(AllProductInfos));
            }
        }
    }

    private int _productId = 0;

    private ProductDAL _productDal = new ProductDAL();

    public ProductManage()
    {
        this.InitializeComponent();
        BindingContext = this;
    }

    private void ClearInput()
    {
        this.BarcodeEntry.Text = string.Empty;
        this.ProductNameEntry.Text = string.Empty;
        this.CatalogPicker.SelectedIndex = -1;
        this.BrandPicker.SelectedIndex = -1;
        this.SpecificationEntry.Text = string.Empty;
        this.CostEntry.Text = string.Empty;
        this.PriceEntry.Text = string.Empty;

        ComboCosts = new ObservableCollection<ProductComboCostInfo>();

        CurrentProduct = new ProductViewInfo();
    }

    private bool CheckInputs()
    {
        if (string.IsNullOrWhiteSpace(BarcodeEntry.Text))
        {
            _ = AppToast.ShowAsync(Controls.ToastView.ToastKind.Error, "Chưa nhập Barcode !", 3000);
            this.BarcodeEntry.Focus();
            return false;
        }

        if (CurrentProduct.ProductId == 0 && this._allProductInfos.Any(x => x.ProductCode == this.BarcodeEntry.Text))
        {
            _ = AppToast.ShowAsync(Controls.ToastView.ToastKind.Error, "Barcode này đã tồn tại !", 3000);
            this.BarcodeEntry.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(ProductNameEntry.Text))
        {
            _ = AppToast.ShowAsync(Controls.ToastView.ToastKind.Error, "Chưa nhập tên sản phẩm !", 3000);
            this.ProductNameEntry.Focus();
            return false;
        }

        return true;
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

        // Load Product List
        AllProductInfos =  await _productDal.GetProductsAsync();
    }

    private void ProductList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection.FirstOrDefault() as ProductViewInfo;
        if (selected == null)
            return;

        // Gán sản phẩm đang chọn lên CurrentProduct (header đang bind vào CurrentProduct)
        CurrentProduct = selected;
        OnPropertyChanged(nameof(CurrentProduct));

        // Set CatalogPicker
        var catalog = Catalogs.FirstOrDefault(c => c.CatalogId == selected.CatalogId);
        if (catalog != null)
            CatalogPicker.SelectedItem = catalog;

        // Set BrandPicker
        var brand = Brands.FirstOrDefault(b => b.BrandId == selected.BrandId);
        if (brand != null)
            BrandPicker.SelectedItem = brand;

        // Nếu muốn load lại ComboCosts theo product:
        ComboCosts.Clear();
        foreach (var combo in CurrentProduct.ProductComboCostInfos)
            ComboCosts.Add(combo);
    }


    private async void BtnLuu_Clicked(object sender, EventArgs e)
    {
        if(CheckInputs() == false)
            return;

        CurrentProduct.ProductComboCostInfos = ComboCosts.ToList();

        if (CatalogPicker.SelectedItem is CatalogInfo selectedCatalog)
            CurrentProduct.CatalogId = selectedCatalog.CatalogId;

        if (BrandPicker.SelectedItem is BrandInfo selectedBrand)
            CurrentProduct.BrandId = selectedBrand.BrandId;

        if (CurrentProduct.ProductId == 0)
        {
            this.CurrentProduct.CreateBy = "admin";
            this.CurrentProduct.CreateAt = DateTime.Now;
        }else
        {
            this.CurrentProduct.UpdateBy = "admin";
            this.CurrentProduct.UpdateAt = DateTime.Now;
        }

        await _productDal.SaveItemAsync(CurrentProduct.ConvertProductToSave());

        _ = AppToast.ShowAsync(Controls.ToastView.ToastKind.Success, "Đã lưu sản phẩm thành công", 2000);

        this.ClearInput();

        AllProductInfos = await _productDal.GetProductsAsync();
    }

    private async void BtnXoa_Clicked(object sender, EventArgs e)
    {
        if (CurrentProduct == null || CurrentProduct.ProductId == 0)
        {
            _ = AppToast.ShowAsync(Controls.ToastView.ToastKind.Warning, "Chưa chọn sản phẩm để xóa", 2000);
            return;
        }

        bool confirm = await DisplayAlert(
            "Xác nhận xóa",
            $"Bạn có chắc muốn xóa sản phẩm \"{CurrentProduct.ProductName}\" không?",
            "Xóa",
            "Không");

        if (!confirm)
            return;

        _ = AppToast.ShowAsync(Controls.ToastView.ToastKind.Success, "Đã xóa sản phẩm thành công", 2000);

        // Reset form nhập
        this.ClearInput();

        AllProductInfos = await _productDal.GetProductsAsync();
    }
}
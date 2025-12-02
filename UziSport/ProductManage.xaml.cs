using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using UziSport.Model;
using UziSport.DAL;

namespace UziSport
{
    public partial class ProductManage : ContentPage
    {
        public ObservableCollection<ProductComboCostInfo> ComboCosts { get; set; } = new();
        public ObservableCollection<CatalogInfo> Catalogs { get; } = new();
        public ObservableCollection<BrandInfo> Brands { get; } = new();
        public ProductViewInfo CurrentProduct { get; set; } = new ProductViewInfo();

        private List<ProductViewInfo> _allProductInfos = new();

        private List<ProductViewInfo> _viewProductInfos = new();

        private ProductViewInfo? _selectedProduct;

        public List<ProductViewInfo> ViewProductInfos
        {
            get => _viewProductInfos;
            set
            {
                if (_viewProductInfos != value)
                {
                    _viewProductInfos = value;
                    OnPropertyChanged(nameof(ViewProductInfos));
                }
            }
        }

        private bool _isInitialized;

        private ProductDAL _productDal = new ProductDAL();

        private bool _isFrameSetting = false;

        private bool _isCostHidden = true;

        // Cờ để tránh vòng lặp khi set lại IsCostHidden trong code
        private bool _isHandlingCostHiddenCheck = false;

        public bool IsCostHidden
        {
            get => _isCostHidden;
            set
            {
                if (_isCostHidden != value)
                {
                    _isCostHidden = value;
                    OnPropertyChanged(nameof(IsCostHidden));
                }
            }
        }


        public ProductManage()
        {
            this.InitializeComponent();

            // Mặc định ẩn giá vốn
            IsCostHidden = true;

            BindingContext = this;
        }

        private void ClearInput(bool screanOnly = false)
        {
            this.BarcodeEntry.Text = string.Empty;
            this.ProductNameEntry.Text = string.Empty;
            this.CatalogPicker.SelectedIndex = -1;
            this.BrandPicker.SelectedIndex = -1;
            this.SpecificationEntry.Text = string.Empty;
            this.CostEntry.Text = string.Empty;
            this.PriceEntry.Text = string.Empty;
            ComboCosts.Clear();

            if (!screanOnly)
            {
                _selectedProduct = null;

                CurrentProduct = new ProductViewInfo();
                OnPropertyChanged(nameof(CurrentProduct));
            }
        }


        private bool CheckInputs()
        {
            if (string.IsNullOrWhiteSpace(BarcodeEntry.Text))
            {
                _ = AppToast.ShowAsync(Controls.ToastView.ToastKind.Error, "Chưa nhập Barcode !", 3000);
                this.BarcodeEntry.Focus();
                return false;
            }

            //if (CurrentProduct.ProductId == 0 && this._viewProductInfos.Any(x => x.ProductCode == this.BarcodeEntry.Text))
            //{
            //    _ = AppToast.ShowAsync(Controls.ToastView.ToastKind.Error, "Barcode này đã tồn tại !", 3000);
            //    this.BarcodeEntry.Focus();
            //    return false;
            //}

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
            {
                return;
            }

            if (!decimal.TryParse(NewCostEntry.Text, out var value))
            {
                return;
            }

            ComboCosts.Add(new ProductComboCostInfo
            {
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

            try
            {
                if (_isInitialized)
                    return;

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
                _allProductInfos = await _productDal.GetProductsAsync();
                ViewProductInfos = _allProductInfos.ToList();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _isInitialized = true;
            }
        }


        private async void ProductList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _isFrameSetting = true;

            this.ClearInput(true);

            var selected = e.CurrentSelection.FirstOrDefault() as ProductViewInfo;
            if (selected == null)
            {
                _isFrameSetting = false;
                return;
            }

            // Giữ lại item gốc trong list
            _selectedProduct = selected;

            // Tạo bản copy để edit
            CurrentProduct = selected.CloneProduct();
            OnPropertyChanged(nameof(CurrentProduct));

            // Set CatalogPicker
            var catalog = Catalogs.FirstOrDefault(c => c.CatalogId == CurrentProduct.CatalogId);
            if (catalog != null)
                CatalogPicker.SelectedItem = catalog;

            // Set BrandPicker
            var brand = Brands.FirstOrDefault(b => b.BrandId == CurrentProduct.BrandId);
            if (brand != null)
                BrandPicker.SelectedItem = brand;

            // Load ComboCosts từ bản copy
            ComboCosts.Clear();
            var combos = await new ProductComboCostDAL().GetItemByProductIdAsync(CurrentProduct.ProductId);
            foreach (var combo in combos)
                ComboCosts.Add(combo);

            _isFrameSetting = false;
        }



        private async void BtnLuu_Clicked(object sender, EventArgs e)
        {
            if (CheckInputs() == false)
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
            }
            else
            {
                this.CurrentProduct.UpdateBy = "admin";
                this.CurrentProduct.UpdateAt = DateTime.Now;
            }

            await _productDal.SaveItemAsync(CurrentProduct);

            _ = AppToast.ShowAsync(Controls.ToastView.ToastKind.Success, "Đã lưu sản phẩm thành công", 2000);

            this.ClearInput();

            ViewProductInfos = await _productDal.GetProductsAsync();
            _allProductInfos = ViewProductInfos.ToList();

            this.BarcodeEntry.Focus();
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

            await _productDal.DeleteItemAsync(CurrentProduct);

            _ = AppToast.ShowAsync(Controls.ToastView.ToastKind.Success, "Đã xóa sản phẩm thành công", 2000);

            // Reset form nhập
            this.ClearInput();

            ViewProductInfos = await _productDal.GetProductsAsync();
            _allProductInfos = ViewProductInfos.ToList();
        }

        private void SearchEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allProductInfos == null)
                return;

            var text = e.NewTextValue;

            if (string.IsNullOrWhiteSpace(text))
            {
                ViewProductInfos = _allProductInfos.ToList();
                return;
            }

            text = text.Trim();

            var filtered = _allProductInfos.Where(p =>
                   (!string.IsNullOrEmpty(p.ProductCode) && p.ProductCode.Contains(text, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrEmpty(p.ProductName) && p.ProductName.Contains(text, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrEmpty(p.CatalogName) && p.CatalogName.Contains(text, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrEmpty(p.BrandName) && p.BrandName.Contains(text, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrEmpty(p.Specification) && p.Specification.Contains(text, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrEmpty(p.Note) && p.Note.Contains(text, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            ViewProductInfos = filtered;
        }

        private void BtnTao_Clicked(object sender, EventArgs e)
        {
            this.ClearInput();
            ViewProductInfos = _allProductInfos.ToList();
            this.BarcodeEntry.Focus();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            ClearInput();
        }

        private async void BtnAddBrand_Clicked(object sender, EventArgs e)
        {
            await BrandPopup.ShowAsync();
        }

        private async void Brands_Saved(object sender, EventArgs e)
        {
            // Refresh Brand list after popup closed
            var brands = await BrandDAL.Instance.GetBrandsAsync(forceRefresh: true);
            Brands.Clear();
            foreach (var b in brands)
                Brands.Add(b);
        }

        private async void BtnAddCatalog_Clicked(object sender, EventArgs e)
        {
            await CatalogPopup.ShowAsync();
        }

        private async void Catalogs_Saved(object sender, EventArgs e)
        {
            // Refresh Catalog list after popup closed
            var catalogs = await CatalogDAL.Instance.GetCatalogsAsync(forceRefresh: true);
            Catalogs.Clear();
            foreach (var b in catalogs)
                Catalogs.Add(b);
        }

        private void BarcodeEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFrameSetting)
                return;

            if (_allProductInfos == null)
                return;

            if (_allProductInfos.Count(p => p.ProductCode == this.BarcodeEntry.Text) > 0)
            {
                this.BarcodeWarning.IsVisible = true;
            }
            else
            {
                this.BarcodeWarning.IsVisible = false;
            }
        }

        private async void CostHiddenCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_isHandlingCostHiddenCheck)
            {
                return;
            }

            if (sender is not CheckBox cb)
            {
                return;
            }

            // Trạng thái hiện tại của hệ thống (ẩn/hiện) nằm ở IsCostHidden
            // e.Value là trạng thái mới của CheckBox (true = checked, false = unchecked)

            // Trường hợp đang ẨN (IsCostHidden == true) mà user UNCHECK (e.Value == false)
            // => user muốn HIỆN giá vốn => phải hỏi mật khẩu trước
            if (IsCostHidden && !e.Value)
            {
                bool ok = await AdminPasswordPopupView.ShowAsync();

                if (ok)
                {
                    // Mật khẩu đúng -> cho phép hiện
                    IsCostHidden = false;
                    // CheckBox đang unchecked (e.Value == false) là đúng với trạng thái mới
                }
                else
                {
                    // Mật khẩu sai hoặc bấm Đóng -> không cho hiện, revert checkbox về checked
                    _isHandlingCostHiddenCheck = true;
                    cb.IsChecked = true;    // user sẽ vẫn thấy checkbox đang check
                    _isHandlingCostHiddenCheck = false;

                    // Vẫn giữ IsCostHidden = true (ẩn)
                }
            }
            // Trường hợp đang HIỆN (IsCostHidden == false) mà user CHECK (e.Value == true)
            // => user muốn ẨN lại -> không cần mật khẩu
            else if (!IsCostHidden && e.Value)
            {
                IsCostHidden = true;
                // CheckBox đã là true (checked) đúng với trạng thái ẩn
            }
            else
            {
                // Các trường hợp còn lại chủ yếu do code set lại (revert),
                // không cần xử lý thêm.
            }
        }

    }
}

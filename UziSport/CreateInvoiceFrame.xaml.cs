using System.Globalization;
using System.Text;
using UziSport.Controls;
using UziSport.DAL;
using UziSport.Model;

namespace UziSport;

public partial class CreateInvoiceFrame : ContentPage
{
    private List<ProductStockViewInfo> _allProductInfos = new();

    private List<ProductStockViewInfo> _viewProductInfos = new();

    public List<ProductStockViewInfo> ViewProductInfos
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

    private List<ProductStockViewInfo> _viewProductInBills = new();

    public List<ProductStockViewInfo> ViewProductInBills
    {
        get => _viewProductInBills;
        set
        {
            if (_viewProductInBills != value)
            {
                _viewProductInBills = value;
                OnPropertyChanged(nameof(ViewProductInBills));
            }
        }
    }

    public StockOutViewInfo CurrentStockOutInfo { get; set; } = new StockOutViewInfo();

    private decimal _totalAmount = 0;
    public decimal TotalAmout
    {
        get => _totalAmount;
        set
        {
            if (_totalAmount != value)
            {
                _totalAmount = value;
                OnPropertyChanged(nameof(TotalAmout));
            }
        }
    }

    private ProductDAL _productDal = new ProductDAL();

    private ProductComboCostDAL _productComboCostDal = new ProductComboCostDAL();

    public CreateInvoiceFrame()
	{
		InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await ClearInputs(true);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {

        }
    }

    private void BtnThem_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is ProductStockViewInfo product)
        {
            var newList = ViewProductInBills?.ToList() ?? new List<ProductStockViewInfo>();

            var inBillProduct = newList.Where(x => x.ProductId == product.ProductId && x.LineDiscountRate == product.LineDiscountRate).FirstOrDefault();

            if (inBillProduct != null)
            {
                inBillProduct.SaleQty++;
            }else
            {
                product.SaleQty = 1;

                newList.Add(product.Clone());
            }

            ViewProductInBills = newList;

            // Tính lại tổng tiền (giả sử Price là decimal? trong ProductStockViewInfo)
            TotalAmout = ViewProductInBills.Sum(x => x.SalePrice);
        }
    }

    private async Task ClearInputs(bool reGetProductlist = false)
    {
        this.StockOutCodeEntry.Text = this.GenerateNewStockOutCode();
        this.StockOutDatePicker.Date = DateTime.Now;
        this.NoteEntry.Text = string.Empty;
        this.DiscountRateEntry.Text = string.Empty;
        this.ViewProductInBills = new List<ProductStockViewInfo>();
        this.TotalAmout = 0;

        if (reGetProductlist)
        {
            // Load Product List
            _allProductInfos = await _productDal.GetProductsWithStockAsync();
            ViewProductInfos = _allProductInfos.ToList();
        }
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

    private string GenerateNewStockOutCode()
    {
        var userName = Environment.UserName ?? "USER";

        var userPart = RemoveDiacriticsAndNonAlphanumeric(userName);

        if (string.IsNullOrWhiteSpace(userPart))
            userPart = "USER";

        var timePart = DateTime.Now.ToString("yyyyMMddHHmmss");

        return $"{userPart}_{timePart}";
    }

    private void LineDiscountRate_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is NumericEntry numericEntry)
        {
            // Khi text bị xóa hết (rỗng / toàn khoảng trắng) thì set lại Value = 0
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                numericEntry.Value = (int?)0m; // 0m = decimal 0
            }
        }
    }

    private void DeleteButton_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is ProductStockViewInfo item)
        {
            var newList = ViewProductInBills?.ToList() ?? new List<ProductStockViewInfo>();

            if (newList.Contains(item))
            {
                newList.Remove(item);
            }

            ViewProductInBills = newList;

            // Tính lại tổng tiền
            TotalAmout = ViewProductInBills.Sum(x => x.SalePrice);
        }
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


    private async void BtnHuy_Clicked(object sender, EventArgs e)
    {
        await ClearInputs();
    }

    private async void BtnLuu_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (ViewProductInBills == null || ViewProductInBills.Count == 0)
            {
                _ = AppToast.ShowAsync(Controls.ToastView.ToastKind.Warning, "Chưa có sản phẩm nào trong hóa đơn.", 2000);
                return;
            }

            // Map từ ViewProductInBills → StockOutDetailViewInfo
            var detailList = new List<StockOutDetailViewInfo>();

            var productInBillIds = ViewProductInBills.GroupBy(x => x.ProductId).Select(g => g.First().ProductId).ToList();

            Dictionary<int, Queue<ProductComboCostInfo>> productComboCostDict = new();

            // Danh sách ID combo đã dùng
            var usedComboCostIds = new List<int>();

            foreach (var productId in productInBillIds)
            {
                var comboCosts = await _productComboCostDal.GetItemByProductIdAsync(productId)
                                  ?? new List<ProductComboCostInfo>();

                // Sắp theo thời gian/id cho chắc, dùng kiểu FIFO
                var ordered = comboCosts
                    .OrderBy(c => c.CreateAt)
                    .ThenBy(c => c.ProductComboCostId);

                productComboCostDict[productId] = new Queue<ProductComboCostInfo>(ordered);
            }

            foreach (var item in ViewProductInBills)
            {
                if (item.SaleQty <= 0)
                    continue;

                decimal unitPrice = item.Price ?? 0m;
                decimal unitCostDefault = item.Cost ?? 0m;
                decimal quantity = item.SaleQty;
                decimal discountRate = item.LineDiscountRate;

                // Chiết khấu trên 1 đơn vị
                decimal discountPerUnit = unitPrice * discountRate / 100m;

                // =========================
                //  CASE ƯU TIÊN GIÁ COMBO
                // =========================
                if (item.IsComboCostPriority
                    && productComboCostDict.TryGetValue(item.ProductId, out var comboQueue)
                    && comboQueue.Count > 0)
                {
                    // Số lượng còn lại cần xuất
                    decimal remainingQty = quantity;

                    // 1 comboCost tương ứng 1 cây (1 đơn vị)
                    while (remainingQty >= 1m && comboQueue.Count > 0)
                    {
                        var comboCost = comboQueue.Dequeue();

                        var detail = new StockOutDetailViewInfo
                        {
                            StockOutDetailId = 0,
                            ProductId = item.ProductId,
                            Quantity = 1m,
                            UnitPrice = unitPrice,
                            UnitCost = comboCost.Cost,
                            LineDiscountAmount = discountPerUnit * 1m,

                            ProductCode = item.ProductCode,
                            ProductName = item.ProductName,
                            Specification = item.Specification,
                            BrandName = item.BrandName,
                            CatalogName = item.CatalogName,
                            Cost = comboCost.Cost,
                            Price = unitPrice,
                            //// Gắn comboCost đã dùng vào detail (nếu anh muốn xem lại)
                            //ProductComboCostInfos = new List<ProductComboCostInfo>
                            //{
                            //    new ProductComboCostInfo
                            //    {
                            //        ProductComboCostId = comboCost.ProductComboCostId,
                            //        ProductId          = comboCost.ProductId,
                            //        Cost               = comboCost.Cost,
                            //        CreateAt           = comboCost.CreateAt,
                            //        CreateBy           = comboCost.CreateBy,
                            //        UpdateAt           = comboCost.UpdateAt,
                            //        UpdateBy           = comboCost.UpdateBy
                            //    }
                            //},
                            Deleted = false
                        };

                        detailList.Add(detail);

                        // Lưu lại ID combo đã dùng để xóa sau khi lưu hóa đơn
                        usedComboCostIds.Add(comboCost.ProductComboCostId);

                        remainingQty -= 1m;
                    }

                    // Phần còn lại (nếu còn) dùng Cost của Product
                    if (remainingQty > 0)
                    {
                        var detail = new StockOutDetailViewInfo
                        {
                            StockOutDetailId = 0,
                            ProductId = item.ProductId,
                            Quantity = remainingQty,
                            UnitPrice = unitPrice,
                            UnitCost = unitCostDefault,
                            LineDiscountAmount = discountPerUnit * remainingQty,

                            ProductCode = item.ProductCode,
                            ProductName = item.ProductName,
                            Specification = item.Specification,
                            BrandName = item.BrandName,
                            CatalogName = item.CatalogName,
                            Cost = unitCostDefault,
                            Price = unitPrice,
                            ProductComboCostInfos = new List<ProductComboCostInfo>(),
                            Deleted = false
                        };

                        detailList.Add(detail);
                    }

                    // Đã xử lý xong dòng này, khỏi chạy case mặc định bên dưới
                    continue;
                }

                // =========================
                //  CASE MẶC ĐỊNH (không ưu tiên combo hoặc hết combo)
                // =========================
                var detailDefault = new StockOutDetailViewInfo
                {
                    StockOutDetailId = 0,
                    ProductId = item.ProductId,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    UnitCost = unitCostDefault,
                    LineDiscountAmount = discountPerUnit * quantity,

                    ProductCode = item.ProductCode,
                    ProductName = item.ProductName,
                    Specification = item.Specification,
                    BrandName = item.BrandName,
                    CatalogName = item.CatalogName,
                    Cost = unitCostDefault,
                    Price = unitPrice,
                    //ProductComboCostInfos = item.ProductComboCostInfos?
                    //    .Select(c => new ProductComboCostInfo
                    //    {
                    //        ProductComboCostId = c.ProductComboCostId,
                    //        ProductId = c.ProductId,
                    //        Cost = c.Cost,
                    //        CreateAt = c.CreateAt,
                    //        CreateBy = c.CreateBy,
                    //        UpdateAt = c.UpdateAt,
                    //        UpdateBy = c.UpdateBy
                    //    })
                    //    .ToList() ?? new List<ProductComboCostInfo>(),
                    Deleted = false
                };

                detailList.Add(detailDefault);
            }

            if (detailList.Count == 0)
            {
                _ = AppToast.ShowAsync(Controls.ToastView.ToastKind.Warning, "Tất cả sản phẩm đều có số lượng bằng 0", 2000);
                return;
            }

            // Gán detail vào CurrentStockOutInfo
            CurrentStockOutInfo.StockOutDetailInfos = detailList;

            // Ngày hóa đơn mặc định là hôm nay nếu chưa set
            if (CurrentStockOutInfo.StockOutDate == default)
            {
                CurrentStockOutInfo.StockOutDate = DateTime.Today;
            }

            // Tổng tiền = tổng (UnitPrice * Qty - LineDiscountAmount) của tất cả dòng
            CurrentStockOutInfo.TotalAmount = detailList.Sum(d => (d.UnitPrice * d.Quantity) - d.LineDiscountAmount);

            // Đồng bộ với property TotalAmout đang bind ra UI
            TotalAmout = CurrentStockOutInfo.TotalAmount;

            var dal = new StockOutDAL();
            await dal.SaveItemAsync(CurrentStockOutInfo);

            if (usedComboCostIds.Count > 0)
            {
                await _productComboCostDal.DeleteByProductComboCostIdAsync(usedComboCostIds);
            }

            // Clear màn hình
            await ClearInputs(true);

            _ = AppToast.ShowAsync(Controls.ToastView.ToastKind.Success, "Tạo hóa đơn thành công.", 2000);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", ex.Message, "OK");
        }
    }

    private void SaleQty_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is NumericEntry numericEntry)
        {
            // Khi text bị xóa hết (rỗng / toàn khoảng trắng) thì set lại Value = 0
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                numericEntry.Value = (int?)0m; // 0m = decimal 0
            }

            TotalAmout = ViewProductInBills.Sum(x => x.SalePrice);
        }
    }
}
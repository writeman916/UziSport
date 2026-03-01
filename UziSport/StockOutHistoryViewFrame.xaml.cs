using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using UziSport.Controls;
using UziSport.DAL;
using UziSport.Model;
using UziSport.Services;

namespace UziSport;

public partial class StockOutHistoryViewFrame : ContentPage
{
    private List<StockOutHistoryInfo> _viewStockOutHistoryInfos = new();

    public List<StockOutHistoryInfo> ViewStockOutHistoryInfos
    {
        get => _viewStockOutHistoryInfos;
        set
        {
            if (_viewStockOutHistoryInfos != value)
            {
                _viewStockOutHistoryInfos = value;
                OnPropertyChanged(nameof(ViewStockOutHistoryInfos));
            }
        }
    }

    private List<StockOutDetailViewInfo> _viewProductInBills = new();

    public List<StockOutDetailViewInfo> ViewProductInBills
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

    public StockOutHistoryInfo CurrentStockOutInfo { get; set; } = new StockOutHistoryInfo();

    public ObservableCollection<PaymentMethodInfo> PaymentMethods { get; } = new();
    public ObservableCollection<PaymentMethodInfo> SearchPaymentMethods { get; } = new();


    public ObservableCollection<PaymentStatusInfo> PaymentStatuses { get; } = new();
    public ObservableCollection<PaymentStatusInfo> SearchPaymentStatuses { get; } = new();


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

    private decimal _totalSaleAmount = 0;
    public decimal TotalSaleAmout
    {
        get => _totalSaleAmount;
        set
        {
            if (_totalSaleAmount != value)
            {
                _totalSaleAmount = value;
                OnPropertyChanged(nameof(TotalSaleAmout));
            }
        }
    }

    private decimal _totalDiscountAmount = 0;
    public decimal TotalDiscountAmount
    {
        get => _totalDiscountAmount;
        set
        {
            if (_totalDiscountAmount != value)
            {
                _totalDiscountAmount = value;
                OnPropertyChanged(nameof(TotalDiscountAmount));
            }
        }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
    }

    private StockOutDAL _stockOutDAL = new StockOutDAL();

    private StockOutDetailDAL _stockOutDetailDAL = new StockOutDetailDAL();

    public StockOutHistoryViewFrame()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        //Init Payment Methods
        if (PaymentMethods.Count == 0)
        {
            var paymentMethods = new ObservableCollection<PaymentMethodInfo>()
            {
                new PaymentMethodInfo { Method = PaymentMethod.Cash,     MethodName = Constants.PaymentMethod_Cash },
                new PaymentMethodInfo { Method = PaymentMethod.Transfer, MethodName = Constants.PaymentMethod_Transfer },
            };

            PaymentMethods.Clear();
            foreach (var method in paymentMethods)
            {
                PaymentMethods.Add(method);
            }

            SearchPaymentMethods.Clear();
            SearchPaymentMethods.Add(new PaymentMethodInfo { Method = PaymentMethod.None, MethodName = " " }); 
            foreach (var method in paymentMethods) SearchPaymentMethods.Add(method);
        }

        //Init Payment Status
        if (PaymentStatuses.Count == 0)
        {
            var paymentStatuses = new ObservableCollection<PaymentStatusInfo>()
                {
                    new PaymentStatusInfo { Method = PaymentStatus.Paid,     MethodName = Constants.PaymentStatus_Paid },
                    new PaymentStatusInfo { Method = PaymentStatus.Unpaid, MethodName = Constants.PaymentStatus_Unpaid },
                };

            PaymentStatuses.Clear();

            foreach (var method in paymentStatuses)
            {
                PaymentStatuses.Add(method);
            }

            SearchPaymentStatuses.Clear();
            SearchPaymentStatuses.Add(new PaymentStatusInfo { Method = PaymentStatus.None, MethodName = " " });
            foreach (var method in paymentStatuses) SearchPaymentStatuses.Add(method);
        }

        this.ClearSearchCriteria();
        this.ClearDetailInfo();
    }


    private void ReCalculateBillTotal()
    {
        TotalAmout = ViewProductInBills.Sum(x => x.LineAfterDiscountSaleAmout);
        TotalDiscountAmount = ViewProductInBills.Sum(x => x.LineDiscountAmount);
        TotalSaleAmout = ViewProductInBills.Sum(x => x.LineSaleAmount);
    }


    private void ClearDetailInfo(bool reGetProductlist = false)
    {
        this.StockOutCodeEntry.Text = string.Empty;
        this.StockOutDatePicker.Date = DateTime.Now;
        this.NoteEntry.Text = string.Empty;
        this.ViewProductInBills = new List<StockOutDetailViewInfo>();
        this.TotalAmout = 0;
        this.TotalSaleAmout = 0;
        this.TotalDiscountAmount = 0;
        this.ActualIncomeEntry.Text = "0";
        this.CurrentStockOutInfo = new StockOutHistoryInfo();

        this.PaymentMethodPicker.SelectedItem = -1;
        this.PaymentStatusPicker.SelectedItem = -1;

        this.ControlEnableToggle(false);
    }

    private void ClearSearchCriteria()
    {
        this.SearchDateFromPicker.Date = DateTime.Now.AddMonths(-1);
        this.SearchDateToPicker.Date = DateTime.Now;
        this.SearchPaymentMethodPicker.SelectedIndex = 0;
        this.SearchPaymentStatusPicker.SelectedIndex = 0;

        this.ViewStockOutHistoryInfos = new List<StockOutHistoryInfo>();
        this.ResultTotalAmountLabel.Text = "0";
        this.ResultProfitAmountLabel.Text = "0";
    }

    private void BtnSearch_Clicked(object sender, EventArgs e)
    {
        this.DoSearch();
    }

    private async void DoSearch()
    {
        //lay dieu kien tim kiem
        var searchCondition = new StockOutSearchCriteria()
        {
            FromDate = this.SearchDateFromPicker.Date,
            ToDate = this.SearchDateToPicker.Date
        };

        if (this.SearchPaymentMethodPicker.SelectedItem is PaymentMethodInfo selectedMethod)
        {
            if (selectedMethod.MethodValue != 0)
                searchCondition.PaymentMethod = selectedMethod.MethodValue;
        }

        if (this.SearchPaymentStatusPicker.SelectedItem is PaymentStatusInfo selectedStatus)
        {
            if (selectedStatus.MethodValue != 0)
                searchCondition.PaymentStatus = selectedStatus.MethodValue;
        }

        IsLoading = true;

        this.ClearDetailInfo();

        try
        {
            //Get du lieu
            this.ViewStockOutHistoryInfos = await _stockOutDAL.GetStockOutHistorysAsync(searchCondition);

            this.ResultProfitAmountLabel.Text = ViewStockOutHistoryInfos.Sum(x => x.ProfitAmount).ToString("N0", CultureInfo.InvariantCulture);
            this.ResultTotalAmountLabel.Text = ViewStockOutHistoryInfos.Sum(x => x.ActualIncome).ToString("N0", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {

            await DisplayAlert("Lỗi", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ControlEnableToggle(bool isEnable)
    {
        SaveButton.IsEnabled = isEnable;
        this.NoteEntry.IsEnabled = isEnable;
        this.ActualIncomeEntry.IsEnabled = isEnable;
        this.PaymentMethodPicker.IsEnabled = isEnable;
        this.PaymentStatusPicker.IsEnabled = isEnable;
    }

    private async void StockOutList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is StockOutHistoryInfo selectedStockOut)
        {
            this.CurrentStockOutInfo = selectedStockOut;
            this.StockOutCodeEntry.Text = selectedStockOut.StockOutCode;
            this.StockOutDatePicker.Date = selectedStockOut.StockOutDate;
            this.NoteEntry.Text = selectedStockOut.Note;
                      
            if (CurrentStockOutInfo != null)
            {
                this.ActualIncomeEntry.Text = CurrentStockOutInfo.ActualIncome.ToString("N0", CultureInfo.InvariantCulture);

                var statusValue = CurrentStockOutInfo.PaymentStatus;
                PaymentStatusPicker.SelectedItem = PaymentStatuses.FirstOrDefault(x => x.MethodValue == statusValue);

                var methodsValue = CurrentStockOutInfo.PaymentMethod;
                PaymentMethodPicker.SelectedItem = PaymentMethods.FirstOrDefault(x => x.MethodValue == methodsValue);
            }

            ControlEnableToggle(CurrentStockOutInfo != null);

            this.ViewProductInBills = await _stockOutDetailDAL.GetDetailByStockOutIdAsync(selectedStockOut.StockOutId);

            this.ReCalculateBillTotal();
        }
    }

    private async void btnLuu_Clicked(object sender, EventArgs e)
    {
        this.CurrentStockOutInfo.Note = this.NoteEntry.Text;
        this.CurrentStockOutInfo.ActualIncome = this.ActualIncomeEntry.Value ?? 0m;
        this.CurrentStockOutInfo.PaymentMethod = (PaymentMethodPicker.SelectedItem as PaymentMethodInfo)?.MethodValue ?? 0;
        this.CurrentStockOutInfo.PaymentStatus = (PaymentStatusPicker.SelectedItem as PaymentStatusInfo)?.MethodValue ?? 0;

        var dal = new StockOutDAL();

        var stockOutInfo = CurrentStockOutInfo;
        stockOutInfo.StockOutDetailInfos = this.ViewProductInBills;

        await dal.SaveItemAsync(stockOutInfo);

        _ = AppToast.ShowAsync(Controls.ToastView.ToastKind.Success, "Lưu hóa đơn thành công.", 2000);

        this.ClearDetailInfo();

        this.DoSearch();
    }
}
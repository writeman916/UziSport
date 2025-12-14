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

        if (PaymentMethods.Count == 0)
        {
            var paymentMethods = new ObservableCollection<PaymentMethodInfo>()
            {
                new PaymentMethodInfo { Method = PaymentMethod.None, MethodName = " "},
                new PaymentMethodInfo { Method = PaymentMethod.Cash,     MethodName = Constants.PaymentMethod_Cash },
                new PaymentMethodInfo { Method = PaymentMethod.Transfer, MethodName = Constants.PaymentMethod_Transfer },
            };

            PaymentMethods.Clear();
            foreach (var method in paymentMethods)
            {
                PaymentMethods.Add(method);
            }
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
    }

    private void ClearSearchCriteria()
    {
        this.SearchDateFromPicker.Date = DateTime.Now.AddMonths(-1);
        this.SearchDateToPicker.Date = DateTime.Now;
        this.PaymentMethodPicker.SelectedIndex = 0;

        this.ViewStockOutHistoryInfos = new List<StockOutHistoryInfo>();
        this.ResultTotalAmountLabel.Text = "0";
        this.ResultProfitAmountLabel.Text = "0";
    }

    private async void BtnSearch_Clicked(object sender, EventArgs e)
    {
        //lay dieu kien tim kiem
        var searchCondition = new StockOutSearchCriteria()
        {
            FromDate = this.SearchDateFromPicker.Date,
            ToDate = this.SearchDateToPicker.Date
        };
        
        if(this.PaymentMethodPicker.SelectedItem is PaymentMethodInfo selectedMethod)
        {
            if(selectedMethod.MethodValue != 0)
                searchCondition.PaymentMethod = selectedMethod.MethodValue;
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
            }

            this.ViewProductInBills = await _stockOutDetailDAL.GetDetailByStockOutIdAsync(selectedStockOut.StockOutId);

            this.ReCalculateBillTotal();
        }
    }
}
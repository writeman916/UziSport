using UziSport.DAL;

namespace UziSport
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private readonly ProductDAL _productDal;

        public MainPage()
        {
            InitializeComponent();

            _productDal = new ProductDAL();

        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            try
            {
                //var products = await _productDal.GetProductsAsync();

                //var brandDAL = new BrandDAL();

                //await brandDAL.SaveItemAsync(new Model.BrandInfo() { BrandName = "Nike" });
                //await brandDAL.SaveItemAsync(new Model.BrandInfo() { BrandName = "Yonex" });
                //await brandDAL.SaveItemAsync(new Model.BrandInfo() { BrandName = "Lining" });
                //await brandDAL.SaveItemAsync(new Model.BrandInfo() { BrandName = "Victor" });
                //await brandDAL.SaveItemAsync(new Model.BrandInfo() { BrandName = "Adidas" });

                var catalogDAL = new CatalogDAL();
                await catalogDAL.SaveItemAsync(new Model.CatalogInfo() { CatalogName = "Vợt" });
                await catalogDAL.SaveItemAsync(new Model.CatalogInfo() { CatalogName = "Giày" });
                await catalogDAL.SaveItemAsync(new Model.CatalogInfo() { CatalogName = "Trang phục" });
                await catalogDAL.SaveItemAsync(new Model.CatalogInfo() { CatalogName = "Phụ kiện" });

            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }
    }

}

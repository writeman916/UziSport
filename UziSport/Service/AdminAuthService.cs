namespace UziSport.Services
{
    public static class AdminAuthService
    {
        public static bool IsAuthorized { get; private set; }

        public static void SetAuthorized(bool authorized)
        {
            IsAuthorized = authorized;
        }

        public static void Reset()
        {
            IsAuthorized = false;
        }
    }
}

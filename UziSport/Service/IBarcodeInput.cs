namespace UziSport.Services;

public interface IBarcodeInput
{
    event EventHandler<string>? BarcodeScanned;

    void Start(nint hwnd);
    void Stop();
    void SetScanMode(bool enabled);

}

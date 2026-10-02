using LocateMobile.Services;
using ZXing.Net.Maui.Controls;

namespace LocateMobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseBarcodeReader()
            .ConfigureFonts(polices => polices.AddFont("LineAwesome.ttf", "LineAwesome"));

        builder.Services.AddSingleton<ApiClient>();
        builder.Services.AddSingleton<Session>();

        return builder.Build();
    }
}

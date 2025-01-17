using CommunityToolkit.Maui;
using MauiContentButton;
using Microsoft.Extensions.Logging;
using PicsyncClient.Components.Popups;
using PicsyncClient.ViewModels.Popups;

namespace PicsyncClient
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .AddMauiContentButtonHandler()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            //builder.Services.AddTransientPopup<AlbumSyncPopup, AlbumSyncViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

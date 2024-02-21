using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Rwb.Images.Services;

namespace Rwb.Images
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            // Services.
            builder.Services.AddSingleton<BuildConfiguration>();
            builder.Services.AddTransient<DuplicateDetector>();
            builder.Services.AddTransient<Rename>();

            // Pages.
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<Filenames>();
            builder.Services.AddSingleton<Duplicates>();
            builder.Services.AddSingleton<Settings>();

            // Blah.
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
                

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

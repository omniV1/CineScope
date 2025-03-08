using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CineScope.Client;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add MudBlazor services with configuration
builder.Services.AddMudServices(config => {
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    // Explicitly set theme colors here as a backup 
    var theme = new MudTheme()
    {
        PaletteDark = new PaletteDark()
        {
            Black = "#0f0f0f",
            White = "#ffffff",
            Primary = "#E50914", // Updated to logo red
            Secondary = "#f5f5f1", // Off-white for contrast
            Success = "#3bef9e",
            Error = "#ff3f5b",
            Warning = "#ffb527",
            Info = "#2196f3",
            Background = "#0f0f0f",
            BackgroundGrey = "#1a1a1a",
            Surface = "#1a1a1a",
            AppbarBackground = "#0f0f0f",
            AppbarText = "#ffffff",
            DrawerBackground = "#1a1a1a",
            DrawerText = "#ffffff",
            TextPrimary = "#ffffff",
            TextSecondary = "#b3b3b3",
            ActionDefault = "#ffffff",
            ActionDisabled = "#636363",
            ActionDisabledBackground = "#2c2c2c",
            LinesDefault = "#2c2c2c",
            LinesInputs = "#4a4a4a",
            TableLines = "#2c2c2c",
            TableStriped = "#2c2c2c"
        },
        Typography = new Typography()
        {
            Default = new Default()
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                FontSize = "1rem",
                FontWeight = 400,
                LineHeight = 1.5,
                LetterSpacing = ".00938em"
            },
            H1 = new H1()
            {
                FontSize = "2.5rem",
                FontWeight = 700,
                LineHeight = 1.167,
                LetterSpacing = "-.01562em"
            },
            H2 = new H2()
            {
                FontSize = "2rem",
                FontWeight = 700,
                LineHeight = 1.2,
                LetterSpacing = "-.00833em"
            }
        }
    };
});

await builder.Build().RunAsync();
using System;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using static Toimistotilojen_varausjarjestelma.Tietokanta;

namespace Toimistotilojen_varausjarjestelma
{
    public partial class App : Application
    {
        // Notice the addition of the 'async' keyword here
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var culture = new CultureInfo("fi-FI");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            Tietokanta db = new Tietokanta();
            TestiDataGeneraattori generaattori = new TestiDataGeneraattori();

            // Wrap the async calls in a try-catch to prevent unhandled application crashes
            try
            {
                // These methods need to be updated in Tietokanta.cs to use 'async Task' 
                // and use await cmd.ExecuteNonQueryAsync(); internally.
                await db.PoistaTietokantaAsync();
                await db.PoistaVanhaTietokantaAsync();
                await db.LuoTietokantaAsync();
                await db.LuoTaulutAsync();

                // Generating mock data is CPU-bound, so it doesn't necessarily need to be async,
                // but doing it before the database inserts keeps things organized.
                generaattori.GeneroiData(50, 3, 15, 100, 5, 10);

                await db.TallennaAsiakkaatAsync(generaattori.Asiakkaat);
                await db.TallennaToimipisteetAsync(generaattori.Toimipisteet);
                await db.TallennaTilatAsync(generaattori.Tilat);
                await db.TallennaVarauksetAsync(generaattori.Varaukset);
                await db.TallennaPalvelutAsync(generaattori.Palvelut);
                await db.TallennaLaitteetAsync(generaattori.Laitteet);
                await db.TallennaLaskutAsync(generaattori.Laskut);
            }
            catch (Exception ex)
            {
                // If the database fails to initialize, show a user-friendly error message
                MessageBox.Show(
                    $"Tietokannan alustuksessa tapahtui virhe. Sovellus suljetaan.\n\nLisätietoja: {ex.Message}",
                    "Kriittinen virhe",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Current.Shutdown(); // Close the application safely
            }
        }
    }
}
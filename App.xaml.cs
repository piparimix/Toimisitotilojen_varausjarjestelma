using System;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using static Toimistotilojen_varausjarjestelma.Tietokanta;

namespace Toimistotilojen_varausjarjestelma
{
    public partial class App : Application
    {
        // Sovelluksen käynnistyessä suoritettava koodi
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Asetetaan sovelluksen kulttuuri suomeksi, jotta päivämäärät ja muut kulttuurisidonnaiset elementit näkyvät suomeksi
            var culture = new CultureInfo("fi-FI");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // Asetetaan sovelluksen kieli suomeksi, jotta WPF-kontrollit (kuten päivämäärävalitsin) näyttävät suomenkieliset nimet
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            // 1. Määritetään projektin juurihakemisto dynaamisesti
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\"));

            // 2. Määritetään polut molemmille kansioille
            string laskutPath = Path.Combine(projectRoot, "Laskut");
            string raportitPath = Path.Combine(projectRoot, "Raportit");

            // 3. Käydään molemmat kansiot läpi taulukon avulla
            string[] foldersToReset = { laskutPath, raportitPath };

            // 4. Poistetaan vanhat kansiot ja luodaan uudet tilalle, käsitellään mahdolliset poikkeukset
            foreach (string folder in foldersToReset)
            {         
                if (Directory.Exists(folder))
                {
                    try
                    {
                        Directory.Delete(folder, true);
                    }
                    catch (IOException ioEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Tiedosto on käytössä tai lukittu ({folder}): " + ioEx.Message);
                    }
                    catch (UnauthorizedAccessException unAuthEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ei oikeuksia kansion poistamiseen ({folder}): " + unAuthEx.Message);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Kansion poisto epäonnistui ({folder}): " + ex.Message);
                    }
                }
                try
                {
                    Directory.CreateDirectory(folder);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Kansion luonti epäonnistui ({folder}): " + ex.Message);
                }
            }


            // Luodaan uusi Tietokanta-olio ja TestiDataGeneraattori-olio
            Tietokanta db = new Tietokanta();
            TestiDataGeneraattori generaattori = new TestiDataGeneraattori();

            // Alustetaan tietokanta ja generoidaan testidata, sekä tallennetaan se tietokantaan
            try
            {
                await db.PoistaTietokantaAsync();
                await db.PoistaVanhaTietokantaAsync();
                await db.LuoTietokantaAsync();
                await db.LuoTaulutAsync();

                // järjestys: asiakkaat, toimipisteet, tilat, varaukset, palvelut, laitteet, laskut
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
                MessageBox.Show(
                    $"Tietokannan alustuksessa tapahtui virhe. Sovellus suljetaan.\n\nLisätietoja: {ex.Message}",
                    "Kriittinen virhe",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Current.Shutdown();
            }
        }
    }
}
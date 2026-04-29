using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace Toimistotilojen_varausjarjestelma
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Sulje_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Haluatko sulkea ohjelman", "Lopeta käyttö", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {

                this.Close();
            }
        }

        private void UusiVaraus_Click(object sender, RoutedEventArgs e)
        {
            Uusi_varaus varausWin = new Uusi_varaus();
            this.Close();
            varausWin.ShowDialog();

        }

        private void Asiakkaat_Click(object sender, RoutedEventArgs e)
        {
            Asiakkaat asiakkaatWin = new Asiakkaat();
            this.Close();
            asiakkaatWin.ShowDialog();
        }

        private void PalvelutJaLaitteet_Click(object sender, RoutedEventArgs e)
        {
            Palvelut palvelutWin = new Palvelut();
            this.Close();
            palvelutWin.ShowDialog();
        }
        private void Toimipisteet_Click(object sender, RoutedEventArgs e)
        {
            Toimipisteet toimipisteetWin = new Toimipisteet();
            this.Close();
            toimipisteetWin.ShowDialog();

        }

        private void Muokkaa_Varausta_Click(object sender, RoutedEventArgs e)
        {
            Muokkaa_Varausta muokkaaWin = new Muokkaa_Varausta();
            this.Close();
            muokkaaWin.ShowDialog();
        }

        private void KaikkiVaraukset_Click(object sender, RoutedEventArgs e)
        {
            Kaikki_varaukset varauksetWin = new Kaikki_varaukset();
            this.Close();
            varauksetWin.ShowDialog();
        }

        private void Raportit_Click(object sender, RoutedEventArgs e)
        {
            Raportit raportitWin = new Raportit();
            this.Close();
            raportitWin.ShowDialog();
        }

        private void Laskutus_Click(object sender, RoutedEventArgs e)
        {
            Laskutuksen_Seuranta laskutusWin = new Laskutuksen_Seuranta();
            this.Close();
            laskutusWin.ShowDialog();
        }


        // TestiPDF-nappi on tarkoitettu vain kehityskäyttöön, eikä sitä ole tarkoitus näkyviin lopullisessa versiossa
        private void TestiPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TestiDataGeneraattori generaattori = new TestiDataGeneraattori();
                generaattori.GeneroiData(5, 2, 5, 5, 3, 3);

                // --- Laskun tietojen asettaminen ---
                var varaus = generaattori.Varaukset[0];
                var asiakas = generaattori.Asiakkaat.First(a => a.AsiakasId == varaus.AsiakasId);
                var toimipiste = generaattori.Toimipisteet.First(t => t.ToimipisteId == varaus.ToimipisteId);
                var tila = generaattori.Tilat.First(t => t.TilaId == varaus.TilaId);
                var lasku = generaattori.Laskut.First(l => l.VarausId == varaus.VarausId);

                if (generaattori.Palvelut.Count > 0) varaus.VaratutPalvelut.Add(generaattori.Palvelut[0]);
                if (generaattori.Laitteet.Count > 0) varaus.VaratutLaitteet.Add(generaattori.Laitteet[0]);

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string projectRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, @"..\..\..\"));

                // --- Luo Lasku PDF ---
                string laskutPath = System.IO.Path.Combine(projectRoot, "Laskut");
                System.IO.Directory.CreateDirectory(laskutPath);
                string laskuPolku = System.IO.Path.Combine(laskutPath, $"Lasku_{lasku.LaskuId}.pdf");

                lasku.Summa = varaus.LaskeVarauksenYhteishinta(tila.Hinta);
                PDF_Palvelu.LuoLaskuPDF(lasku, varaus, asiakas, toimipiste, tila, laskuPolku);

                // --- Luo Raportti PDF ---
                string raportitPath = System.IO.Path.Combine(projectRoot, "Raportit");
                System.IO.Directory.CreateDirectory(raportitPath);
                string raporttiPolku = System.IO.Path.Combine(raportitPath, "TestiRaportti.pdf");

                DateTime alku = DateTime.Now;
                DateTime loppu = DateTime.Now.AddDays(30);

                // Pass the lists of Asiakkaat, Toimipisteet, and Tilat into the method
                PDF_Palvelu.LuoRaporttiPDF(
                    generaattori.Varaukset,
                    generaattori.Asiakkaat,
                    generaattori.Toimipisteet,
                    generaattori.Tilat,
                    alku,
                    loppu,
                    raporttiPolku
                );

                MessageBox.Show($"PDF:t luotu onnistuneesti kansioihin:\n{laskutPath}\n{raportitPath}", "Onnistui", MessageBoxButton.OK, MessageBoxImage.Information);

                // Avaa molemmat tiedostot testauksen vuoksi
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(laskuPolku) { UseShellExecute = true });
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(raporttiPolku) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Virhe PDF:n luonnissa: {ex.Message}", "Virhe", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    } 
}
    

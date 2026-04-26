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
    }
}
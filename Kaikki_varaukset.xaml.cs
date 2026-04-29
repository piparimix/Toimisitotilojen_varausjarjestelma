using System.Windows;
using System.Windows.Controls;

namespace Toimistotilojen_varausjarjestelma
{
    public partial class Kaikki_varaukset : Window
    {

        public Kaikki_varaukset()
        {
            InitializeComponent();

        }

        private void Takaisin_Click(object sender, RoutedEventArgs e)
        {
            MainWindow paavalikko = new MainWindow();
            paavalikko.Show();
            this.Close();
        }
    }
}

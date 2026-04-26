using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Toimistotilojen_varausjarjestelma
{
    /// <summary>
    /// Interaction logic for Laskutuksen_Seuranta.xaml
    /// </summary>
    public partial class Laskutuksen_Seuranta : Window
    {
        public Laskutuksen_Seuranta()
        {
            InitializeComponent();
        }
        private void Takaisin_Click(object sender, RoutedEventArgs e)
        {
            MainWindow paavalikko = new MainWindow();
            paavalikko.Show();
            this.Close();
        }

        private void Hae_Click(object sender, RoutedEventArgs e)
        {
            // Add your logic here
        }

        private void NaytaKaikki_Click(object sender, RoutedEventArgs e)
        {
            // Add your logic here
        }
    }
}

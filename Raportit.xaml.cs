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
    /// Interaction logic for Raportit.xaml
    /// </summary>
    public partial class Raportit : Window
    {
        public Raportit()
        {
            InitializeComponent();
        }

        private void LuoRaportti_Click(object sender, RoutedEventArgs e)
        {
            // Add your logic here
        }

        private void Tyhjenna_Click(object sender, RoutedEventArgs e)
        {
            // Add your logic here
        }

        private void Takaisin_Click(object sender, RoutedEventArgs e)
        {
            MainWindow paavalikko = new MainWindow();
            paavalikko.Show();
            this.Close();
        }


    }
}

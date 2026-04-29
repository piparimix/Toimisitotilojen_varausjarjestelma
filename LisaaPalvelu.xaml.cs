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
    /// Interaction logic for LisaaPalvelu.xaml
    /// </summary>
    public partial class LisaaPalvelu : Window
    {
        public LisaaPalvelu()
        {
            InitializeComponent();
        }

        private void Takaisin_Click(object sender, RoutedEventArgs e)
        {
            Palvelut palvelutWin = new Palvelut();
            palvelutWin.Show();
            this.Close();

        }
    }
}

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
    /// Interaction logic for Uusi_varaus.xaml
    /// </summary>
    public partial class Uusi_varaus : Window
    {
        public Uusi_varaus()
        {
            InitializeComponent();
            LoadDummyData();
        }

        private void Takaisin_Click(object sender, RoutedEventArgs e)
        {
            MainWindow paavalikko = new MainWindow();
            paavalikko.Show();
            this.Close();
        }


        // Tästä alas on esimerkkidataa, joka poistetaan ja korvata oikealla datalla myöhemmin
        private void LoadDummyData()
        {
            string[] palvelut = { "Siivous", "Kasvit", "IT-tuki", "Ikkunan pesu", "Verkkoyhteys", "Kulunvalvonta" };

            foreach (string service in palvelut)
            {
                AddPalvelu(service);
            }
            string[] laitteet = { "Videotykki", "Kaiuttimet", "Neuvottelukaiutin", "Kahvikone", "Monitoimilaite" };

            foreach (string equipment in laitteet)
            {
                AddLaite(equipment);
            }
        }
 
        public void AddPalvelu(string name)
        {
            CheckBox newCheckBox = new CheckBox { Content = name };
            PalvelutListBox.Items.Add(newCheckBox);
        }
        public void AddLaite(string name)
        {
            StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };
            CheckBox cb = new CheckBox { Content = name, Width = 150 };
            ComboBox combo = new ComboBox { Width = 60, SelectedIndex = 0 };
            combo.Items.Add(new ComboBoxItem { Content = "0" });
            combo.Items.Add(new ComboBoxItem { Content = "1" });
            combo.Items.Add(new ComboBoxItem { Content = "2" });
            combo.Items.Add(new ComboBoxItem { Content = "3" });
            sp.Children.Add(cb);
            sp.Children.Add(combo);
            LaitteetListBox.Items.Add(sp);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}


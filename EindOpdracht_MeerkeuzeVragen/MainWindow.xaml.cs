using MeerkeuzeVragenApp.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MeerkeuzeVragenApp.UI
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

        private void BtnTestOpstellen_Click(object sender, RoutedEventArgs e)
            => new TestBeheerView().Show();

        private void BtnImport_Click(object sender, RoutedEventArgs e)
            => new ImportView().Show();

        private void BtnVragenBeheren_Click(object sender, RoutedEventArgs e)
            => new VraagBeheerView().Show();

        private void BtnTestUitvoeren_Click(object sender, RoutedEventArgs e)
            => new TestUitvoerenView().Show();
    }
}

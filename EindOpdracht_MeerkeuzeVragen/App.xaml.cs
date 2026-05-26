using MeerkeuzevragenApp.BUSINESS.Services;
using MeerkeuzevragenApp.DATA;
using MeerkeuzevragenApp.DATA.Repositories;
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
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static VraagService VraagService { get; private set; } = null!;
        public static TestService TestService { get; private set; } = null!;
        public static ImportService ImportService { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string connectionString = "Server=localhost;Port=3306;Database=meerkeuzeDB;User ID=root;Password=root;";
            var db = new DatabaseConnection(connectionString);
            var vraagRepo = new VraagRepository(db);
            var testRepo = new TestRepository(db);

            VraagService = new VraagService(vraagRepo);
            TestService = new TestService(testRepo, vraagRepo);
            ImportService = new ImportService(vraagRepo);

            new MainWindow().Show();
        }
    }
}

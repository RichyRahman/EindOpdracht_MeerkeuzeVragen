using MeerkeuzevragenApp.DATA;
using MeerkeuzevragenApp.DATA.Repositories;
using MeerkeuzevragenApp.DOMEIN.Models;
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
        public static VraagManager VraagManager { get; private set; } = null!;
        public static TestManager TestManager { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            
            var db = new DatabaseConnection();
            var vraagRepo = new VraagRepository(db);
            var testRepo = new TestRepository(db);
            var parsers = new List<MeerkeuzevragenApp.DOMEIN.Interfaces.ITestParser>
            {
                new MeerkeuzevragenApp.DATA.Parsers.CorrectFormaatParser(),
                new MeerkeuzevragenApp.DATA.Parsers.StandaardFormaatParser()
            };

            VraagManager = new VraagManager(vraagRepo);
            TestManager = new TestManager(testRepo, vraagRepo, parsers);

            new MainWindow().Show();
        }
    }
}

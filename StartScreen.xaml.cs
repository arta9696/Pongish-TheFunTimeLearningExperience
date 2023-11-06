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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pongish
{
    /// <summary>
    /// Interaction logic for StartScreen.xaml
    /// </summary>
    public partial class StartScreen : UserControl
    {
        GameController controller = GameController.Instance;
        public StartScreen()
        {
            InitializeComponent();
        }

        public void StartGame_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            controller.ScreensStack.Push(GameController.ScreensStackPosition.GameWindow);
            controller.RunGame();
        }

        public void Settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not Opening settings...");
        }

        public void Quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}

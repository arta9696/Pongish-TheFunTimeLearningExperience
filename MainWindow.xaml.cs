using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace Pongish
{
    public partial class MainWindow : Window
    {
        GameController controller = GameController.Instance;
        private Dictionary<string, UserControl> screens = new Dictionary<string, UserControl>();
        public MainWindow()
        {
            // Initialize the window
            InitializeComponent();

            controller.Window = this;
            controller.Canvas = canvas;
            controller.FPSLabel = FPSLabel;

            Width = controller.Width+15;
            Height = controller.Height+40;
            Title = "Pong Game";
            Closing += MainWindow_Closing;

            screens.Add("StartScreen" ,startScreen);
            //TODO option screen

            controller.InitializeGame();
        }

        public Dictionary<string, UserControl> Screens { get => screens; }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if(controller.Running)
            {
                controller.StopGame();
            }
        }
    }
}

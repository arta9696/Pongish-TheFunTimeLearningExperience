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
        private int _Width = 800;
        private int _Height = 480;
        private int PaddleWidth = 10;
        private int PaddleHeight = 80;
        private int BallSize = 10;

        private Ball ball;
        private Pad leftPaddle;
        private Pad rightPaddle;

        DeltaTimer deltaTimer = new DeltaTimer();
        double deltaTime = 0;
        private BackgroundWorker staticUI = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };
        private Thread updateThread;
        private CancellationTokenSource cTS = new CancellationTokenSource();

        Dictionary<Key, KeyEventArgs> keyDown = new Dictionary<Key, KeyEventArgs>();

        public MainWindow()
        {
            // Initialize the window
            InitializeComponent();
            Width = _Width;
            Height = _Height;
            Title = "Pong Game";
            updateThread = new Thread(UpdateGame) { Priority = ThreadPriority.AboveNormal };
            Closing += MainWindow_Closing;
        }
        private void InitializeGame()
        {
            // Initialize the canvas
            canvas.Background = Brushes.Black;
            canvas.Width = _Width-15;
            canvas.Height = _Height-40;
            welcomePanel.Visibility = Visibility.Hidden;

            // Initialize the paddles and ball
            leftPaddle = new Pad(
                PaddleWidth, 
                PaddleHeight, 
                new Vector(10, canvas.Height / 2 - PaddleHeight / 2),
                canvas
                );

            rightPaddle = new Pad(
                PaddleWidth, 
                PaddleHeight, 
                new Vector(canvas.Width - 20, canvas.Height / 2 - PaddleHeight / 2),
                canvas
                );

            ball = new Ball(
                BallSize, 
                new Vector(canvas.Width / 2 - BallSize / 2, canvas.Height / 2 - BallSize / 2), 
                new Vector(100,100),
                canvas
                );

            // Handle user input for the right paddle
            KeyDown += (sender, e) =>
            {
                KeyEventArgs? kea;
                if (!keyDown.TryGetValue(e.Key, out kea))
                {
                    keyDown.Add(e.Key, e);
                }
            };
            KeyUp += (sender, e) =>
            {
                try
                {
                    keyDown.Remove(e.Key);
                }
                catch { }
            };

            //Handle UI inputs
            staticUI.DoWork += delegate (object? sender, DoWorkEventArgs e)
            {
                while (true)
                {
                    (sender as BackgroundWorker).ReportProgress(0, 1 / deltaTimer.Average());
                    Thread.Sleep(10);
                }
            };
            staticUI.ProgressChanged += delegate (object? sender, ProgressChangedEventArgs e)
            {
                FPSLabel.Content = e.UserState;
            };
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
           StopGame();
        }

        private void RunGame()
        {
            deltaTimer.Start();
            updateThread.Start();
            staticUI.RunWorkerAsync(); ;
        }
        private void StopGame()
        {
            staticUI.CancelAsync();
            cTS.Cancel();
            deltaTimer.Stop();
        }
        private void UpdateGame()
        {
            while (!cTS.IsCancellationRequested)
            {
                deltaTime = deltaTimer.DeltaTime;
                Dispatcher.Invoke(() => {
                    // Move the ball
                    ball.Move(deltaTime);

                    // Check for collisions with top and bottom
                    if (ball.Position.Y < 0 && ball.Speed.Y<0 || ball.Position.Y > canvas.Height - ball.Height && ball.Speed.Y > 0)
                    {
                        ball.Speed = new Vector(ball.Speed.X, -ball.Speed.Y);
                    }

                    // Check for collisions with paddles
                    if (ball.Speed.X<0 && ball.Position.X<=leftPaddle.Position.X && ball.Position.Y>leftPaddle.Position.Y && ball.Position.Y<leftPaddle.Position.Y+leftPaddle.Height)
                    {
                        ball.Speed = new Vector(-ball.Speed.X, ball.Speed.Y);
                    }
                    if (ball.Speed.X > 0 && ball.Position.X >= rightPaddle.Position.X && ball.Position.Y > rightPaddle.Position.Y && ball.Position.Y < rightPaddle.Position.Y + rightPaddle.Height)
                    {
                        ball.Speed = new Vector(-ball.Speed.X, ball.Speed.Y);
                    }

                    // Check if the ball goes out of bounds (scoring)
                    if (ball.Position.X < 0 || ball.Position.X > canvas.Width)
                    {
                        ball.Position = new Vector(canvas.Width / 2 - ball.Width / 2, canvas.Height / 2 - ball.Height / 2);
                    }

                    //Move left paddle
                    if (leftPaddle.Position.Y + leftPaddle.Height / 2 > ball.Position.Y && leftPaddle.Position.Y > 0)
                    {
                        leftPaddle.Move(new Vector(0, -100), deltaTime);
                    }
                    if (leftPaddle.Position.Y + leftPaddle.Height / 2 < ball.Position.Y && leftPaddle.Position.Y < canvas.Height - leftPaddle.Height)
                    {
                        leftPaddle.Move(new Vector(0, 100), deltaTime);
                    }

                    //Move right paddle
                    KeyEventArgs? kea;
                    if (keyDown.TryGetValue(Key.W, out kea) && rightPaddle.Position.Y > 0)
                    {
                        rightPaddle.Move(new Vector(0, -100), deltaTime);
                    }
                    if (keyDown.TryGetValue(Key.S, out kea) && rightPaddle.Position.Y < canvas.Height-rightPaddle.Height)
                    {
                        rightPaddle.Move(new Vector(0, 100), deltaTime);
                    }
                });
            }
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            InitializeGame();
            RunGame();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Opening settings...");
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}

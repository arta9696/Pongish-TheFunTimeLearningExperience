using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Pongish
{
    internal class GameController
    {
        private static readonly GameController instance = new GameController();
        static GameController()
        {
        }
        private GameController()
        {
            screensStack.Push(ScreensStackPosition.MainWindow);
        }
        public static GameController Instance
        {
            get
            {
                return instance;
            }
        }

        public MainWindow? Window { set => window = value; }
        public Canvas? Canvas { get => canvas; set => canvas = value; }
        public Label? FPSLabel { get => _FPSLabel; set => _FPSLabel = value; }
        public bool Running { get => running; }
        public Stack<ScreensStackPosition> ScreensStack { get => screensStack; }

        private bool running = false;
        private MainWindow? window;
        Canvas? canvas;
        Label? _FPSLabel;

        public int Width = 800 - 15;
        public int Height = 480 - 40;
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
        private CancellationTokenSource cTS;

        Dictionary<Key, KeyEventArgs> keyDown = new Dictionary<Key, KeyEventArgs>();
        public enum ScreensStackPosition { MainWindow, GameWindow, Menu, SubMenu}
        private Stack<ScreensStackPosition> screensStack = new Stack<ScreensStackPosition>();

        public void InitializeGame()
        {
            if(window is not null)
            {
                // Handle user input for the right paddle
                window.KeyDown += (sender, e) =>
                {
                    if (screensStack.Peek() == ScreensStackPosition.GameWindow)
                    {
                        if (!keyDown.TryGetValue(e.Key, out KeyEventArgs? key))
                        {
                            keyDown.Add(e.Key, e);
                        }
                    }
                };
                window.KeyUp += (sender, e) =>
                {
                    if (screensStack.Peek() == ScreensStackPosition.GameWindow)
                    {
                        try
                        {
                            keyDown.Remove(e.Key);
                        }
                        catch { }
                    }
                };
            }

            //Handle UI inputs
            staticUI.DoWork += delegate (object? sender, DoWorkEventArgs e)
            {
                while (true)
                {
                    (sender as BackgroundWorker).ReportProgress(0, 1 / deltaTimer.Average());
                    Thread.Sleep(10);
                }
            };
            if (_FPSLabel is not null)
            {
                staticUI.ProgressChanged += delegate (object? sender, ProgressChangedEventArgs e)
                {
                    if (screensStack.Peek() == ScreensStackPosition.GameWindow)
                    {
                        _FPSLabel.Content = e.UserState;
                    }
                };
            }
            staticUI.RunWorkerAsync();
        }

        public void RunGame()
        {
            // Initialize the canvas
            if (canvas is not null)
            {
                canvas.Background = Brushes.Black;
                canvas.Width = Width;
                canvas.Height = Height;

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
                new Vector(100, 100),
                    canvas
                    );
            }

            if (_FPSLabel is not null)
            {
                _FPSLabel.Visibility = Visibility.Visible;
            }

            if (window != null)
            {
                updateThread = new Thread(UpdateGame) { Priority = ThreadPriority.AboveNormal };
                cTS = new CancellationTokenSource();
                running = true;

                deltaTimer.Start();
                updateThread.Start();
            }
        }
        public void StopGame()
        {
            // Deinitialize the canvas
            if (canvas is not null)
            {
                canvas.Background = Brushes.White;

                // Deinitialize the paddles and ball
                leftPaddle.Dispose();

                rightPaddle.Dispose();

                ball.Dispose();
            }
            if (_FPSLabel is not null)
            {
                _FPSLabel.Visibility = Visibility.Hidden;
            }

            running = false;
            cTS.Cancel();
            deltaTimer.Stop();
        }
        private void UpdateGame()
        {
            while (!cTS.IsCancellationRequested)
            {
                deltaTime = deltaTimer.DeltaTime;
                window.Dispatcher.Invoke(() => {
                    // Move the ball
                    ball.Move(deltaTime);

                    // Check for collisions with top and bottom
                    if (ball.Position.Y < 0 && ball.Speed.Y < 0 || ball.Position.Y > Height - ball.Height && ball.Speed.Y > 0)
                    {
                        ball.Speed = new Vector(ball.Speed.X, -ball.Speed.Y);
                    }

                    // Check for collisions with paddles
                    if (ball.Speed.X < 0 && ball.Position.X <= leftPaddle.Position.X && ball.Position.Y > leftPaddle.Position.Y && ball.Position.Y < leftPaddle.Position.Y + leftPaddle.Height)
                    {
                        ball.Speed = new Vector(-ball.Speed.X, ball.Speed.Y);
                    }
                    if (ball.Speed.X > 0 && ball.Position.X >= rightPaddle.Position.X && ball.Position.Y > rightPaddle.Position.Y && ball.Position.Y < rightPaddle.Position.Y + rightPaddle.Height)
                    {
                        ball.Speed = new Vector(-ball.Speed.X, ball.Speed.Y);
                    }

                    // Check if the ball goes out of bounds (scoring)
                    if (ball.Position.X < 0 || ball.Position.X > Width)
                    {
                        ball.Position = new Vector(Width / 2 - ball.Width / 2, Height / 2 - ball.Height / 2);
                    }

                    //Move left paddle
                    if (leftPaddle.Position.Y + leftPaddle.Height / 2 > ball.Position.Y && leftPaddle.Position.Y > 0)
                    {
                        leftPaddle.Move(new Vector(0, -100), deltaTime);
                    }
                    if (leftPaddle.Position.Y + leftPaddle.Height / 2 < ball.Position.Y && leftPaddle.Position.Y < Height - leftPaddle.Height)
                    {
                        leftPaddle.Move(new Vector(0, 100), deltaTime);
                    }

                    //Move right paddle
                    KeyEventArgs? key;
                    if (keyDown.TryGetValue(Key.W, out key) && rightPaddle.Position.Y > 0)
                    {
                        rightPaddle.Move(new Vector(0, -100), deltaTime);
                    }
                    if (keyDown.TryGetValue(Key.S, out key) && rightPaddle.Position.Y < Height - rightPaddle.Height)
                    {
                        rightPaddle.Move(new Vector(0, 100), deltaTime);
                    }

                    //Other
                    if (keyDown.TryGetValue(Key.Escape, out key))
                    {
                        window.Screens["StartScreen"].Visibility = Visibility.Visible;
                        keyDown.Remove(Key.Escape);
                        screensStack.Pop();
                        StopGame();
                    }

                    /* TODO insted of rigit keys to functions use mapping dictionary like
                     * Dictionary <Keys, Meaning>
                     * keyDown <Meaning, KeyEventArgs>
                     * enum Meaning {}
                     * then in options you can change Dictionary <Keys, Meaning> setting, and dont touch code
                     */
                });
            }
        }
    }
}

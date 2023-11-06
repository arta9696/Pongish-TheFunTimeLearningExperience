using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Pongish
{
    internal class Ball: IDisposable
    {
        Ellipse Geometry { get; set; }
        Canvas _canvas;
        public Vector Position { 
            get { 
                return new Vector(Canvas.GetLeft(Geometry), Canvas.GetTop(Geometry)); 
            } 
            set {
                Canvas.SetTop(Geometry, value.Y);
                Canvas.SetLeft(Geometry, value.X);
            }
        }
        public double Width { get { return Geometry.Width; } }
        public double Height { get { return Geometry.Height; } }
        public Vector Speed {  get; set; }
        public Ball(double BallSize, Vector position, Vector speed, Canvas canvas) {
            Geometry = new Ellipse
            {
                Width = BallSize,
                Height = BallSize,
                Fill = Brushes.White
            };
            Canvas.SetTop(Geometry, position.Y);
            Canvas.SetLeft(Geometry, position.X);
            Speed = speed;
            canvas.Children.Add(Geometry);
            _canvas = canvas;
        }
        public void Move(double deltaTime)
        {
            Canvas.SetTop(Geometry, Canvas.GetTop(Geometry) + Speed.Y*deltaTime);
            Canvas.SetLeft(Geometry, Canvas.GetLeft(Geometry) + Speed.X*deltaTime);
        }

        public void Dispose()
        {
            _canvas.Children.Remove(Geometry);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Pongish
{
    internal class Pad
    {
        Rectangle Geometry { get; set; }
        public Vector Position
        {
            get
            {
                return new Vector(Canvas.GetLeft(Geometry), Canvas.GetTop(Geometry));
            }
            set
            {
                Canvas.SetTop(Geometry, value.Y);
                Canvas.SetLeft(Geometry, value.X);
            }
        }
        public double Width { get { return Geometry.Width; } }
        public double Height { get { return Geometry.Height; } }
        public Pad(double PaddleWidth, double PaddleHeight, Vector position, Canvas canvas) {
            Geometry = new Rectangle
            {
                Width = PaddleWidth,
                Height = PaddleHeight,
                Fill = Brushes.White
            };
            Canvas.SetTop(Geometry, position.Y);
            Canvas.SetLeft(Geometry, position.X);
            canvas.Children.Add(Geometry);
        }

        public void Move(Vector direction, double deltaTime)
        {
            Canvas.SetTop(Geometry, Canvas.GetTop(Geometry)+direction.Y * deltaTime);
            Canvas.SetLeft(Geometry, Canvas.GetLeft(Geometry) + direction.X * deltaTime);
        }
    }
}

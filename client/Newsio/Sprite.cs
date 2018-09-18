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

namespace WpfApplication1
{
    public class Sprite
    {
        public int X=0;
        public int Y=0;
        public Image tex;
        public BitmapImage bi;
        public Thickness XY;
        public int VelocityX = 0;
        public int VelocityY = 0;
        public int width = 50;
        public int height = 50;
        public Sprite(string name, int X, int Y)
        {
            this.X = X;
            this.Y = Y;

            tex = new Image();
            tex.Width = 50;
            tex.Height = 50;
            tex.Margin = new Thickness(0);

            bi = new BitmapImage();

            //load image based on name in constructor
            bi.BeginInit();
            if (name == "TRUMP")
                bi.UriSource = new Uri(@"../trump.png", UriKind.Relative);
            else if (name == "CAMERON")
                bi.UriSource = new Uri(@"../cameron.png", UriKind.Relative);
            else if (name == "QUEEN")
                bi.UriSource = new Uri(@"../queen.png", UriKind.Relative);
            else if (name == "BALL")
                bi.UriSource = new Uri(@"../ball.png", UriKind.Relative);
            else if (name == "COWELL")
                bi.UriSource = new Uri(@"../cowell.png", UriKind.Relative);
            else if (name == "BASKETBALL")
                bi.UriSource = new Uri(@"../bball.png", UriKind.Relative);
            bi.EndInit();

            tex = new Image();
            tex.Width = 50;
            tex.Height = 50;
            tex.Source = bi;
            XY = new Thickness(X, Y, 0, 0);
            tex.Margin = XY;

            tex.Visibility = Visibility.Visible;
        }

        //move based on a set velocity each update
        public void UpdateVelocity()
        {
            Move(VelocityX, VelocityY);
        }

        //update position using Margin feature
        public void Move(int x, int y)
        {
            XY.Top += y;
            XY.Left += x;
            tex.Margin = XY;
        }
    }
}

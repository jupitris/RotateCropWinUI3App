using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using Windows.Graphics.Imaging;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RotateCropWinUI3App.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RotatedImagePage : Page
    {
        public RotatedImagePage()
        {
            InitializeComponent();
            Draw();
        }

        private void Draw(int angle = 0)
        {
            string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            System.Drawing.Image source = Bitmap.FromFile($"{path}/Assets/Images/undou_zenpou_chugaeri.png");
            using Bitmap bitmap = new(source);
            using Graphics graphics = Graphics.FromImage(bitmap);
            using Pen pen = new(System.Drawing.Color.FromArgb(255, 255, 0, 0), 2);
            using SolidBrush brush = new(System.Drawing.Color.FromArgb(25, 255, 0, 0));

            int centerX = bitmap.Width / 2;
            int centerY = bitmap.Height / 2;

            // 画像の中心に幅250px、高さ300pxの矩形を描画する
            Point p0 = new(centerX - 125, centerY - 150);
            Point p1 = new(centerX + 125, centerY - 150);
            Point p2 = new(centerX + 125, centerY + 150);
            Point p3 = new(centerX - 125, centerY + 150);

            System.Drawing.Drawing2D.Matrix matrix = new();
            matrix.RotateAt(angle, new PointF(centerX, centerY));   // 回転軸を画像の中心にする
            matrix.TransformPoints(new Point[] { p0, p1, p2, p3 }); // ジオメトリック変換(この例では回転)を矩形座標へ適用する

            graphics.Transform = matrix;
            graphics.DrawLine(pen, p0, p1);
            graphics.DrawLine(pen, p2, p3);
            graphics.DrawLine(pen, p0, p3);
            graphics.DrawLine(pen, p1, p2);
            graphics.FillPolygon(brush, new Point[] { p0, p1, p2, p3 });

            BitmapImage bitmapImage = new();
            using MemoryStream stream = new();
            bitmap.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            bitmapImage.SetSource(stream.AsRandomAccessStream());

            Microsoft.UI.Xaml.Controls.Image image = MainImage;
            image.Source = bitmapImage;
        }

        private void AngleSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            int angle = (int)e.NewValue;
            Draw(angle);
        }
    }
}

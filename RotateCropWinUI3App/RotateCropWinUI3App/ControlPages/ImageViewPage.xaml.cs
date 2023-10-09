using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
    public sealed partial class ImageViewPage : Page
    {
        private Point[] _points = Array.Empty<Point>();
        private int _angle = 0;

        public ImageViewPage()
        {
            InitializeComponent();
            Draw();
        }

        private void Draw(int angle = 0)
        {
            // 切り抜き対象画像(この段階では表示のみ)を読み込む
            string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            System.Drawing.Image source = Bitmap.FromFile($"{path}/Assets/Images/undou_zenpou_chugaeri.png");
            using Bitmap bitmap = new(source);

            // 画像の中心座標を取得する
            int centerX = bitmap.Width / 2;
            int centerY = bitmap.Height / 2;

            // 画像を中心に幅250px、高さ300pxの矩形座標を作る
            // TODO: 自由に矩形を描画する場合は、マウスポインターから座標を取得するなど動的に座標を決める
            Point p0 = new(centerX - 125, centerY - 150);
            Point p1 = new(centerX + 125, centerY - 150);
            Point p2 = new(centerX + 125, centerY + 150);
            Point p3 = new(centerX - 125, centerY + 150);
            _points = new[] { p0, p1, p2, p3 };

            // 矩形座標の回転
            System.Drawing.Drawing2D.Matrix matrix = new();
            matrix.RotateAt(angle, new PointF(centerX, centerY));   // 回転軸を画像の中心にする
            matrix.TransformPoints(_points);                        // ジオメトリック変換(この例では回転)を矩形座標へ適用する

            // 矩形描画
            using Graphics graphics = Graphics.FromImage(bitmap);
            using Pen pen = new(System.Drawing.Color.FromArgb(255, 255, 0, 0), 2);
            using SolidBrush brush = new(System.Drawing.Color.FromArgb(25, 255, 0, 0));
            graphics.Transform = matrix;
            graphics.DrawLine(pen, p0, p1);
            graphics.DrawLine(pen, p2, p3);
            graphics.DrawLine(pen, p0, p3);
            graphics.DrawLine(pen, p1, p2);
            graphics.FillPolygon(brush, new Point[] { p0, p1, p2, p3 });

            // BitmapImageへ変換
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
            _angle = (int)e.NewValue;
            Draw(_angle);
        }

        private void CropImage_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // 切り抜き対象画像(この段階では表示のみ)を読み込む
            string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            System.Drawing.Image source = Bitmap.FromFile($"{path}/Assets/Images/undou_zenpou_chugaeri.png");
            using Bitmap srcBitmap = new(source);

            // NOTE: 実行順序が重要。回転 → 移動 → 複写の順に実行する。
            // 矩形座標に回転を適用する
            using System.Drawing.Drawing2D.Matrix matrix = new();
            matrix.Rotate(_angle * -1);
            matrix.TransformPoints(_points);

            // 矩形のサイズを取得する
            int xmin = _points.Min(p => p.X);
            int xmax = _points.Max(p => p.X);
            int ymin = _points.Min(p => p.Y);
            int ymax = _points.Max(p => p.Y);
            int width = xmax - xmin;
            int height = ymax - ymin;
            
            // 切り抜いた画像の複写先を用意する
            using Bitmap dstBitmap = new(srcBitmap, width, height);

            // 矩形の中心に収まるように移動する
            matrix.Translate(-xmin, -ymin, MatrixOrder.Append);

            // 新しいBitmapに複写する
            using Graphics graphics = Graphics.FromImage(dstBitmap);
            graphics.Transform = matrix;
            graphics.Clear(System.Drawing.Color.FromArgb(255, 243, 243, 243)); // 画像の背景色と同じ色で塗りつぶす
            graphics.DrawImage(srcBitmap, Point.Empty);

            Frame.Navigate(typeof(CroppedImagePage), (Bitmap)dstBitmap.Clone());
        }
    }
}

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RotateCropWinUI3App.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImageViewPage : Page
    {
        private PointF[] _points = Array.Empty<PointF>();
        private int _angle = 0;

        public ImageViewPage()
        {
            InitializeComponent();
            Draw();
        }

        private void Draw(int angle = 0)
        {
            // 切り抜き対象画像(この段階では表示のみ)を読み込む
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            System.Drawing.Image source = System.Drawing.Image.FromFile($"{path}/Assets/Images/undou_zenpou_chugaeri.png");
            using Bitmap bitmap = new(source);

            // 画像の中心座標を取得する
            float centerX = bitmap.Width / 2;
            float centerY = bitmap.Height / 2;

            // 画像に対して80%の矩形を描画する 
            // TODO: 自由に矩形を描画する場合は、マウスポインターから座標を取得するなど動的に座標を決める
            PointF p0 = new(centerX - (centerX * 0.8f), centerY - (centerY * 0.8f));
            PointF p1 = new(centerX + (centerX * 0.8f), centerY - (centerY * 0.8f));
            PointF p2 = new(centerX + (centerX * 0.8f), centerY + (centerY * 0.8f));
            PointF p3 = new(centerX - (centerX * 0.8f), centerY + (centerY * 0.8f));
            _points = new[] { p0, p1, p2, p3 };

            // 矩形座標の回転
            Matrix matrix = new();
            matrix.RotateAt(angle, new PointF(centerX, centerY));   // 回転軸を画像の中心にする
            matrix.TransformPoints(_points);                        // ジオメトリック変換(この例では回転)を矩形座標へ適用する

            // 矩形描画
            using Graphics graphics = Graphics.FromImage(bitmap);
            using Pen pen = new(Color.FromArgb(255, 255, 0, 0), 2);
            using SolidBrush brush = new(Color.FromArgb(25, 255, 0, 0));
            graphics.Transform = matrix;
            graphics.DrawLine(pen, p0, p1);
            graphics.DrawLine(pen, p2, p3);
            graphics.DrawLine(pen, p0, p3);
            graphics.DrawLine(pen, p1, p2);
            graphics.FillPolygon(brush, new PointF[] { p0, p1, p2, p3 });

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
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            System.Drawing.Image source = System.Drawing.Image.FromFile($"{path}/Assets/Images/undou_zenpou_chugaeri.png");
            using Bitmap srcBitmap = new(source);

            // NOTE: 実行順序が重要。回転 → 移動 → 複写の順に実行する。
            // 矩形座標に回転を適用する
            using Matrix matrix = new();
            matrix.Rotate(_angle * -1);
            matrix.TransformPoints(_points);

            // 矩形のサイズを取得する
            float xmin = _points.Min(p => p.X);
            float xmax = _points.Max(p => p.X);
            float ymin = _points.Min(p => p.Y);
            float ymax = _points.Max(p => p.Y);
            int width = (int)Math.Ceiling(xmax - xmin);
            int height = (int)Math.Ceiling(ymax - ymin);
            
            // 切り抜いた画像の複写先を用意する
            using Bitmap dstBitmap = new(srcBitmap, width, height);

            // 矩形の中心に収まるように移動する
            matrix.Translate(-xmin, -ymin, MatrixOrder.Append);

            // 新しいBitmapに複写する
            using Graphics graphics = Graphics.FromImage(dstBitmap);
            graphics.Transform = matrix;
            graphics.Clear(Color.FromArgb(255, 243, 243, 243)); // 画像の背景色と同じ色で塗りつぶす
            graphics.DrawImage(srcBitmap, Point.Empty);

            Frame.Navigate(typeof(CroppedImagePage), (Bitmap)dstBitmap.Clone());
        }
    }
}

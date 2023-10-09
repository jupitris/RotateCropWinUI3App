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
            // �؂蔲���Ώۉ摜(���̒i�K�ł͕\���̂�)��ǂݍ���
            string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            System.Drawing.Image source = Bitmap.FromFile($"{path}/Assets/Images/undou_zenpou_chugaeri.png");
            using Bitmap bitmap = new(source);

            // �摜�̒��S���W���擾����
            int centerX = bitmap.Width / 2;
            int centerY = bitmap.Height / 2;

            // �摜�𒆐S�ɕ�250px�A����300px�̋�`���W�����
            // TODO: ���R�ɋ�`��`�悷��ꍇ�́A�}�E�X�|�C���^�[������W���擾����ȂǓ��I�ɍ��W�����߂�
            Point p0 = new(centerX - 125, centerY - 150);
            Point p1 = new(centerX + 125, centerY - 150);
            Point p2 = new(centerX + 125, centerY + 150);
            Point p3 = new(centerX - 125, centerY + 150);
            _points = new[] { p0, p1, p2, p3 };

            // ��`���W�̉�]
            System.Drawing.Drawing2D.Matrix matrix = new();
            matrix.RotateAt(angle, new PointF(centerX, centerY));   // ��]�����摜�̒��S�ɂ���
            matrix.TransformPoints(_points);                        // �W�I���g���b�N�ϊ�(���̗�ł͉�])����`���W�֓K�p����

            // ��`�`��
            using Graphics graphics = Graphics.FromImage(bitmap);
            using Pen pen = new(System.Drawing.Color.FromArgb(255, 255, 0, 0), 2);
            using SolidBrush brush = new(System.Drawing.Color.FromArgb(25, 255, 0, 0));
            graphics.Transform = matrix;
            graphics.DrawLine(pen, p0, p1);
            graphics.DrawLine(pen, p2, p3);
            graphics.DrawLine(pen, p0, p3);
            graphics.DrawLine(pen, p1, p2);
            graphics.FillPolygon(brush, new Point[] { p0, p1, p2, p3 });

            // BitmapImage�֕ϊ�
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
            // �؂蔲���Ώۉ摜(���̒i�K�ł͕\���̂�)��ǂݍ���
            string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            System.Drawing.Image source = Bitmap.FromFile($"{path}/Assets/Images/undou_zenpou_chugaeri.png");
            using Bitmap srcBitmap = new(source);

            // NOTE: ���s�������d�v�B��] �� �ړ� �� ���ʂ̏��Ɏ��s����B
            // ��`���W�ɉ�]��K�p����
            using System.Drawing.Drawing2D.Matrix matrix = new();
            matrix.Rotate(_angle * -1);
            matrix.TransformPoints(_points);

            // ��`�̃T�C�Y���擾����
            int xmin = _points.Min(p => p.X);
            int xmax = _points.Max(p => p.X);
            int ymin = _points.Min(p => p.Y);
            int ymax = _points.Max(p => p.Y);
            int width = xmax - xmin;
            int height = ymax - ymin;
            
            // �؂蔲�����摜�̕��ʐ��p�ӂ���
            using Bitmap dstBitmap = new(srcBitmap, width, height);

            // ��`�̒��S�Ɏ��܂�悤�Ɉړ�����
            matrix.Translate(-xmin, -ymin, MatrixOrder.Append);

            // �V����Bitmap�ɕ��ʂ���
            using Graphics graphics = Graphics.FromImage(dstBitmap);
            graphics.Transform = matrix;
            graphics.Clear(System.Drawing.Color.FromArgb(255, 243, 243, 243)); // �摜�̔w�i�F�Ɠ����F�œh��Ԃ�
            graphics.DrawImage(srcBitmap, Point.Empty);

            Frame.Navigate(typeof(CroppedImagePage), (Bitmap)dstBitmap.Clone());
        }
    }
}

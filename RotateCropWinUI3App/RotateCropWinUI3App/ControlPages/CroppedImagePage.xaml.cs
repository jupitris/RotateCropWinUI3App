using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RotateCropWinUI3App.ControlPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CroppedImagePage : Page
    {
        public CroppedImagePage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e) 
        {
            if (e.Parameter is Bitmap bitmap)
            {
                // BitmapImage‚Ö•ÏŠ·
                BitmapImage bitmapImage = new();
                using MemoryStream stream = new();
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                bitmapImage.SetSource(stream.AsRandomAccessStream());

                Microsoft.UI.Xaml.Controls.Image image = MainImage;
                image.Source = bitmapImage;
            }
            base.OnNavigatedTo(e);
        }

        private void Reset_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) => Frame.Navigate(typeof(ImageViewPage));
    }
}

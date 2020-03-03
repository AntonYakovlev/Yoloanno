using System.Windows;
using System.Windows.Media.Imaging;
using Yoloanno.Types;

namespace Yoloanno.UI
{
    /// <summary>
    /// Interaction logic for RegionPreviewWnd.xaml
    /// </summary>
    public partial class RegionPreviewWnd : Window
    {
        private static RegionPreviewWnd _instance;

        public static RegionPreviewWnd Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new RegionPreviewWnd();
                }
                return _instance;
            }
        }
        public RegionPreviewWnd()
        {
            InitializeComponent();            
        }

        public void UpdatePreview(BitmapSource image, DetectionRegion region)
        {
            previewAreaImg.Source = image;
            if (region != null)
            {
                Title = "" + region;
                DuplicatorLbl.Content = region.Category.Description;
            }
        }        

        private void PreviewForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }
    }
}

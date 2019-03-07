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
//Thumb コントロールで Photoshop のナビゲーターを再現する | grabacr.nét
//http://grabacr.net/archives/1723

namespace _20190307_レイアウト2
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            BitmapImage image;
            image = new BitmapImage(new Uri(@"D:\ブログ用\チェック用2\NEC_6221_2019_02_24_午後わてん.jpg"));

            LargeImage.Source = image;
            SmallImage.Source = image;

            MyScrollViewer.ScrollChanged += MyScrollViewer_ScrollChanged;

            ContentRendered += (s, e) =>
            {
                MyCombinedGeometry.Geometry1 = new RectangleGeometry(
                new Rect(0, 0, SmallImage.ActualWidth, SmallImage.ActualHeight));
            };

            ThumbViewport.DragDelta += ThumbViewport_DragDelta;
        }

        private void ThumbViewport_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var smallImgWidth = SmallImage.ActualWidth;
            var smallImgHeight = SmallImage.ActualHeight;

            MyScrollViewer.ScrollToHorizontalOffset(
                MyScrollViewer.HorizontalOffset + (e.HorizontalChange * MyScrollViewer.ExtentWidth / smallImgWidth));
            MyScrollViewer.ScrollToVerticalOffset(
                MyScrollViewer.VerticalOffset + (e.VerticalChange * MyScrollViewer.ExtentHeight / smallImgHeight));

            //Canvasの外に出ないように
            var setLeft = Canvas.GetLeft(ThumbViewport) + e.HorizontalChange;
            var setTop = Canvas.GetTop(ThumbViewport) + e.VerticalChange;
            var right = setLeft + ThumbViewport.ActualWidth;
            var bottom = setTop + ThumbViewport.ActualHeight;
            if (0 < setLeft && 0 < setTop && right < smallImgWidth && bottom < smallImgHeight)
            {
                Canvas.SetLeft(ThumbViewport, setLeft);
                Canvas.SetTop(ThumbViewport, setTop);
            }
                
            
        }

        private void MyScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var x = SmallImage.ActualWidth / e.ExtentWidth;
            var y = SmallImage.ActualHeight / e.ExtentHeight;

            var left = e.HorizontalOffset * x;
            var top = e.VerticalOffset * y;

            var width = e.ViewportWidth * x;
            if (width > SmallImage.ActualWidth) width = SmallImage.ActualWidth;

            var height = e.ViewportHeight * y;
            if (height > SmallImage.ActualHeight) height = SmallImage.ActualHeight;

            //Canvas.SetLeft(BorderViewport, left);
            //Canvas.SetTop(BorderViewport, top);
            Canvas.SetLeft(ThumbViewport, left);
            Canvas.SetTop(ThumbViewport, top);

            //BorderViewport.Width = width;
            //BorderViewport.Height = height;
            ThumbViewport.Width = width;
            ThumbViewport.Height = height;

            //            
            MyCombinedGeometry.Geometry2 = new RectangleGeometry(new Rect(left, top, width, height));
        }


    }
}

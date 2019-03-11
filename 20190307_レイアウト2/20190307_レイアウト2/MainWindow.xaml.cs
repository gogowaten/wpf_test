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

            MyImageMain.Source = image;
            MyImageThumb.Source = image;

            MyScrollViewer.ScrollChanged += MyScrollViewer_ScrollChanged;

            ContentRendered += (s, e) =>
            {
                MyCombinedGeometry.Geometry1 = new RectangleGeometry(
                new Rect(0, 0, MyImageThumb.ActualWidth, MyImageThumb.ActualHeight));
                
            };

            MyThumbViewport.DragDelta += ThumbViewport_DragDelta;
            
        }

        
        private void ThumbViewport_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var smallImgWidth = MyImageThumb.ActualWidth;
            var smallImgHeight = MyImageThumb.ActualHeight;

            MyScrollViewer.ScrollToHorizontalOffset(
                MyScrollViewer.HorizontalOffset + (e.HorizontalChange * MyScrollViewer.ExtentWidth / smallImgWidth));
            MyScrollViewer.ScrollToVerticalOffset(
                MyScrollViewer.VerticalOffset + (e.VerticalChange * MyScrollViewer.ExtentHeight / smallImgHeight));

            //Canvasの外に出ないように
            var setLeft = Canvas.GetLeft(MyThumbViewport) + e.HorizontalChange;
            var setTop = Canvas.GetTop(MyThumbViewport) + e.VerticalChange;
            var right = setLeft + MyThumbViewport.ActualWidth;
            var bottom = setTop + MyThumbViewport.ActualHeight;
            if (0 < setLeft && 0 < setTop && right < smallImgWidth && bottom < smallImgHeight)
            {
                Canvas.SetLeft(MyThumbViewport, setLeft);
                Canvas.SetTop(MyThumbViewport, setTop);
            }
        }

        private void MyScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var x = MyImageThumb.ActualWidth / e.ExtentWidth;
            var y = MyImageThumb.ActualHeight / e.ExtentHeight;

            var left = e.HorizontalOffset * x;
            var top = e.VerticalOffset * y;

            var width = e.ViewportWidth * x;
            if (width > MyImageThumb.ActualWidth) width = MyImageThumb.ActualWidth;

            var height = e.ViewportHeight * y;
            if (height > MyImageThumb.ActualHeight) height = MyImageThumb.ActualHeight;

            //Canvas.SetLeft(BorderViewport, left);
            //Canvas.SetTop(BorderViewport, top);
            Canvas.SetLeft(MyThumbViewport, left);
            Canvas.SetTop(MyThumbViewport, top);

            //BorderViewport.Width = width;
            //BorderViewport.Height = height;
            MyThumbViewport.Width = width;
            MyThumbViewport.Height = height;

            //            
            MyCombinedGeometry.Geometry2 = new RectangleGeometry(new Rect(left, top, width, height));
        }


    }
}

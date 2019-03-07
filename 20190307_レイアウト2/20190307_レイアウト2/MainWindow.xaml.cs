using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Controls.Primitives;

//Thumb コントロールで Photoshop のナビゲーターを再現する | grabacr.nét
//http://grabacr.net/archives/1723

//イベントをBindingにしてみたけどあんまり意味なかった
//ScrollViewerのOffsetとThumbの位置を双方向Bindingにしようとしたけどエラー、
//これはScrollViewerのOffsetPropertyが読み取り専用だから
//他にもいろいろ不具合が出そう、イベントをBindingにするのはやめたほうが良さそう

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
                MyBinding();
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
            //var setLeft = Canvas.GetLeft(ThumbViewport) + e.HorizontalChange;
            //var setTop = Canvas.GetTop(ThumbViewport) + e.VerticalChange;
            //var right = setLeft + ThumbViewport.ActualWidth;
            //var bottom = setTop + ThumbViewport.ActualHeight;
            //if (0 < setLeft && 0 < setTop && right < smallImgWidth && bottom < smallImgHeight)
            //{
            //    Canvas.SetLeft(ThumbViewport, setLeft);
            //    Canvas.SetTop(ThumbViewport, setTop);
            //}
        }

        private void MyScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //全領域＝extent、見えている領域＝Viewport
            var x = SmallImage.ActualWidth / e.ExtentWidth;
            var y = SmallImage.ActualHeight / e.ExtentHeight;

            var left = e.HorizontalOffset * x;
            var top = e.VerticalOffset * y;

            var width = e.ViewportWidth * x;
            if (width > SmallImage.ActualWidth) width = SmallImage.ActualWidth;

            var height = e.ViewportHeight * y;
            if (height > SmallImage.ActualHeight) height = SmallImage.ActualHeight;

            //Canvas.SetLeft(ThumbViewport, left);//to binding
            //Canvas.SetTop(ThumbViewport, top);

            //ThumbViewport.Width = width;//to binding
            //ThumbViewport.Height = height;

            //            
            MyCombinedGeometry.Geometry2 = new RectangleGeometry(new Rect(left, top, width, height));

        }
        private void MyBinding()
        {
            //source scrollviewer
            //target thumb
            var x = SmallImage.ActualWidth / MyScrollViewer.ExtentWidth;
            var y = SmallImage.ActualHeight / MyScrollViewer.ExtentHeight;

            var b = new Binding();
            b.Source = MyScrollViewer;
            b.Path = new PropertyPath(ScrollViewer.HorizontalOffsetProperty);
            b.Converter = new MyConverter();
            double param = x;
            b.ConverterParameter = param;
            ThumbViewport.SetBinding(Canvas.LeftProperty, b);

            b = new Binding();
            b.Source = MyScrollViewer;
            b.Path = new PropertyPath(ScrollViewer.VerticalOffsetProperty);
            b.Converter = new MyConverter();
            b.ConverterParameter = y;
            ThumbViewport.SetBinding(Canvas.TopProperty, b);

            b = new Binding();
            b.Source = MyScrollViewer;
            b.Path = new PropertyPath(ScrollViewer.ViewportWidthProperty);
            b.Converter = new MyConverter();
            b.ConverterParameter = x;
            ThumbViewport.SetBinding(WidthProperty, b);

            b = new Binding();
            b.Source = MyScrollViewer;
            b.Path = new PropertyPath(ScrollViewer.ViewportHeightProperty);
            b.Converter = new MyConverter();
            b.ConverterParameter = y;
            ThumbViewport.SetBinding(HeightProperty, b);

            var left = MyScrollViewer.HorizontalOffset * x;
            var top = MyScrollViewer.VerticalOffset * y;
            var width = MyScrollViewer.ViewportWidth * x;
            if (width > SmallImage.ActualWidth) width = SmallImage.ActualWidth;

            var height = MyScrollViewer.ViewportHeight * y;
            if (height > SmallImage.ActualHeight) height = SmallImage.ActualHeight;

            //b = new Binding();
            //b.Source = MyScrollViewer;
            //b.Path = new PropertyPath(ScrollViewer.ViewportWidthProperty);
            //b.Converter = new MyWidthConverter();
            //b.ConverterParameter = new Rect(left, top, width, height);
            //BindingOperations.SetBinding(ThumbViewport, CombinedGeometry.Geometry2Property, b);

        }

    }



    public class MyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)parameter * (double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class MyTopConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        double verticalOffset = (double)value;
    //        double top = (double)parameter * verticalOffset;
    //        return top;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class MyWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new RectangleGeometry((Rect)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

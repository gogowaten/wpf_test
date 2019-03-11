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
using System.Collections.ObjectModel;
using System.Globalization;

namespace _20190307_レイアウト
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {   
        Point MyPoint;

        public MainWindow()
        {
            InitializeComponent();
            Title = this.ToString();

            MyInitialize();

            Loaded += MainWindow_Loaded;
            ButtonViewStack.Click += ButtonViewStack_Click;
            ButtonViewParallel.Click += ButtonViewParallel_Click;
            ButtonZOrder.Click += ButtonZOrder_Click;

            MyCheckBoxVisibleThumbImage.Click += MyCheckBoxVisibleThumbImage_Click;
        }

        private void MyCheckBoxVisibleThumbImage_Click(object sender, RoutedEventArgs e)
        {
            if (MyCheckBoxVisibleThumbImage.IsChecked == true)
            {
                MyRowDifinitionThumbImage.Height = new GridLength(200);
            }
            else
            {
                MyRowDifinitionThumbImage.Height = new GridLength(0);
            }
        }




        //初期処理
        private void MyInitialize()
        {
            //表示する画像ファイルのパス
            string filePath1 = @"D:\ブログ用\チェック用2\NEC_6221_2019_02_24_午後わてん_half.jpg";
            string filePath2 = @"D:\ブログ用\チェック用2\NEC_6221_2019_02_24_午後わてん_half_16colors.png";

            //Imageに画像表示
            MyImage1.Source = new BitmapImage(new Uri(filePath1));
            MyImage2.Source = new BitmapImage(new Uri(filePath2));
            MyImageThumbnail.Source = new BitmapImage(new Uri(filePath1));


            SliderScale.ValueChanged += SliderScale_ValueChanged;
            MyScroll1.ScrollChanged += MyScroll1_ScrollChanged;
            MyScroll2.ScrollChanged += MyScroll2_ScrollChanged;
            MyImage1.MouseRightButtonDown += MyImage1_MouseRightButtonDown;
            MyImage1.MouseRightButtonUp += MyImage1_MouseRightButtonUp;
            MyImage1.MouseMove += MyImage1_MouseMove;
            MyImage2.MouseRightButtonDown += MyImage2_MouseRightButtonDown;
            MyImage2.MouseRightButtonUp += MyImage2_MouseRightButtonUp;
            MyImage2.MouseMove += MyImage2_MouseMove;
            MyImage2.MouseLeftButtonUp += MyImage_MouseLeftButtonUp;
            MyImage2.MouseLeftButtonDown += MyImage_MouseLeftButtonDown;
            MyImage1.MouseLeftButtonDown += MyImage_MouseLeftButtonDown;
            MyImage1.MouseLeftButtonUp += MyImage_MouseLeftButtonUp;

            ThumbViewport.DragDelta += ThumbViewport_DragDelta;
           

            //Make pallete
            var listB = new ListBox();
            var rand = new Random();
            var rgb = new byte[3];
            var myData = new ObservableCollection<MyData>();

            for (int i = 0; i < 24; i++)
            {
                rand.NextBytes(rgb);
                var col = Color.FromRgb(rgb[0], rgb[1], rgb[2]);
                myData.Add(new MyData() { Color = col });
                var bor = new Border()
                {
                    Width = 16,
                    Height = 16,
                    Margin = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Colors.AliceBlue),
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush(col)
                };
                StackPanel2.Children.Add(bor);                
            }
            MyListBoxPalette1.DataContext = myData;
            //var datatemp = new DataTemplate();
            //var f = new FrameworkElementFactory(typeof(StackPanel), "DataTemplate");
            //var fBorder = new FrameworkElementFactory(typeof(Border),"DataTempBorder");
            //f.AppendChild(fBorder);
            //datatemp.VisualTree = f;
            //MyListBoxPalette1.ItemTemplate = datatemp;
        }


        //ThumbViewportドラッグ移動
        private void ThumbViewport_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double imgWidth = MyImageThumbnail.ActualWidth;
            double imgHeight = MyImageThumbnail.ActualHeight;
            MyScroll1.ScrollToHorizontalOffset(
                MyScroll1.HorizontalOffset + (e.HorizontalChange * MyScroll1.ExtentWidth / imgWidth));
            MyScroll1.ScrollToVerticalOffset(
                MyScroll1.VerticalOffset + (e.VerticalChange * MyScroll1.ExtentHeight / imgHeight));

        }

        private void MyImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Image2が上だったら、Image1より下げる
            int z1 = Panel.GetZIndex(MyScroll1);
            int z2 = Panel.GetZIndex(MyScroll2);

            if (z2 >= z1)
            {
                Panel.SetZIndex(MyScroll2, z1 - 1);
            }
        }

        private void MyImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Image2が下だったら、Image1より上げる
            int z1 = Panel.GetZIndex(MyScroll1);
            int z2 = Panel.GetZIndex(MyScroll2);

            if (z2 < z1)
            {
                Panel.SetZIndex(MyScroll2, z1 + 1);
            }
        }


        private void MyImage2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                MyImage2.Cursor = Cursors.ScrollAll;
                //今のマウスの座標
                Point mouseP = e.GetPosition(this);
                //マウスの移動距離だけスクロールさせる
                SetScroll(MyScroll2, mouseP);
            }
        }
        //マウスの移動距離だけスクロールさせる
        private void SetScroll(ScrollViewer scrollViewer, Point mousePoint)
        {
            //マウスの移動距離＝直前の座標と今の座標の差
            double xd = MyPoint.X - mousePoint.X;
            double yd = MyPoint.Y - mousePoint.Y;
            //拡大表示のときはそのぶん大きくスクロールさせる
            xd *= SliderScale.Value;
            yd *= SliderScale.Value;
            //移動距離＋今のスクロール位置
            xd += scrollViewer.HorizontalOffset;
            yd += scrollViewer.VerticalOffset;
            //スクロール位置の指定
            scrollViewer.ScrollToHorizontalOffset(xd);
            scrollViewer.ScrollToVerticalOffset(yd);
            //直前のマウスカーソル座標を今の座標に変更
            MyPoint = mousePoint;
        }
        private void MyImage2_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MyImage2.ReleaseMouseCapture();
            MyImage2.Cursor = Cursors.Arrow;
        }

        private void MyImage2_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyPoint = e.GetPosition(this);
            MyImage2.CaptureMouse();
        }

        //右クリックドラッグ移動
        private void MyImage1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                MyImage1.Cursor = Cursors.ScrollAll;
                //今のマウスの座標
                Point mouseP = e.GetPosition(this);
                //マウスの移動距離だけスクロールさせる
                SetScroll(MyScroll1, mouseP);
            }
        }

        private void MyImage1_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //マウスがScrollViewer外になってもドラッグ移動を有効にしたいときだけ必要
            MyImage1.ReleaseMouseCapture();
            MyImage1.Cursor = Cursors.Arrow;
        }

        private void MyImage1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyPoint = e.GetPosition(this);
            //マウスがScrollViewer外になってもドラッグ移動を有効にしたいときだけ必要
            MyImage1.CaptureMouse();
        }



        //アプリ起動完了時に表示された画像サイズを取得してCanvasサイズに指定する
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Canvasサイズを画像のサイズにして
            //ScrollViewerによるスクロールバーを正しく表示する
            MyCanvas1.Width = MyImage1.ActualWidth;
            MyCanvas1.Height = MyImage1.ActualHeight;
            MyCanvas2.Width = MyImage2.ActualWidth;
            MyCanvas2.Height = MyImage2.ActualHeight;
            //画像の拡大方法の指定、無指定なら線形補間
            RenderOptions.SetBitmapScalingMode(MyImage1, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(MyImage2, BitmapScalingMode.NearestNeighbor);

        }


        //		UWPのScrollViewerでスクロール位置の同期を行うメモ
        //http://studio-geek.com/archives/857

        //ThumbViewportの位置とサイズを変更
        private void MyScroll2_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

            //値が双方で異なるときだけ更新
            if (e.VerticalOffset != MyScroll1.VerticalOffset | e.ExtentHeightChange != 0 | e.ViewportHeightChange != 0)
            {
                MyScroll1.ScrollToVerticalOffset(e.VerticalOffset);
                SetThumbViewportVertical(e);
            }
            if (e.HorizontalOffset != MyScroll1.HorizontalOffset | e.ExtentWidthChange != 0 | e.ViewportWidthChange != 0)
            {
                MyScroll1.ScrollToHorizontalOffset(e.HorizontalOffset);
                SetThumbViewportHorizontal(e);
            }
        }

        private void MyScroll1_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalOffset != MyScroll2.VerticalOffset | e.ExtentHeightChange != 0 | e.ViewportHeightChange != 0)
            {
                MyScroll2.ScrollToVerticalOffset(e.VerticalOffset);
                SetThumbViewportVertical(e);
            }
            if (e.HorizontalOffset != MyScroll2.HorizontalOffset | e.ExtentWidthChange != 0 | e.ViewportWidthChange != 0)
            {
                MyScroll2.ScrollToHorizontalOffset(e.HorizontalOffset);
                SetThumbViewportHorizontal(e);
            }
        }
        //ThumbViewportの位置と高さを変更、縦スクロールバー変更時
        private void SetThumbViewportVertical(ScrollChangedEventArgs e)
        {
            double y = MyImageThumbnail.ActualHeight / e.ExtentHeight;
            double top = e.VerticalOffset * y;
            double height = e.ViewportHeight * y;
            if (height > MyImageThumbnail.ActualHeight) { height = MyImageThumbnail.ActualHeight; }
            Canvas.SetTop(ThumbViewport, top);
            ThumbViewport.Height = height;
        }
        //ThumbViewportの位置と幅を変更、横スクロールバー変更時
        private void SetThumbViewportHorizontal(ScrollChangedEventArgs e)
        {
            double x = MyImageThumbnail.ActualWidth / e.ExtentWidth;
            double left = e.HorizontalOffset * x;
            double width = e.ViewportWidth * x;
            if (width > MyImageThumbnail.ActualWidth) { width = MyImageThumbnail.ActualWidth; }
            Canvas.SetLeft(ThumbViewport, left);
            ThumbViewport.Width = width;
        }


        //拡大倍率変更時はImageを乗せているCanvasのサイズを変更する
        private void SliderScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Imageが乗っかっているCanvasのサイズを変更すると
            //正しく表示され、スクロールバーも期待通りになる
            //拡大後Imageのサイズを取得
            double widht = MyImage1.ActualWidth * SliderScale.Value;
            double height = MyImage1.ActualHeight * SliderScale.Value;
            //設定
            MyCanvas1.Width = widht;
            MyCanvas1.Height = height;
            MyCanvas2.Width = widht;
            MyCanvas2.Height = height;
        }


        private void ButtonZOrder_Click(object sender, RoutedEventArgs e)
        {
            int z1 = Panel.GetZIndex(MyScroll1);
            int z2 = Panel.GetZIndex(MyScroll2);

            if (z1 > z2)
            {
                Panel.SetZIndex(MyScroll2, z2 + 1);
            }
            else
            {
                Panel.SetZIndex(MyScroll2, z2 - 1);
            }
        }

        private void ButtonViewParallel_Click(object sender, RoutedEventArgs e)
        {
            Grid.SetColumn(MyScroll2, 1);
            Grid.SetColumnSpan(MyScroll2, 1);
            Grid.SetColumnSpan(MyScroll1, 1);
            MyCanvas1.HorizontalAlignment = HorizontalAlignment.Right;
            MyCanvas2.HorizontalAlignment = HorizontalAlignment.Left;
        }

        private void ButtonViewStack_Click(object sender, RoutedEventArgs e)
        {
            Grid.SetColumn(MyScroll2, 0);
            Grid.SetColumnSpan(MyScroll1, 2);
            Grid.SetColumnSpan(MyScroll2, 2);
            MyCanvas1.HorizontalAlignment = HorizontalAlignment.Center;
            MyCanvas2.HorizontalAlignment = HorizontalAlignment.Center;
        }
    }

    public class MyData
    {
        public Color Color { get; set; }
    }

    public class MyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color c = (Color)value;
            return new SolidColorBrush(c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

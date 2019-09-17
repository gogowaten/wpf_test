using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;


//Thumbをマウスドラッグで移動

namespace _20190917_thumbリハビリ
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thumb MyThumb;
        public MainWindow()
        {
            InitializeComponent();

            //Thumb作成してCanvasに追加、サイズは100x100
            MyThumb = new Thumb
            {
                Width = 100,
                Height = 100
            };
            MyCanvas.Children.Add(MyThumb);
            Canvas.SetLeft(MyThumb, 200);
            Canvas.SetTop(MyThumb, 100);

            //DragDeltaイベント
            MyThumb.DragDelta += MyThumb_DragDelta;
            
        }

        //DragDeltaイベントで移動
        private void MyThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double yoko = e.HorizontalChange;//マウス横移動距離
            double tate = e.VerticalChange;//マウス縦移動距離
            double left = Canvas.GetLeft(MyThumb);//直前の位置、横
            double top = Canvas.GetTop(MyThumb);//直前の位置、縦
            //移動(位置指定)
            Canvas.SetLeft(MyThumb, left + yoko);
            Canvas.SetTop(MyThumb, top + tate);
        }

    }
}

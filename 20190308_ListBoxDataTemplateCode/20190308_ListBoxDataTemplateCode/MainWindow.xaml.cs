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

//【WPF】XAMLでテンプレートを定義とコードで定義は違う。 | 創造的プログラミングと粘土細工
//http://pro.art55.jp/?eid=1150138
//どうやらコードでDataTemplateを扱うのはいまいちなことみたい

namespace _20190308_ListBoxDataTemplateCode
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            for (int i = 0; i < 20; i++)
            {
                MyListbox.Items.Add("items" + i);
            }
            var fStack = new FrameworkElementFactory(typeof(StackPanel), "tStack");
            var fBorder = new FrameworkElementFactory(typeof(Border), "tBoder");
            fBorder.SetValue(Border.BackgroundProperty, new SolidColorBrush(Colors.Red));
            fBorder.SetValue(Border.WidthProperty, 20.0);
            fBorder.SetValue(HeightProperty, 20.0);
            fStack.AppendChild(fBorder);
            var dt = new DataTemplate(typeof(StackPanel));
            dt.VisualTree = fStack;
            MyListbox.ItemTemplate = dt;
            var neko = MyListbox.ApplyTemplate();
            var ts = dt.FindName("tStack", MyListbox);
            LayoutUpdated += MainWindow_LayoutUpdated;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var tst = MyListbox.ItemTemplate.FindName("tStack", MyListbox);
        }

        private void MainWindow_LayoutUpdated(object sender, EventArgs e)
        {
            MyListbox.ApplyTemplate();
            //var tst = MyListbox.ItemTemplate.FindName("tStack",this);
        }
    }
}

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
using System.IO;
using MyHSV;//参照にHSV.dllを追加してある
//WPF、Color(RGB)とHSVを相互変換するdll作ってみた、オブジェクトブラウザ使ってみた(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15380324.html

//8色に減色でも誤差拡散法を試してみた(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15385432.html

namespace _20180223_誤差拡散8色
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapSource OriginBitmap;
        string ImageFileFullPath;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;
            Button0.Click += Button0_Click;
            Button1.Click += Button1_Click;
            Button2.Click += Button2_Click;
            Button3.Click += Button3_Click;
            Button4.Click += Button4_Click;
            Button5.Click += Button5_Click;
            Button6.Click += Button6_Click;
            Button7.Click += Button7_Click;
            Button8.Click += Button8_Click;
            Button9.Click += Button9_Click;
            Button10.Click += Button10_Click;
            Button11.Click += Button11_Click;
            Button12.Click += Button12_Click;
            //RedSV();
            //ColorScaleHorizontal();
            //ColorScaleVertical();
        }






        //誤差拡散時に0以下、256以上も扱えるようにint配列を作成して
        //そこにコピーした値で誤差拡散処理をする
        //すべてのピクセルの処理を終えたら元のbyte配列に戻す
        private void ErrorDiffusion右隣だけ()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);//byte配列にCopyPixel
            //int配列作成してコピー
            int[] iPixels = new int[pixels.Length];
            for (int i = 0; i < iPixels.Length; ++i)
            {
                iPixels[i] = pixels[i];
            }
            long p = 0, pp = 0;
            int gosa = 0;
            for (int y = 0; y < h; ++y)
            {
                gosa = 0;//行が変わったら誤差をリセット
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    for (int i = 0; i < 3; ++i)
                    {
                        pp = p + i;
                        if (iPixels[pp] < 128)//128未満なら0
                        {
                            gosa += iPixels[pp];//誤差記録
                            iPixels[pp] = 0;
                        }
                        else//128以上なら255
                        {
                            gosa += (iPixels[pp] - 255);//誤差記録
                            iPixels[pp] = 255;
                        }

                        //誤差拡散、右隣へ拡散
                        if (pp + 4 < pixels.Length && x < w - 1)
                        {
                            iPixels[pp + 4] += gosa;//右隣＋誤差(誤差拡散)
                            gosa = 0;//拡散したので誤差をリセット                        
                        }
                    }
                }
            }
            //byte配列に戻す
            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = (byte)iPixels[i];
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            MyImage.Source = wb;
        }






        //フロイドステインバーグ式の誤差拡散はこれが正解
        //閾値は127.5、誤差拡散時はfloatのまま拡散、誤差記録もfloat型
        private void ErrorDiffusionFloydSteinberg()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);//byte配列にCopyPixel
            //float配列作成してコピー
            float[] iPixels = new float[pixels.Length];
            for (int i = 0; i < iPixels.Length; ++i)
            {
                iPixels[i] = pixels[i];
            }
            long p = 0, pp = 0;//判定中ピクセルの配列の中での位置
            float gosa = 0;//誤差記録用もfloat
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);//PixelFormat.Pbgra32の色の並びはBGRA
                    for (int i = 0; i < 3; ++i)//0から2までの3ループはBGR
                    {
                        pp = p + i;
                        if (iPixels[pp] < 127.5f)//127.5未満なら0
                        {
                            gosa = iPixels[pp];//誤差記録
                            iPixels[pp] = 0;
                        }
                        else//127.5以上なら255
                        {
                            gosa = (iPixels[pp] - 255f);//誤差記録
                            iPixels[pp] = 255;
                        }

                        //誤差拡散、Floyd Steinberg式
                        if (pp + 4 < pixels.Length && x < w - 1)
                        {
                            iPixels[pp + 4] += (gosa / 16f) * 7f;//右隣＋誤差(誤差拡散)                        
                        }
                        if (y < h - 1)//1行下
                        {
                            if (x != 0)
                            {
                                iPixels[pp + stride - 4] += (gosa / 16f) * 3f;//左下
                            }

                            iPixels[pp + stride] += (gosa / 16f) * 5f;//真下

                            if (x < w - 1)
                            {
                                iPixels[pp + stride + 4] += (gosa / 16f) * 1f;//右下
                            }
                        }
                        gosa = 0;//拡散させたので誤差をリセット
                    }
                }
            }
            //byte配列に戻す
            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = (byte)(iPixels[i]);
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            MyImage.Source = wb;
        }

        /// <summary>
        /// PixelFormat.Pbgra32のBitmapSourceを8色に減色する
        /// 8色はRGBそれぞれを閾値127.5で0か255にするので白、黒、赤、緑、青、黄色、水色、赤紫
        /// ディザリング方式はFloydSteinberg
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private BitmapSource ErrorDiffusionFloydSteinberg(BitmapSource source)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);//byte配列にCopyPixel
            //float配列作成してコピー
            float[] iPixels = new float[pixels.Length];
            for (int i = 0; i < iPixels.Length; ++i)
            {
                iPixels[i] = pixels[i];
            }
            long p = 0, pp = 0;//判定中ピクセルの配列の中での位置
            float gosa = 0;//誤差記録用もfloat
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);//PixelFormat.Pbgra32の色の並びはBGRA
                    for (int i = 0; i < 3; ++i)//0から2までの3ループはBGR
                    {
                        pp = p + i;
                        if (iPixels[pp] < 127.5f)//127.5未満なら0
                        {
                            gosa = iPixels[pp];//誤差記録
                            iPixels[pp] = 0;
                        }
                        else//127.5以上なら255
                        {
                            gosa = (iPixels[pp] - 255f);//誤差記録
                            iPixels[pp] = 255;
                        }

                        //誤差拡散、Floyd Steinberg式
                        if (pp + 4 < pixels.Length && x < w - 1)
                        {
                            iPixels[pp + 4] += (gosa / 16f) * 7f;//右隣＋誤差(誤差拡散)                        
                        }
                        if (y < h - 1)//1行下
                        {
                            if (x != 0)
                            {
                                iPixels[pp + stride - 4] += (gosa / 16f) * 3f;//左下
                            }

                            iPixels[pp + stride] += (gosa / 16f) * 5f;//真下

                            if (x < w - 1)
                            {
                                iPixels[pp + stride + 4] += (gosa / 16f) * 1f;//右下
                            }
                        }
                        gosa = 0;//拡散させたので誤差をリセット
                    }
                }
            }
            //byte配列に戻す
            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = (byte)(iPixels[i]);
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return wb;
        }


        private void ErrorDiffusionSierraLite()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);//byte配列にCopyPixel
            //float配列作成してコピー
            float[] iPixels = new float[pixels.Length];
            for (int i = 0; i < iPixels.Length; ++i)
            {
                iPixels[i] = pixels[i];
            }
            long p = 0, pp = 0;//判定中ピクセルの配列の中での位置
            float gosa = 0;//誤差記録用もfloat
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    for (int i = 0; i < 3; ++i)
                    {
                        pp = p + i;
                        if (iPixels[pp] < 127.5f)//127.5未満なら0(黒)
                        {
                            gosa = iPixels[pp];//誤差記録
                            iPixels[pp] = 0;
                        }
                        else//127.5以上なら255(白)
                        {
                            gosa = (iPixels[pp] - 255f);//誤差記録
                            iPixels[pp] = 255;
                        }

                        //誤差拡散、SierraLite式
                        if (pp + 4 < pixels.Length && x < w - 1)
                        {
                            iPixels[pp + 4] += (gosa / 4f) * 2f;//右隣＋誤差(誤差拡散)                        
                        }
                        if (y < h - 1 && x != 0)
                        {
                            iPixels[pp + stride - 4] += (gosa / 4f) * 1f;//左下
                        }
                        if (y < h - 1)
                        {
                            iPixels[pp + stride] += (gosa / 4f) * 1f;//真下
                        }
                        gosa = 0;//拡散させたので誤差をリセット
                    }
                }
            }
            //byte配列に戻す
            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = (byte)(iPixels[i]);
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            MyImage.Source = wb;
        }

        //処理の順番を蛇行、偶数行は右へ、奇数行は左へ
        private void ErrorDiffusionFloydSteinbergSerpentine()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);//byte配列にCopyPixel
            //float配列作成してコピー
            float[] iPixels = new float[pixels.Length];
            for (int i = 0; i < iPixels.Length; ++i)
            {
                iPixels[i] = pixels[i];
            }
            long p = 0, pp = 0;//判定中ピクセルの配列の中での位置
            float gosa = 0;//誤差記録用もfloat
            for (int y = 0; y < h; ++y)
            {
                if (y % 2 == 0)//偶数行
                {
                    for (int x = 0; x < w; ++x)
                    {
                        p = y * stride + (x * 4);
                        for (int i = 0; i < 3; ++i)
                        {
                            pp = p + i;
                            gosa = 0;//誤差をリセット
                            if (iPixels[pp] < 127.5f)//127.5未満なら0(黒)
                            {
                                gosa = iPixels[pp];//誤差記録
                                iPixels[pp] = 0;
                            }
                            else//127.5以上なら255(白)
                            {
                                gosa = (iPixels[pp] - 255f);//誤差記録
                                iPixels[pp] = 255;
                            }

                            //誤差拡散、Floyd Steinberg式
                            if (pp + 4 < pixels.Length && x < w - 1)
                            {
                                iPixels[pp + 4] += (gosa / 16f) * 7f;//右隣＋誤差(誤差拡散)                        
                            }
                            if (y < h - 1)
                            {
                                if (x != 0)
                                {
                                    iPixels[pp + stride - 4] += (gosa / 16f) * 3f;//左下
                                }

                                iPixels[pp + stride] += (gosa / 16f) * 5f;//真下

                                if (x < w - 1)
                                {
                                    iPixels[pp + stride + 4] += (gosa / 16f) * 1f;//右下
                                }
                            }
                        }
                    }
                }
                else//奇数行
                {
                    for (int x = w - 1; x >= 0; --x)
                    {
                        p = y * stride + (x * 4);
                        for (int i = 0; i < 3; ++i)
                        {
                            pp = p + i;
                            gosa = 0;//誤差をリセット
                            if (iPixels[pp] < 127.5f)//127.5未満なら0(黒)
                            {
                                gosa = iPixels[pp];//誤差記録
                                iPixels[pp] = 0;
                            }
                            else//127.5以上なら255(白)
                            {
                                gosa = (iPixels[pp] - 255f);//誤差記録
                                iPixels[pp] = 255;
                            }

                            //誤差拡散、Floyd Steinberg式
                            if (pp - 4 < pixels.Length && x != 0)//左隣
                            {
                                iPixels[pp - 4] += (gosa / 16f) * 7f;
                            }
                            if (y < h - 1)
                            {
                                if (x != 0)
                                {
                                    iPixels[pp + stride - 4] += (gosa / 16f) * 1f;//左下
                                }

                                iPixels[pp + stride] += (gosa / 16f) * 5f;//真下

                                if (x < w - 1)
                                {
                                    iPixels[pp + stride + 4] += (gosa / 16f) * 3f;//右下
                                }
                            }
                        }
                    }
                }
            }
            //byte配列に戻す
            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = (byte)(iPixels[i]);
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            MyImage.Source = wb;
        }




        //Jarvis, Judice and Ninke dithering
        //      [ ] 7  5
        // 3  5  7  5  3
        // 1  3  5  3  1
        private void ErrorDiffusionJaJuNi()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);//byte配列にCopyPixel
            //float配列作成してコピー
            float[] iPixels = new float[pixels.Length];
            for (int i = 0; i < iPixels.Length; ++i)
            {
                iPixels[i] = pixels[i];
            }
            long p = 0, ap = 0;//判定中ピクセルの配列の中での位置
            float gosa = 0;//誤差記録用もfloat
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    for (int i = 0; i < 3; ++i)
                    {
                        ap = p + i;
                        gosa = 0;//誤差をリセット
                        if (iPixels[ap] < 127.5f)//127.5未満なら0(黒)
                        {
                            gosa = iPixels[ap];//誤差記録
                            iPixels[ap] = 0;
                        }
                        else//127.5以上なら255(白)
                        {
                            gosa = (iPixels[ap] - 255f);//誤差記録
                            iPixels[ap] = 255;
                        }

                        //誤差拡散
                        if (x < w - 1)//右隣
                        {
                            iPixels[ap + 4] += (gosa / 48f) * 7f;//右隣＋誤差(誤差拡散)                        
                        }
                        if (x < w - 2)//2つ右
                        {
                            iPixels[ap + 8] += (gosa / 48f) * 5f;//右隣＋誤差(誤差拡散)                        
                        }

                        //下段
                        if (y < h - 1)
                        {
                            long pp = ap + stride;
                            if (x > 1)//2つ左
                            {
                                iPixels[pp - 8] += (gosa / 48f) * 3f;
                            }
                            if (x != 0)
                            {
                                iPixels[pp - 4] += (gosa / 48f) * 5f;//左下
                            }

                            iPixels[pp] += (gosa / 48f) * 7f;//真下

                            if (x < w - 1)
                            {
                                iPixels[pp + 4] += (gosa / 48f) * 5f;//右下
                            }
                            if (x < w - 2)
                            {
                                iPixels[pp + 8] += (gosa / 48f) * 3f;//右下
                            }
                        }

                        //2行下
                        if (y < h - 2)
                        {
                            long pp = ap + (stride * 2);
                            if (x > 1)//2つ左
                            {
                                iPixels[pp - 8] += (gosa / 48f) * 1f;
                            }
                            if (x != 0)
                            {
                                iPixels[pp - 4] += (gosa / 48f) * 3f;//左下
                            }

                            iPixels[pp] += (gosa / 48f) * 5f;//真下

                            if (x < w - 1)
                            {
                                iPixels[pp + 4] += (gosa / 48f) * 3f;//右下
                            }
                            if (x < w - 2)
                            {
                                iPixels[pp + 8] += (gosa / 48f) * 1f;//右下
                            }
                        }

                    }


                }
            }
            //byte配列に戻す
            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = (byte)(iPixels[i]);
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            MyImage.Source = wb;
        }

        private void ErrorDiffusionAtkinson()
        {
            var wb = new WriteableBitmap(OriginBitmap);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);//byte配列にCopyPixel
            //float配列作成してコピー
            float[] iPixels = new float[pixels.Length];
            for (int i = 0; i < iPixels.Length; ++i)
            {
                iPixels[i] = pixels[i];
            }
            long p = 0, pp = 0;//判定中ピクセルの配列の中での位置
            float gosa = 0;//誤差記録用もfloat
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    gosa = 0;//誤差をリセット
                    for (int i = 0; i < 3; ++i)
                    {
                        pp = p + i;
                        if (iPixels[pp] < 127.5f)//127.5未満なら0(黒)
                        {
                            gosa = iPixels[pp];//誤差記録
                            iPixels[pp] = 0;
                        }
                        else//127.5以上なら255(白)
                        {
                            gosa = (iPixels[pp] - 255f);//誤差記録
                            iPixels[pp] = 255;
                        }

                        //誤差拡散、Floyd Steinberg式
                        if (x < w - 2)
                        {
                            iPixels[pp + 8] += (gosa / 8f) * 1f;//2つ右
                        }
                        if (x < w - 1)
                        {
                            iPixels[pp + 4] += (gosa / 8f) * 1f;//右隣＋誤差(誤差拡散)                        
                        }
                        if (y < h - 1 && x != 0)
                        {
                            iPixels[pp + stride - 4] += (gosa / 8f) * 1f;//左下
                        }
                        if (y < h - 1)
                        {
                            iPixels[pp + stride] += (gosa / 8f) * 1f;//真下
                        }
                        if (y < h - 1 && x < w - 1)
                        {
                            iPixels[pp + stride + 4] += (gosa / 8f) * 1f;//右下
                        }
                        if (y < h - 2)
                        {
                            iPixels[pp + stride * 2] += (gosa / 8f) * 1f;//2つ下
                        }
                    }
                }
            }
            //byte配列に戻す
            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = (byte)(iPixels[i]);
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            MyImage.Source = wb;
        }

        private void BlackWhite()
        {
            MyImage.Source = new FormatConvertedBitmap(OriginBitmap, PixelFormats.BlackWhite, null, 0);
        }

        private void ToIndexed2()
        {
            MyImage.Source = new FormatConvertedBitmap(OriginBitmap, PixelFormats.Indexed2, null, 0);
        }

        private void ToIndexed4()
        {
            MyImage.Source = new FormatConvertedBitmap(OriginBitmap, PixelFormats.Indexed4, null, 0);
        }

        //HSVで赤のグラデーションの四角形
        private void RedSV()
        {
            int reso = 256;
            Color color;
            var wb = new WriteableBitmap(reso, reso, 96, 96, PixelFormats.Pbgra32, null);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            int p = 0;
            for (int y = 0; y < wb.PixelHeight; ++y)
            {
                for (int x = 0; x < wb.PixelWidth; ++x)
                {
                    p = y * stride + (x * 4);
                    color = HSV.HSV2Color(0, x / (reso - 1f), y / (reso - 1f));
                    pixels[p + 3] = 255;
                    pixels[p + 2] = color.R;
                    pixels[p + 1] = color.G;
                    pixels[p + 0] = color.B;
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            OriginBitmap = wb;
            MyImage.Source = wb;
        }

        //カラースケール横
        private void ColorScaleHorizontal()
        {
            int reso = 100;
            Color color;
            var wb = new WriteableBitmap(reso, 32, 96, 96, PixelFormats.Pbgra32, null);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            int p = 0;

            for (int x = 0; x < wb.PixelWidth; ++x)
            {
                color = HSV.HSV2Color(x * 360 / reso, 1f, 1f);
                for (int y = 0; y < wb.PixelHeight; ++y)
                {
                    p = y * stride + (x * 4);
                    pixels[p + 3] = 255;
                    pixels[p + 2] = color.R;
                    pixels[p + 1] = color.G;
                    pixels[p + 0] = color.B;
                }
            }

            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            OriginBitmap = wb;
            MyImage.Source = wb;
        }

        private void ColorScaleVertical()
        {
            int reso = 360;
            Color color;
            var wb = new WriteableBitmap(32, reso, 96, 96, PixelFormats.Pbgra32, null);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            int p = 0;

            for (int y = 0; y < wb.PixelHeight; ++y)
            {
                color = HSV.HSV2Color(y * 360 / reso, 1f, 1f);
                for (int x = 0; x < wb.PixelWidth; ++x)
                {
                    p = y * stride + (x * 4);
                    pixels[p + 3] = 255;
                    pixels[p + 2] = color.R;
                    pixels[p + 1] = color.G;
                    pixels[p + 0] = color.B;
                }
            }

            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            OriginBitmap = wb;
            MyImage.Source = wb;
        }

        #region イベント

        private void Button12_Click(object sender, RoutedEventArgs e)
        {         
            ColorScaleVertical();
        }

        private void Button11_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            ErrorDiffusionAtkinson();
        }

        private void Button10_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            ErrorDiffusionJaJuNi();
        }

        private void Button9_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            ErrorDiffusionFloydSteinbergSerpentine();
        }

        private void Button8_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            ErrorDiffusionSierraLite();
        }

        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            //if (OriginBitmap == null) { return; }
            RedSV();
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            ToIndexed4();
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            ToIndexed2();
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            BlackWhite();
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            ErrorDiffusionFloydSteinberg();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            //ErrorDiffusionFloydIntger();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            ErrorDiffusion右隣だけ();
            //Gosakakusan();
            //Gosakkakusan2();
            //Gosakkakusan3_左右ループ_切り捨て();
        }

        private void Button0_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = OriginBitmap;
        }


        //画像ファイルドロップ時
        //PixelFormatをPbgra32に変換してBitmapSource取得
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            //OriginBitmap = GetBitmapSourceWithChangePixelFormat2(filePath[0], PixelFormats.Gray8, 96, 96);
            OriginBitmap = GetBitmapSourceWithChangePixelFormat2(filePath[0], PixelFormats.Pbgra32, 96, 96);
            if (OriginBitmap == null)
            {
                MessageBox.Show("not Image");
            }
            else
            {
                MyImage.Source = OriginBitmap;
                ImageFileFullPath = System.IO.Path.GetFullPath(filePath[0]);//ファイルのフルパス保持
            }
        }
        #endregion

        private BitmapSource GetBitmapSourceWithChangePixelFormat2(
            string filePath, PixelFormat pixelFormat, double dpiX = 0, double dpiY = 0)
        {
            BitmapSource source = null;
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var bf = BitmapFrame.Create(fs);
                    var convertedBitmap = new FormatConvertedBitmap(bf, pixelFormat, null, 0);
                    int w = convertedBitmap.PixelWidth;
                    int h = convertedBitmap.PixelHeight;
                    int stride = (w * pixelFormat.BitsPerPixel + 7) / 8;
                    byte[] pixels = new byte[h * stride];
                    convertedBitmap.CopyPixels(pixels, stride, 0);
                    //dpi指定がなければ元の画像と同じdpiにする
                    if (dpiX == 0) { dpiX = bf.DpiX; }
                    if (dpiY == 0) { dpiY = bf.DpiY; }
                    //dpiを指定してBitmapSource作成
                    source = BitmapSource.Create(
                        w, h, dpiX, dpiY,
                        convertedBitmap.Format,
                        convertedBitmap.Palette, pixels, stride);
                };
            }
            catch (Exception)
            {

            }

            return source;
        }
    }
}

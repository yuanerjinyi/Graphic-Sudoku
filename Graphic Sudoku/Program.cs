using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using OpenCvSharp;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Threading;
using static System.Console;

namespace ConsoleApp1
{
    //用坍缩算法对数独进行求解，数独用矩阵数组表示，矩阵的维度是3，一二维分别表示数独的行列，第三维度的数据长度为12，
    //其中索引0表示当前位置块解得的数字，索引1-10表示待求位置可以填充的数字，索引11表示待求位置可以填入数字的可能个数。
    //对每个待求解的位置块做如下处理，对所有待求解位置块的第三维的索引11的数据进行升序处理，对求得的升序列进行遍历。升序历中是
    //下一次代求块的位置，在遍历中获取当前位置块的剩余可能解（可能填入的数字），对可能解进行遍历，一旦填入的数字满足要求，又将获取待解块的
    //的升序历，循环往复进行求解。直至数独中的所有块都填入数字并满足要求，数独求解完成。
    class Sudoku
    {
        int[,,] Su_matrix = new int[10, 10, 12]; //定义3维数独矩阵
        List<int> Decomposed_layer = new List<int>();
        static int width = 500;                //画布宽高
        static int height = 500;               //
        string wind = "数独";                  //图像显示窗口名称

        static int width1 = 1756;                //画布宽高
        static int height1 = 451;               //
        string wind1 = "数独求解过程";                  //图像显示窗口名称

        // 创建一个黑色的空白图像
        Mat img = new Mat(new Size(width, height), MatType.CV_8UC3, new Scalar(0, 0, 0));
        Mat img1 = new Mat(new Size(width1, height1), MatType.CV_8UC3, new Scalar(0, 0, 0));
        //新建一个显示窗口，并移动窗口到指点位置
        private void Img_show()
        {

            Cv2.NamedWindow(wind, WindowFlags.AutoSize);
            Cv2.MoveWindow(wind, 300, 0);

            Cv2.NamedWindow(wind1, WindowFlags.AutoSize);
            Cv2.ImShow(wind1, img1);
            Cv2.MoveWindow(wind1, 0, 300);

        }
        //显示数独图像
        private void Print_img()
        {
            Scalar Background = new Scalar(24, 215, 24);
            Scalar Wireframe_color = new Scalar(114, 25, 248);
            Scalar Numeral_color = new Scalar(255, 17, 17);
            int bh;
            img.SetTo(Background);
            float kuandu = (float)(height - 1) / 10;
            //int zb = (int)(i * kuandu);
            for (int i = 0; i <= 10; i++)
            {

                //WriteLine($"{kuandu,-8}{i*kuandu}");
                Cv2.Line(img, new Point(0, i * kuandu), new Point(499, i * kuandu), Wireframe_color, thickness: 1);
                Cv2.Line(img, new Point(i * kuandu, 0), new Point(i * kuandu, 499), Wireframe_color, thickness: 1);
            }
            for (int i = 0; i < Su_matrix.GetLength(0); i++)
            {
                for (int j = 0; j < Su_matrix.GetLength(1); j++)
                {
                    if (Su_matrix[i, j, 0] != 0)
                    {
                        Size zwh = Cv2.GetTextSize($"{Su_matrix[i, j, 0]}", HersheyFonts.HersheyComplex, 1, 1, out bh);
                        Cv2.PutText(img, $"{Su_matrix[i, j, 0]}", new Point((2 * j + 1) * kuandu / 2 - zwh.Width / 2 - 4, (2 * i + 1) * kuandu / 2 + zwh.Height / 2), HersheyFonts.HersheyComplex, 1, Numeral_color);

                    }
                }
            }

            //WriteLine($"绘制的文字宽高{zwh}\t字的基线{bh}");
            Cv2.ImShow(wind, img);
            Cv2.WaitKey(1);
        }
        //显示方法，用颜色代替数字，并显示出来
        private void Print_img1()
        {
            Scalar Background = new Scalar(0, 0, 0);
            Scalar Wireframe_color = new Scalar(255, 255, 255);
            int bh;
            List<Scalar> colors = new List<Scalar>
            {
            new Scalar(0, 0, 255),   // 红色
            new Scalar(255, 0, 0),   // 蓝色
            new Scalar(0, 255, 0),   // 绿色
            new Scalar(0, 255, 255), // 黄色
            new Scalar(0, 165, 255), // 橙色
            new Scalar(128, 0, 128), // 紫色
            new Scalar(255, 255, 0), // 青色
            new Scalar(255, 0, 255), // 粉红色
            new Scalar(19, 69, 139), // 棕色
            new Scalar(128, 128, 0)  // 青绿色
            };
            img.SetTo(Background);
            float kuandu = (float)(height - 1) / 10;
            //int zb = (int)(i * kuandu);
            for (int i = 0; i <= 10; i++)
            {

                //WriteLine($"{kuandu,-8}{i*kuandu}");
                Cv2.Line(img, new Point(0, i * kuandu), new Point(499, i * kuandu), Wireframe_color, thickness: 2);
                Cv2.Line(img, new Point(i * kuandu, 0), new Point(i * kuandu, 499), Wireframe_color, thickness: 2);
            }
            for (int i = 0; i < Su_matrix.GetLength(0); i++)
            {
                for (int j = 0; j < Su_matrix.GetLength(1); j++)
                {
                    if (Su_matrix[i, j, 0] != 0)
                    {


                        Rect region = new Rect((int)(j * kuandu), (int)(i * kuandu), (int)kuandu, (int)kuandu); // (x, y, 宽度, 高度)
                        img.Rectangle(region, colors[Su_matrix[i, j, 0] - 1], -1); // 在蒙版上绘制一个填充矩形
                    }
                }
            }

            //WriteLine($"绘制的文字宽高{zwh}\t字的基线{bh}");
            Cv2.ImShow(wind, img);
            Cv2.WaitKey(1);
        }
        public void Print_img2()
        {
            Scalar Background = new Scalar(0, 0, 0);
            Scalar Wireframe_color = new Scalar(255, 255, 255);
            int bh;
            List<Scalar> colors = new List<Scalar>
            {
            new Scalar(0, 0, 255),   // 红色
            new Scalar(255, 0, 0),   // 蓝色
            new Scalar(0, 255, 0),   // 绿色
            new Scalar(0, 255, 255), // 黄色
            new Scalar(0, 165, 255), // 橙色
            new Scalar(128, 0, 128), // 紫色
            new Scalar(255, 255, 0), // 青色
            new Scalar(255, 0, 255), // 粉红色
            new Scalar(19, 69, 139), // 棕色
            new Scalar(128, 128, 0)  // 青绿色
            };
            img.SetTo(Background);
            int kuandu = (height - 1) / 10;
            //int zb = (int)(i * kuandu);
            for (int i = 0; i <= 10; i++)
            {

                //WriteLine($"{kuandu,-8}{i*kuandu}");
                Cv2.Line(img, new Point(0, i * kuandu), new Point(499, i * kuandu), Wireframe_color, thickness: 2);
                Cv2.Line(img, new Point(i * kuandu, 0), new Point(i * kuandu, 499), Wireframe_color, thickness: 2);
            }
            for (int i = 0; i < Su_matrix.GetLength(0); i++)
            {
                for (int j = 0; j < Su_matrix.GetLength(1); j++)
                {
                    if (Su_matrix[i, j, 0] != 0)
                    {


                        Rect region = new Rect((int)(j * kuandu), (int)(i * kuandu), (int)kuandu, (int)kuandu); // (x, y, 宽度, 高度)
                        img.Rectangle(region, colors[Su_matrix[i, j, 0] - 1], -1); // 在蒙版上绘制一个填充矩形
                    }
                }
            }

            //WriteLine($"绘制的文字宽高{zwh}\t字的基线{bh}");
            Cv2.ImShow(wind, img);
            Cv2.WaitKey(500);
        }
        public void Img()
        {
            Img_show();
            Print_img();

        }
        //数字填充判定，当在待解块内填入一个数字，用此方法对当前数字进行判定，是否满足要求
        private bool Pdetermined(int shu, int[] dw, int[,,] matrix)
        {
            bool bz = true;
            int weidu = matrix.Length;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                if (i != dw[1] && matrix[dw[0], i, 0] != 0)
                {
                    if (matrix[dw[0], i, 0] == shu)
                        return false;

                }
            }
            for (int i = 0; i < matrix.GetLength(1); i++)
            {
                if (i != dw[0] && matrix[i, dw[1], 0] != 0)
                {
                    if (matrix[i, dw[1], 0] == shu)
                        return false;

                }
            }
            return true;
        }
        //数独求解主方法，调用此方法，程序开始求解数独
        public void Sudoku_main()
        {
            Img_show();
            List<student> zxk = new List<student>();
            Init();

            Min_value(zxk, Su_matrix);
            bool bz = true;
            WriteLine("数独初始化");

            Print();
            //int currentRow = Console.CursorTop;
            //Console.WriteLine($"当前输出行的位置是第 {currentRow + 1} 行。");
            foreach (var i in zxk)
            {
                if (Recursive_solving(new[] { i.AgeSerial_number / 10, i.AgeSerial_number % 10 }, Su_matrix))
                {
                    bz = false;
                    WriteLine($"数独解算成功");
                    Print();
                    Cv2.WaitKey(10000);
                    Cv2.DestroyWindow(wind);
                    break;
                }

            }
            if (bz)
                WriteLine("数独求解失败");

        }
        //递归求解模块（核心），利用递归对数独的每个块进行求解
        private bool Recursive_solving(int[] dw, int[,,] matrix)
        {
            Decomposed_layer.Add(dw[0] * 10 + dw[1]);
            Console.SetCursorPosition(0, 552);
            Print();
            //Thread.Sleep(33);
            ////Clear();
            int dqz = matrix[dw[0], dw[1], 11];
            //int[] Dw = new int[2];
            //Array.Copy(dw, Dw, dw.Length);
            List<int> djz = new List<int>();
            List<int> csz = new List<int>();
            List<student> zxk = new List<student>();

            Residual_value(djz, dw, matrix);
            foreach (var i in djz)
            {
                if (Pdetermined(i, dw, matrix))
                {
                    Number_fill(i, dw, csz, matrix);
                    Print_img1();
                    if (Remaining_blocks(matrix))
                        return true;
                    Min_value(zxk, matrix);
                    foreach (var j in zxk)
                    {

                        if (Recursive_solving(new[] { j.AgeSerial_number / 10, j.AgeSerial_number % 10 }, matrix))
                            return true;
                    }
                    zxk.Clear();
                    Reduction(i, dw, csz, matrix);
                }
            }
            Number_restores(dqz, dw, matrix);
            Decomposed_layer.RemoveAt(Decomposed_layer.Count - 1);
            return false;
        }
        private bool Remaining_blocks(int[,,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j, 0] == 0)
                        return false;
                }
            }
            return true;
        }
        //当前块的数值填充，当前块填入数字经判定满足要求后，将该数字填入当前块
        private void Number_fill(int shu, int[] dw, List<int> jl, int[,,] matrix)
        {
            matrix[dw[0], dw[1], 0] = shu;
            matrix[dw[0], dw[1], shu] = 0;
            matrix[dw[0], dw[1], 11] = 0;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                if (i != dw[1] && matrix[dw[0], i, 11] != 0 && matrix[dw[0], i, shu] != 0)
                {
                    matrix[dw[0], i, shu] = 0;
                    matrix[dw[0], i, 11]--;
                    jl.Add(dw[0] * 10 + i);
                }
            }
            for (int i = 0; i < matrix.GetLength(1); i++)
            {
                if (i != dw[0] && matrix[i, dw[1], 11] != 0 && matrix[i, dw[1], shu] != 0)
                {
                    matrix[i, dw[1], shu] = 0;
                    matrix[i, dw[1], 11]--;
                    jl.Add(i * 10 + dw[1]);
                }
            }
        }
        //将当前块数据还原
        private void Reduction(int shu, int[] dw, List<int> jl, int[,,] matrix)
        {
            matrix[dw[0], dw[1], 0] = 0;
            matrix[dw[0], dw[1], shu] = shu;
            matrix[dw[0], dw[1], 11] = 0;
            foreach (var i in jl)
            {
                matrix[i / 10, i % 10, shu] = shu;
                matrix[i / 10, i % 10, 11]++;
            }
            jl.Clear();
        }
        //将当前块可能解的个数还原
        private void Number_restores(int shu, int[] dw, int[,,] matrix)
        {
            matrix[dw[0], dw[1], 11] = shu;

        }
        //返回当前块可能解的数值列表
        private void Residual_value(List<int> li, int[] dw, int[,,] matrix)
        {
            int rand;
            int[] cs = new int[10];
            do
            {
                rand = rand10();
                for (int j = 0; j < cs.Length; j++)
                {
                    if (cs[j] == 0)
                    {
                        cs[j] = rand;
                        if (matrix[dw[0], dw[1], rand] != 0)
                            li.Add(rand);
                        break;
                    }
                    else if (cs[j] == rand)
                    {
                        j = -1;
                        rand = rand10();
                    }
                }
            } while (li.Count != matrix[dw[0], dw[1], 11]);
        }
        //返回下一次待解块的有序升序列
        private void Min_value(List<student> li, int[,,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j, 0] == 0)
                        li.Add(new student(matrix[i, j, 11], i * 10 + j));
                }
            }
            li.Sort(new stupara());

        }


        public bool Collapse_algorithm(int[] dw, ref int[,,] matrix)
        {


            return true;
        }
        //0-99范围内的随机数生成
        public byte rangd100()
        {
            byte[] randomBytes = new byte[1];
            byte Max = (byte.MaxValue / 100) * 100;
            RNGCryptoServiceProvider rngServiceProvider = new RNGCryptoServiceProvider();
            do
            {
                rngServiceProvider.GetBytes(randomBytes);

            } while (!(randomBytes[0] < Max));


            return (byte)(randomBytes[0] % 100);
        }
        //1-10 随机数生成
        public byte rand10()
        {
            byte[] randomBytes = new byte[1];
            byte Max = (byte.MaxValue / 10) * 10;
            RNGCryptoServiceProvider rngServiceProvider = new RNGCryptoServiceProvider();
            do
            {
                rngServiceProvider.GetBytes(randomBytes);

            } while (!(randomBytes[0] < Max));


            return (byte)((byte)(randomBytes[0] % 10) + 1);
        }
        //数独初始化，在数独内随机选取10个块，填入数字，进行数独的初始化
        public void Init()
        {
            byte[] Sjcs = new byte[10];
            bool bz = true;
            for (int i = 0; i < Su_matrix.GetLength(0); i++)
            {
                for (int j = 0; j < Su_matrix.GetLength(1); j++)
                {
                    for (int k = 0; k < Su_matrix.GetLength(2); k++)
                    {
                        if (k == Su_matrix.GetLength(2) - 1)
                            Su_matrix[i, j, k] = 10;
                        else
                            Su_matrix[i, j, k] = k;
                    }
                }
            }
            for (int i = 0; i < Sjcs.Length;)
            {
                Sjcs[i] = rangd100();
                int[] dw = new int[2] { Sjcs[i] / 10, Sjcs[i] % 10 };
                int rand;
                int[] cs = new int[10];
                List<int> libi = new List<int>();
                for (int j = 0; j < i; j++)
                {
                    if (Sjcs[i] == Sjcs[j])
                        continue;
                }
                do
                {
                    rand = rand10();
                    for (int j = 0; j < cs.Length; j++)
                    {
                        if (cs[j] == 0)
                        {
                            cs[j] = rand;
                            break;
                        }
                        else if (cs[j] == rand)
                        {
                            j = -1;
                            rand = rand10();
                        }
                    }
                    //Su_matrix[dw[0], dw[1], 0] = rand;
                } while (!Pdetermined(rand, dw, Su_matrix));
                Number_fill(rand, dw, libi, Su_matrix);
                i++;
            }
            //foreach (var az in randomBytes)
            ////WriteLine($"{az}");
            //Int32 result = BitConverter.ToInt32(randomBytes, 0);
            //WriteLine($"{result}");


        }
        //数独的显示，此方法将数独输出到控制台并实时刷新显示
        public void Print()
        {
            for (int i = 0; i < Su_matrix.GetLength(0); i++)
            {
                Write($"{" ",40}");
                for (int j = 0; j < Su_matrix.GetLength(1); j++)
                {
                    if (Su_matrix[i, j, 0] == 0)
                        Write($"{" ",4}");
                    else
                        Write($"{Su_matrix[i, j, 0],4}");
                }
                WriteLine("\n");
            }

        }


    }
    //定义的数据类，该类用于存储数独块的待解数字个数和块的序号
    class student
    {
        public int remainder;
        public int AgeSerial_number;
        public student(int remainder, int AgeSerial_number)
        {
            this.remainder = remainder;
            this.AgeSerial_number = AgeSerial_number;
        }
        
    }
    //比较函数 用于student类的自定义比较函数
    class stupara : IComparer<student>
    {
        public int Compare(student x, student y)
        {

            return x.remainder.CompareTo(y.remainder);

        }
    }
   
    
    class Program
    {
        static void Main(string[] args)
        {
            

           
            Sudoku sdsl = new Sudoku();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            sdsl.Sudoku_main();
            sw.Stop();
            Console.WriteLine(sw.Elapsed);

            Console.ReadKey();

        }
    }
}

using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Differential
{
    public partial class MainWindow : Window
    {
        double A0, B0, k, k1;
        private List<double> t=new(), A=new(), B=new();
        Polyline ALine=new(),BLine=new();
        private DispatcherTimer timer=new();
        private int currentIndex=0;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                A0 = double.Parse(A0_Box.Text);
                B0 = double.Parse(B0_Box.Text);
                k = double.Parse(k_Box.Text);
                k1 = double.Parse(k1_Box.Text);
                (t,A,B)=Solve();
                GraphCanvas.Children.Clear();
                DrawAxes();
                ALine = new Polyline { Stroke = Brushes.Blue, StrokeThickness = 3 };
                BLine = new Polyline { Stroke = Brushes.Green, StrokeThickness = 3 };
                GraphCanvas.Children.Add(ALine);
                GraphCanvas.Children.Add(BLine);
                StartAnimation();
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка ввода");
            }
        }
        private void StartAnimation()
        {
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            timer.Tick += DrawGraph;
            timer.Start();
        }
        private void DrawGraph(object sender, EventArgs e)
        {
            if (currentIndex >= t.Count)
            {
                timer.Stop();
                return;
            }
            //Масштабирование координат для отображения на Canvas
            double canvasWidth = GraphCanvas.ActualWidth;
            double canvasHeight = GraphCanvas.ActualHeight;
            // Находим максимальные значения для масштабирования
            double maxA = A.Max();
            double maxB = B.Max(); // Убедимся, что maxB не меньше 0
            double maxValue = Math.Max(maxA, maxB);
            double maxTime = t.Last();

            double tPoint = t[currentIndex]*canvasWidth/maxTime;
            double APoint = canvasHeight-A[currentIndex]*canvasHeight/maxValue;
            double BPoint = canvasHeight-B[currentIndex]*canvasHeight/maxValue;
            ALine.Points.Add(new Point(tPoint, APoint));
            BLine.Points.Add(new Point(tPoint, BPoint));
            currentIndex++;
        }
        private (List<double>, List<double>, List<double>) Solve()
        {
            List<double> t = new List<double>();
            List<double> A = new List<double>();
            List<double> B = new List<double>();
            double tEnd = 10;
            double h = 0.01;
            t.Add(0);
            A.Add(A0);
            B.Add(B0);

            while (t.Last() <= tEnd)
            {
                t.Add(t.Last() + h);  

                double k1_A = h * f1(A.Last(), B.Last(), t.Last());
                double k1_B = h * f2(A.Last(), B.Last(), t.Last());

                double k2_A = h * f1(A.Last() + k1_A / 2, B.Last() + k1_B / 2, t.Last() + h / 2);
                double k2_B = h * f2(A.Last() + k1_A / 2, B.Last() + k1_B / 2, t.Last() + h / 2);

                double k3_A = h * f1(A.Last() + k2_A / 2, B.Last() + k2_B / 2, t.Last() + h / 2);
                double k3_B = h * f2(A.Last() + k2_A / 2, B.Last() + k2_B / 2, t.Last() + h / 2);

                double k4_A = h * f1(A.Last() + k3_A, B.Last() + k3_B, t.Last() + h);
                double k4_B = h * f2(A.Last() + k3_A, B.Last() + k3_B, t.Last() + h);

                A.Add(A.Last() + (k1_A + 2 * k2_A + 2 * k3_A + k4_A) / 6);
                if (Math.Abs(A.Last()) < 0.00001)
                {
                    A.RemoveAt(A.Count - 1);
                    A.Add(0);
                }

                B.Add(B.Last() + (k1_B + 2 * k2_B + 2 * k3_B + k4_B) / 6 + addFood(t.Last()));
                if (Math.Abs(B.Last()) < 0.00001 || B.Last() < 0)
                {
                    B.RemoveAt(A.Count - 1);
                    B.Add(0);
                }
            }
            //TODO: if |A|<10^-5 => A=0, 
            //TODO: dynamic rungekutta
            return (t, A, B);
        }
        private double f1(double A, double B, double t)
        {
            return k * A * B;
        }
       private double f2(double A, double B, double t)
        {
            return -k1 * A;
        }
       private static double addFood(double t)
        {
            double period = 50.0;  // Период подачи еды (каждые 10 секунд)
            double duration = 0.1; // Длительность подачи еды
            double strength = 1.0; // Сила подачи еды
            if (t == 0)
            {
                return 0;
            }
            if (t % period < duration)
            {
                return strength;
            }
            return 0;
        }
        private void DrawAxes()
        {
            double height = GraphCanvas.ActualHeight;
            double width = GraphCanvas.ActualWidth;
            Line yAxis = new Line
            {
                X1 = 0,
                Y1 = 0,
                X2 = 0,
                Y2 = height,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Line xAxis = new Line
            {
                X1 = 0,
                Y1 = height,
                X2 = width,
                Y2 = height,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            GraphCanvas.Children.Add(xAxis);
            GraphCanvas.Children.Add(yAxis);
            double x1Position = (width / t.Last()); // Позиция для x=1
            if (x1Position>=0 && x1Position <= width) // Проверяем, чтобы не выходило за границы
            {
                // Линия метки
                Line xTick = new Line
                {
                    X1 = x1Position,
                    Y1 = height,
                    X2 = x1Position,
                    Y2 = height - 5,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
                GraphCanvas.Children.Add(xTick);
            }
            double y1Position = height - height / Math.Max(A.Max(),B.Max()); // Позиция для y=1
            if (y1Position >= 0 && y1Position <= height) // Проверяем, чтобы не выходило за границы
            {
                // Линия метки
                Line yTick = new Line
                {
                    X1 = 0,
                    Y1 = y1Position,
                    X2 = 5,
                    Y2 = y1Position,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
                GraphCanvas.Children.Add(yTick);
            }


        }
    }
}

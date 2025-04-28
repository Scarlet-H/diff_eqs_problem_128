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
        double A0, B0, C0, kA, kCA, muA, kB, kC, muC, kh;
        private List<double> t = new(), A = new(), B = new(), C = new();
        Polyline ALine = new(), BLine = new(), CLine = new();
        private DispatcherTimer timer = new();
        private int currentIndex = 0;
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
                C0 = double.Parse(C0_Box.Text);
                kA = double.Parse(kA_Box.Text);
                kCA = double.Parse(kCA_Box.Text);
                muA = double.Parse(muA_Box.Text);
                kB = double.Parse(kB_Box.Text);
                kC = double.Parse(kC_Box.Text);
                muC = double.Parse(muC_Box.Text);
                kh = double.Parse(kh_Box.Text);
                (t, A, B, C) = Solve();
                GraphCanvas.Children.Clear();
                DrawAxes();
                ALine = new Polyline { Stroke = Brushes.Blue, StrokeThickness = 3 };
                BLine = new Polyline { Stroke = Brushes.Green, StrokeThickness = 3 };
                CLine = new Polyline { Stroke = Brushes.Red, StrokeThickness = 3 };
                GraphCanvas.Children.Add(ALine);
                GraphCanvas.Children.Add(BLine);
                GraphCanvas.Children.Add(CLine);
                StartAnimation();
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка ввода");
            }
        }
        private void StartAnimation()
        {
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1) };
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
            double canvasWidth = GraphCanvas.ActualWidth;
            double canvasHeight = GraphCanvas.ActualHeight;
            double maxA = A.Max();
            double maxB = B.Max(); 
            double maxC = C.Max();
            double maxValue = Math.Max(Math.Max(maxA, maxB), maxC);
            double maxTime = t.Last();

            double tPoint = t[currentIndex] * canvasWidth / maxTime;
            double APoint = canvasHeight - A[currentIndex] * canvasHeight / maxValue;
            double BPoint = canvasHeight - B[currentIndex] * canvasHeight / maxValue;
            double CPoint = canvasHeight - C[currentIndex] * canvasHeight / maxValue;
            ALine.Points.Add(new Point(tPoint, APoint));
            BLine.Points.Add(new Point(tPoint, BPoint));
            CLine.Points.Add(new Point(tPoint, CPoint));
            currentIndex++;
        }
        private (List<double>, List<double>, List<double>, List<double>) Solve()
        {
            List<double> t = new List<double>();
            List<double> A = new List<double>();
            List<double> B = new List<double>();
            List<double> C = new List<double>();
            double tEnd = 50;
            double h = 0.01;
            t.Add(0);
            A.Add(A0);
            B.Add(B0);
            C.Add(C0);
            while (t.Last() <= tEnd)
            {
                t.Add(t.Last() + h);

                double k1_A = h * f1(A.Last(), B.Last(), C.Last(), t.Last());
                double k1_B = h * f2(A.Last(), B.Last(), C.Last(), t.Last());
                double k1_C = h * f3(A.Last(), B.Last(), C.Last(), t.Last());

                double k2_A = Math.Round(h * f1(A.Last() + k1_A / 2.0, B.Last() + k1_B / 2.0, C.Last() + k1_C / 2.0, t.Last() + h / 2.0), 4);
                double k2_B = Math.Round(h * f2(A.Last() + k1_A / 2.0, B.Last() + k1_B / 2.0, C.Last() + k1_C / 2.0, t.Last() + h / 2.0), 4);
                double k2_C = Math.Round(h * f3(A.Last() + k1_A / 2.0, B.Last() + k1_B / 2.0, C.Last() + k1_C / 2.0, t.Last() + h / 2.0), 4);

                double k3_A = Math.Round(h * f1(A.Last() + k2_A / 2.0, B.Last() + k2_B / 2.0,C.Last()+k2_C/2.0, t.Last() + h / 2.0), 4);
                double k3_B = Math.Round(h * f2(A.Last() + k2_A / 2.0, B.Last() + k2_B / 2.0, C.Last() + k2_C / 2.0, t.Last() + h / 2.0), 4);
                double k3_C = Math.Round(h * f3(A.Last() + k2_A / 2.0, B.Last() + k2_B / 2.0, C.Last() + k2_C / 2.0, t.Last() + h / 2.0), 4);

                double k4_A = Math.Round(h * f1(A.Last() + k3_A, B.Last() + k3_B,C.Last()+k3_C, t.Last() + h), 4);
                double k4_B = Math.Round(h * f2(A.Last() + k3_A, B.Last() + k3_B, C.Last() + k3_C, t.Last() + h), 4);
                double k4_C = Math.Round(h * f3(A.Last() + k3_A, B.Last() + k3_B, C.Last() + k3_C, t.Last() + h), 4);

                A.Add(A.Last() + (k1_A + 2.0 * k2_A + 2.0 * k3_A + k4_A) / 6.0);
                B.Add(B.Last() + (k1_B + 2.0 * k2_B + 2.0 * k3_B + k4_B) / 6.0);
                C.Add(C.Last() + (k1_C + 2.0 * k2_C + 2.0 * k3_C + k4_C) / 6.0);

                if (B.Last() < 0.00001)
                {
                    B.RemoveAt(A.Count - 1);
                    B.Add(0);
                }
            }
            return (t, A, B, C);
        }
        private double f1(double A, double B, double C, double t)
        {
            return kA * A * B - kCA * A * C - muA * A;
        }
        private double f2(double A, double B, double C, double t)
        {
            return -kB * A * t + addFood(t);
        }
        private double f3(double A, double B, double C, double t)
        {
            return kC * A * C - muC * C - kh * C * C;
        }
        private static double addFood(double t)
        {
            double period = 10.0;   // Период подачи еды
            double duration =1;  // Длительность подачи еды
            double strength = t;  // Сила подачи еды
            if (t <= 0)
                return 0;
            double timeInPeriod = t % period; 
            if (timeInPeriod > 0 && timeInPeriod < duration)
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
            double x1Position = (width / t.Last());
            if (x1Position >= 0 && x1Position <= width) 
            {
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
            double y1Position = height - height / Math.Max(A.Max(), B.Max()); 
            if (y1Position >= 0 && y1Position <= height)
            {
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

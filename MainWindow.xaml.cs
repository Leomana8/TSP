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
using TSP.ModelTSP;

using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace TSP
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Grid[] graphs;
        Cities cities;
        Stack<TextBox> errorTB;
        Thread calc;
        CancellationTokenSource cts;
        Queue<Task> tasks;
        public MainWindow()
        {
            InitializeComponent();
            graphs = new Grid[5];
            graphs[0] = graph1;
            graphs[1] = graph2;
            graphs[2] = graph3;
            graphs[3] = graph4;
            graphs[4] = graph5;
            errorTB = new Stack<TextBox>();
            tasks = new Queue<Task>();
            calc = new Thread(CalculateInThread);

        }
        // токен для видимого завершения задач
        // задачи не убиваются
        private CancellationTokenSource CancelCalculation()
        {            
            foreach (var task in tasks)
            {
                if (!task.IsCompleted)
                {
                    cts.Cancel();
                    break;
                }
            }
            tasks.Clear();
            cts = new CancellationTokenSource();
            return cts;
        }

        public void DrawPoints(object sender, RoutedEventArgs e)
        {
            CancelCalculation();
            ClearTextBox();
            int nCities;
            if (!Int32.TryParse(textB_countCities.Text, out nCities))
            {
                textB_countCities.Background = Brushes.Coral;
                errorTB.Push(textB_countCities);
                return;
            }
            cities = new Cities(nCities);
            cities.Generate((int)graph1.Width);
            GeometryGroup cityGroup = new GeometryGroup();
            for (int i = 0; i < cities.NumCities; i++)
            {
                Location l = cities.GetLocation(i);
                // формирование точек на карте
                EllipseGeometry city = new EllipseGeometry();
                city.Center = new Point(l.X, l.Y);
                city.RadiusX = 4;
                city.RadiusY = 4;
                cityGroup.Children.Add(city);
            }

            Path[] myPath = new Path[5];
            for (int i = 0; i < 5; i++)
            {
                myPath[i] = new Path();
                myPath[i].Fill = Brushes.Plum;
                myPath[i].Stroke = Brushes.Black;
                myPath[i].Data = cityGroup;

                graphs[i].Children.Clear();
                graphs[i].Children.Add(myPath[i]);
            }
            button_Calculate.IsEnabled = true;
        } // DrawPoints

        public void DrawLines(int[] trail, Grid graph)
        {
            GeometryGroup linesGroup = new GeometryGroup();
            LineGeometry line = new LineGeometry();
            int city = 1;
            for (; city < trail.Length; city++)
            {
                line = new LineGeometry();
                line.StartPoint = new Point(cities.GetLocation(trail[city - 1]).X, cities.GetLocation(trail[city - 1]).Y);
                line.EndPoint = new Point(cities.GetLocation(trail[city]).X, cities.GetLocation(trail[city]).Y);
                linesGroup.Children.Add(line);
            }
            line = new LineGeometry();
            line.StartPoint = new Point(cities.GetLocation(trail[city - 1]).X, cities.GetLocation(trail[city - 1]).Y);
            line.EndPoint = new Point(cities.GetLocation(trail[0]).X, cities.GetLocation(trail[0]).Y);
            linesGroup.Children.Add(line);

            Path myPath = new Path();
            myPath.Stroke = Brushes.Black;
            myPath.Data = linesGroup;
            if (graph.Children.Count > 1)
                graph.Children.Remove(graph.Children[1]);
            graph.Children.Add(myPath);
        }
        public void DrawLines(Location[] trail, Grid graph)
        {
            GeometryGroup linesGroup = new GeometryGroup();
            LineGeometry line = new LineGeometry();
            int city = 1;
            for (; city < trail.Length; city++)
            {
                line = new LineGeometry();
                line.StartPoint = new Point(trail[city - 1].X, trail[city - 1].Y);
                line.EndPoint = new Point(trail[city].X, trail[city].Y);
                linesGroup.Children.Add(line);
            }
            line = new LineGeometry();
            line.StartPoint = new Point(trail[city - 1].X, trail[city - 1].Y);
            line.EndPoint = new Point(trail[0].X, trail[0].Y);
            linesGroup.Children.Add(line);

            Path myPath = new Path();
            myPath.Stroke = Brushes.Black;
            myPath.Data = linesGroup;
            if (graph.Children.Count > 1)
                graph.Children.Remove(graph.Children[1]);
            graph.Children.Add(myPath);
        }

        public void ClearTextBox()
        {
            while (errorTB.Count > 0)
            {
                errorTB.Pop().Background = Brushes.White;
            }
        }

        public void Calculate(object sender, RoutedEventArgs e)
        {           
            ClearTextBox();
            CalculateInThread();
        }

        public void CalculateInThread()
        {
            CancellationTokenSource newCts = CancelCalculation();
            CalculateBF(newCts);
            CalculateACO(newCts);
            CalculateGA(newCts);
            CalculateSA(newCts);
            CalculateBB(newCts);           
        }
        
        public async void CalculateBF(CancellationTokenSource token)
        {
            progressBF.Visibility = Visibility.Visible;
            decimal maxTour = 0;
            string tb = textB_maxTour.Text;
            if (!Decimal.TryParse(tb, out maxTour))
            {
                textB_maxTour.Background = Brushes.Coral;
                errorTB.Push(textB_maxTour);
                return;
            }
            BruteForce algorithm = null;
            Stopwatch time = null;
            int[] solve = null;
            Task thisTask = (Task.Run(() =>
                {
                    algorithm = new BruteForce(maxTour);
                    time = new Stopwatch();
                    time.Start();
                    solve = algorithm.Solution(cities);
                    time.Stop();
                }));
            tasks.Enqueue(thisTask);          
            await Task.Run(() =>
            {
                while (true)
                {

                    if (token.Token.IsCancellationRequested || thisTask.IsCompleted)
                    {
                        break;
                    }
                }
            });
            if (solve != null)
            {
                DrawLines(solve, graphs[0]);
                timeBF.Content = time.ElapsedMilliseconds.ToString();
                lengthBF.Content = algorithm.TotalDistance.ToString("F2");
            }
            progressBF.Visibility = Visibility.Hidden;
        }

        public async void CalculateACO(CancellationTokenSource token)
        {
            progressACO.Visibility = Visibility.Visible;
            int alpha;
            if (!Int32.TryParse(textB_alpha.Text, out alpha))
            {
                textB_alpha.Background = Brushes.Coral;
                errorTB.Push(textB_alpha);
                return;
            }
            int beta;
            if (!Int32.TryParse(textB_beta.Text, out beta))
            {
                textB_beta.Background = Brushes.Coral;
                errorTB.Push(textB_beta);
                return;
            }
            double rho;
            if (!Double.TryParse(textB_rho.Text, out rho))
            {
                textB_rho.Background = Brushes.Coral;
                errorTB.Push(textB_rho);
                return;
            }
            double Q;
            if (!Double.TryParse(textB_Q.Text, out Q))
            {
                textB_Q.Background = Brushes.Coral;
                errorTB.Push(textB_Q);
                return;
            }
            int numAnts;
            if (!Int32.TryParse(textB_nAnts.Text, out numAnts))
            {
                textB_nAnts.Background = Brushes.Coral;
                errorTB.Push(textB_nAnts);
                return;
            }
            int maxTime;
            if (!Int32.TryParse(textB_time.Text, out maxTime))
            {
                textB_time.Background = Brushes.Coral;
                errorTB.Push(textB_time);
                return;
            }
            AntColony algorithm = null;
            Stopwatch time = null;
            int[] solve = null;
            Task thisTask = (Task.Run(() =>
                {
                    algorithm = new AntColony(alpha, beta, rho, Q, numAnts, maxTime);
                    time = new Stopwatch();
                    time.Start();
                    solve = algorithm.Solution(cities);
                    time.Stop();
                }));
            tasks.Enqueue(thisTask);          
            await Task.Run(() =>
            {
                while (true)
                {

                    if (token.Token.IsCancellationRequested || thisTask.IsCompleted)
                    {
                        break;
                    }
                }
            });
            if (solve != null)
            {
                DrawLines(solve, graphs[1]);
                timeAC.Content = time.ElapsedMilliseconds.ToString();
                lengthAC.Content = algorithm.TotalDistance.ToString("F2");
            }
            progressACO.Visibility = Visibility.Hidden;
        }

        public async void CalculateGA(CancellationTokenSource token)
        {
            progressGA.Visibility = Visibility.Visible;
            int numPopulation;
            if (!Int32.TryParse(textB_numPopulate.Text, out numPopulation))
            {
                textB_numPopulate.Background = Brushes.Coral;
                errorTB.Push(textB_numPopulate);
                return;
            }
            int maxTime;
            if (!Int32.TryParse(textB_timeGA.Text, out maxTime))
            {
                textB_time.Background = Brushes.Coral;
                errorTB.Push(textB_timeGA);
                return;
            }
            GeneticAlgorithm algorithm = null;
            Stopwatch time = null;
            Location[] solve = null;
            Task thisTask = (Task.Run(() =>
                {
                    algorithm = new GeneticAlgorithm(numPopulation, maxTime);
                    time = new Stopwatch();
                    time.Start();
                    solve = algorithm.Solution(cities);
                    time.Stop();
                }));
            tasks.Enqueue(thisTask);          
            await Task.Run(() =>
            {
                while (true)
                {

                    if (token.Token.IsCancellationRequested || thisTask.IsCompleted)
                    {
                        break;
                    }
                }
            });
            if (solve != null)
            {
                DrawLines(solve, graphs[2]);
                timeGA.Content = time.ElapsedMilliseconds.ToString();
                lengthGA.Content = algorithm.TotalDistance.ToString("F2");
            }
            progressGA.Visibility = Visibility.Hidden;
        }

        public async void CalculateSA(CancellationTokenSource token)
        {
            progressSA.Visibility = Visibility.Visible;
            double temperature;
            if (!Double.TryParse(textB_temperature.Text, out temperature))
            {
                textB_temperature.Background = Brushes.Coral;
                errorTB.Push(textB_temperature);
                return;
            }
            double absTemperature;
            if (!Double.TryParse(textB_absoluteTemperature.Text, out absTemperature))
            {
                textB_absoluteTemperature.Background = Brushes.Coral;
                errorTB.Push(textB_absoluteTemperature);
                return;
            }
            double coolingRate;
            if (!Double.TryParse(textB_coolingRate.Text, out coolingRate))
            {
                textB_coolingRate.Background = Brushes.Coral;
                errorTB.Push(textB_coolingRate);
                return;
            }
            SimulatedAnnealing algorithm = null;
            Stopwatch time = null;
            int[] solve = null;
            Task thisTask = (Task.Run(() =>
                {
                    algorithm = new SimulatedAnnealing(temperature, absTemperature, coolingRate);
                    time = new Stopwatch();
                    time.Start();
                    solve = algorithm.Solution(cities);
                    time.Stop();
                }));
            tasks.Enqueue(thisTask);          
            await Task.Run(() =>
            {
                while (true)
                {

                    if (token.Token.IsCancellationRequested || thisTask.IsCompleted)
                    {
                        break;
                    }
                }
            });
            if (solve != null)
            {
                DrawLines(solve, graphs[3]);
                timeSA.Content = time.ElapsedMilliseconds.ToString();
                lengthSA.Content = algorithm.TotalDistance.ToString("F2");
            }
            progressSA.Visibility = Visibility.Hidden;
        }

        public async void CalculateBB(CancellationTokenSource token)
        {
            progressBB.Visibility = Visibility.Visible;
            int maxTime;
            if (!Int32.TryParse(textB_timeBB.Text, out maxTime))
            {
                textB_timeBB.Background = Brushes.Coral;
                errorTB.Push(textB_timeBB);
                return;
            }
            BranchAndBound algorithm = null;
            Stopwatch time = null;
            Location[] solve = null;
            Task thisTask = (Task.Run(() =>
                {
                    algorithm = new BranchAndBound(maxTime);
                    time = new Stopwatch();
                    time.Start();
                    solve = algorithm.Solution(cities);
                    time.Stop();
                }));
            tasks.Enqueue(thisTask);          
            await Task.Run(() =>
            {
                while (true)
                {

                    if (token.Token.IsCancellationRequested || thisTask.IsCompleted)
                    {
                        break;
                    }
                }
            });
            if (solve != null)
            {
                DrawLines(solve, graphs[4]);
                timeBB.Content = time.ElapsedMilliseconds.ToString();
                lengthBB.Content = algorithm.TotalDistance.ToString("F2");
            }
            progressBB.Visibility = Visibility.Hidden;
        }

    } // Class MainWindow
}
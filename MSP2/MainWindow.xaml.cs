using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MultithreadingApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // 1
        private async void Task1_Click(object sender, RoutedEventArgs e)
        {
            await RunRangeAsync(ConsoleOutput, 0, 50, 1);
        }

        //  2
        private async void Task2_Click(object sender, RoutedEventArgs e)
        {
            int start, end;

            if (!ReadRange(StartBox, EndBox, out start, out end))
                return;

            await RunRangeAsync(ConsoleOutput, start, end, 1);
        }

        // 3
        private async void Task3_Click(object sender, RoutedEventArgs e)
        {
            int start, end, threadCount;

            if (!ReadRange(StartBox, EndBox, out start, out end))
                return;

            if (!ReadThreadCount(ThreadCountBox, out threadCount))
                return;

            await RunRangeAsync(ConsoleOutput, start, end, threadCount);
        }

        // 6 
        private async void Task6_Click(object sender, RoutedEventArgs e)
        {
            int start, end, threadCount;

            if (!ReadRange(StartBox6, EndBox6, out start, out end))
                return;

            if (!ReadThreadCount(ThreadCountBox6, out threadCount))
                return;

            await RunRangeAsync(Output6, start, end, threadCount);
        }

        bool ReadRange(TextBox startBox, TextBox endBox,
                       out int start, out int end)
        {
            start = 0;
            end = 0;

            if (!int.TryParse(startBox.Text, out start) ||
                !int.TryParse(endBox.Text, out end))
            {
                MessageBox.Show("Enter valid integer numbers!");
                return false;
            }

            if (start > end)
            {
                MessageBox.Show("Start number must be less than end number!");
                return false;
            }

            return true;
        }

        bool ReadThreadCount(TextBox box, out int threadCount)
        {
            threadCount = 0;

            if (!int.TryParse(box.Text, out threadCount) ||
                threadCount < 1 || threadCount > 100)
            {
                MessageBox.Show("Enter a number of threads from 1 to 100!");
                return false;
            }

            return true;
        }

        async Task RunRangeAsync(TextBox output, int start,
                                 int end, int threadCount)
        {
            output.Clear();

            Thread[] threads = new Thread[threadCount];

            int totalNumbers = end - start + 1;
            int numbersPerThread = totalNumbers / threadCount;
            int remainder = totalNumbers % threadCount;

            int currentStart = start;

            for (int i = 0; i < threadCount; i++)
            {
                int count = numbersPerThread;

                if (i < remainder)
                    count++;

                int threadStart = currentStart;
                int threadEnd = currentStart + count - 1;
                int threadNumber = i + 1;

                threads[i] = new Thread(() =>
                {
                    StringBuilder result = new StringBuilder();

                    result.AppendLine("Thread " + threadNumber + ":");

                    for (int j = threadStart; j <= threadEnd; j++)
                    {
                        result.AppendLine(j.ToString());
                    }

                    Dispatcher.Invoke(() =>
                    {
                        output.AppendText(result.ToString());
                    });
                });

                currentStart += count;
                threads[i].Start();
            }

            await Task.Run(() =>
            {
                foreach (Thread thread in threads)
                {
                    thread.Join();
                }
            });
        }


        int[] GenerateArray()
        {
            int[] numbers = new int[10000];

            Random random = new Random();

            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = random.Next(0, 1001);
            }

            return numbers;
        }

        // 4

        private async void Task4_Click(object sender, RoutedEventArgs e)
        {
            await CalculateStatisticsAsync(ConsoleOutput, false);
        }

        // 5

        private async void Task5_Click(object sender, RoutedEventArgs e)
        {
            await CalculateStatisticsAsync(ConsoleOutput, true);
        }

        //  7

        private async void Task7_Click(object sender, RoutedEventArgs e)
        {
            await CalculateStatisticsAsync(Output7, true);
        }


        async Task CalculateStatisticsAsync(TextBox output, bool saveToFile)
        {
            output.Clear();

            output.AppendText("Generating 10000 numbers...\n");

            int[] numbers = GenerateArray();

            int min = 0;
            int max = 0;
            double average = 0;

            // 1

            Thread minThread = new Thread(() =>
            {
                min = numbers[0];

                foreach (int number in numbers)
                {
                    if (number < min)
                        min = number;
                }
            });

            // 2

            Thread maxThread = new Thread(() =>
            {
                max = numbers[0];

                foreach (int number in numbers)
                {
                    if (number > max)
                        max = number;
                }
            });

            // 3

            Thread averageThread = new Thread(() =>
            {
                long sum = 0;

                foreach (int number in numbers)
                {
                    sum += number;
                }

                average = (double)sum / numbers.Length;
            });

            minThread.Start();
            maxThread.Start();
            averageThread.Start();

            await Task.Run(() =>
            {
                minThread.Join();
                maxThread.Join();
                averageThread.Join();
            });

            output.AppendText("\nCalculation completed!\n");
            output.AppendText("Number of elements: " + numbers.Length + "\n");
            output.AppendText("Minimum: " + min + "\n");
            output.AppendText("Maximum: " + max + "\n");
            output.AppendText("Average: " + average.ToString("F2") + "\n");

            // 5

            if (saveToFile)
            {
                string filePath = Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDocuments),
                    "MultithreadingResults.txt");

                string error = "";

                Thread fileThread = new Thread(() =>
                {
                    try
                    {
                        using (StreamWriter writer =
                               new StreamWriter(filePath, false))
                        {
                            writer.WriteLine("Generated numbers:");
                            writer.WriteLine(string.Join(", ", numbers));

                            writer.WriteLine();
                            writer.WriteLine("Minimum: " + min);
                            writer.WriteLine("Maximum: " + max);
                            writer.WriteLine("Average: " + average);
                        }
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message;
                    }
                });

                fileThread.Start();

                await Task.Run(() => fileThread.Join());

                if (error == "")
                {
                    output.AppendText("\nResults saved successfully!\n");
                    output.AppendText("File: " + filePath + "\n");
                }
                else
                {
                    output.AppendText("\nFile error: " + error + "\n");
                }
            }
        }

        private void ClearConsole_Click(object sender, RoutedEventArgs e)
        {
            ConsoleOutput.Clear();
        }
    }
}

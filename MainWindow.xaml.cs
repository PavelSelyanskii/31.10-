using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace _31._10_СП
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<Process> processes = new ObservableCollection<Process>();
        private Timer timer;
        private int timeSlice = 1000; // 1 секунда для Round Robin
        private int rrCounter = 0;  // Счетчик для Round Robin

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            ProcessListBox.ItemsSource = processes;
            timer = new Timer(TimerTick, null, 0, timeSlice);
            StatusTextBlock.Text = "Приложение запущено.\n";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddProcess_Click(object sender, RoutedEventArgs e)
        {
            var newProcess = new Process()
            {
                Name = $"Процесс {processes.Count + 1}",
                Priority = 1,
                BurstTime = 5,
                RemainingTime = 5,
                State = ProcessState.Ready,
                StartTime = DateTime.Now // Фиксируем время создания
            };
            processes.Add(newProcess);
            StatusTextBlock.Text += $"Процесс {newProcess.Name} добавлен в {DateTime.Now:HH:mm:ss.fff}.\n";
        }

        private void AlgorithmComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox)?.SelectedItem != null && StatusTextBlock != null)
            {
                rrCounter = 0;
                StatusTextBlock.Text += $"Выбран алгоритм: {(sender as ComboBox).SelectedItem} в {DateTime.Now:HH:mm:ss.fff}.\n";
            }
        }

        private void TimerTick(object state)
        {
            Dispatcher.Invoke(() =>
            {
                switch (AlgorithmComboBox.SelectedIndex)
                {
                    case 0:
                        PriorityScheduling();
                        break;
                    case 1:
                        RoundRobin();
                        break;
                    case 2:
                        ShortestJobFirst();
                        break;
                }
            });
        }

        private void PriorityScheduling()
        {
            var readyProcesses = processes.Where(p => p.State == ProcessState.Ready).OrderByDescending(p => p.Priority).ToList();
            if (readyProcesses.Any())
            {
                var currentProcess = readyProcesses.First();
                ProcessTick(currentProcess);
            }
        }

        private void RoundRobin()
        {
            var readyProcesses = processes.Where(p => p.State == ProcessState.Ready).ToList();
            if (readyProcesses.Any())
            {
                var currentProcess = readyProcesses.ElementAt(rrCounter % readyProcesses.Count);
                ProcessTick(currentProcess);
                rrCounter++;
            }
        }

        private void ShortestJobFirst()
        {
            var readyProcesses = processes.Where(p => p.State == ProcessState.Ready).OrderBy(p => p.BurstTime).ToList();
            if (readyProcesses.Any())
            {
                var currentProcess = readyProcesses.First();
                ProcessTick(currentProcess);
            }
        }

        private void ProcessTick(Process process)
        {
            process.RemainingTime--;
            if (process.RemainingTime <= 0)
            {
                process.State = ProcessState.Waiting;
                StatusTextBlock.Text += $"{process.Name} выполнен в {DateTime.Now:HH:mm:ss.fff}.\n";
            }
            else
            {
                StatusTextBlock.Text += $"{process.Name} выполняется...\n";
            }
        }
    }

    public enum ProcessState
    {
        Ready,
        Running,
        Waiting
    }

    public class Process : INotifyPropertyChanged
    {
        private string name;
        private int priority;
        private int burstTime;
        private int remainingTime;
        private ProcessState state;
        private DateTime startTime;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        public int Priority
        {
            get => priority;
            set
            {
                priority = value;
                OnPropertyChanged();
            }
        }

        public int BurstTime
        {
            get => burstTime;
            set
            {
                burstTime = value;
                OnPropertyChanged();
            }
        }

        public int RemainingTime
        {
            get => remainingTime;
            set
            {
                remainingTime = value;
                OnPropertyChanged();
            }
        }

        public ProcessState State
        {
            get => state;
            set
            {
                state = value;
                OnPropertyChanged();
            }
        }

        public DateTime StartTime
        {
            get => startTime;
            set
            {
                startTime = value;
                OnPropertyChanged();
            }
        }

        public string ProcessInfo => $"{Name} (Приоритет: {Priority}, Осталось: {RemainingTime}, Состояние: {State})";

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
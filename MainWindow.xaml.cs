using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using Microsoft.Win32;

namespace LogAnalyzer
{
    public class LogEntry
    {
        public int LineNumber { get; set; }
        public string Timestamp { get; set; } = "";
        public string Level { get; set; } = "";
        public string Source { get; set; } = "";
        public string Message { get; set; } = "";
        public string RawLine { get; set; } = "";
    }

    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<LogEntry> _allLogs = new();
        private readonly ICollectionView _logsView;

        // Patterns ordered from most specific to most generic
        private static readonly Regex[] LogPatterns =
        {
            // 2024-01-15 10:00:00.123 [INFO] [Source] Message
            new(@"^(?<ts>\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}:\d{2}(?:[.,]\d+)?)\s+\[(?<lvl>[A-Z]+)\]\s+(?:\[(?<src>[^\]]+)\]\s+)?(?<msg>.+)$", RegexOptions.Compiled),
            // 2024-01-15 10:00:00.123 INFO  Source: Message
            new(@"^(?<ts>\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}:\d{2}(?:[.,]\d+)?)\s+(?<lvl>ERROR|WARN(?:ING)?|INFO|DEBUG|TRACE|FATAL|CRITICAL)\s+(?:(?<src>[\w\.]+(?:\s[\w\.]+){0,2})\s*[:\-]\s+)?(?<msg>.+)$", RegexOptions.Compiled),
            // [2024-01-15 10:00:00] INFO - Message
            new(@"^\[(?<ts>[^\]]+)\]\s+(?<lvl>ERROR|WARN(?:ING)?|INFO|DEBUG|TRACE|FATAL|CRITICAL)\s*[-:]\s*(?<msg>.+)$", RegexOptions.Compiled),
            // 10:00:00.123 INFO Message
            new(@"^(?<ts>\d{2}:\d{2}:\d{2}(?:[.,]\d+)?)\s+(?<lvl>ERROR|WARN(?:ING)?|INFO|DEBUG|TRACE|FATAL|CRITICAL)\s+(?<msg>.+)$", RegexOptions.Compiled),
            // ERROR: Message  /  INFO - Message  (no timestamp)
            new(@"^(?<lvl>ERROR|WARN(?:ING)?|INFO|DEBUG|TRACE|FATAL|CRITICAL)\s*[-:]\s*(?<msg>.+)$", RegexOptions.Compiled),
        };

        public MainWindow()
        {
            // Must run before InitializeComponent(): CheckBox IsChecked fires during XAML load
            // and calls FilterCheckBox_Changed → UpdateStatusBar(), which needs _logsView.
            _logsView = CollectionViewSource.GetDefaultView(_allLogs);
            _logsView.Filter = FilterLog;
            InitializeComponent();
            LogGrid.ItemsSource = _logsView;
        }

        // ──────────────────────── FILTER ────────────────────────

        private bool FilterLog(object obj)
        {
            if (obj is not LogEntry entry) return false;

            bool levelOk = entry.Level switch
            {
                "ERROR"  => ErrorCheck.IsChecked == true,
                "WARN"   => WarnCheck.IsChecked == true,
                "INFO"   => InfoCheck.IsChecked == true,
                "DEBUG"  => DebugCheck.IsChecked == true,
                "TRACE"  => DebugCheck.IsChecked == true,
                _        => true
            };
            if (!levelOk) return false;

            var search = SearchBox.Text.Trim();
            if (string.IsNullOrEmpty(search)) return true;

            return entry.Message.Contains(search, StringComparison.OrdinalIgnoreCase)
                || entry.Source.Contains(search, StringComparison.OrdinalIgnoreCase)
                || entry.Timestamp.Contains(search, StringComparison.OrdinalIgnoreCase)
                || entry.Level.Contains(search, StringComparison.OrdinalIgnoreCase);
        }

        // ──────────────────────── OPEN FILE ────────────────────────

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Log files (*.log;*.txt)|*.log;*.txt|All files (*.*)|*.*",
                Title = "Open Log File"
            };
            if (dialog.ShowDialog() != true) return;
            LoadFile(dialog.FileName);
        }

        private void LoadFile(string path)
        {
            _allLogs.Clear();

            try
            {
                var lines = File.ReadAllLines(path);
                int lineNum = 0;

                foreach (var line in lines)
                {
                    lineNum++;
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    _allLogs.Add(ParseLine(line, lineNum));
                }

                _logsView.Refresh();
                UpdateStats();

                var filename = Path.GetFileName(path);
                FileNameText.Text = filename;
                Title = $"Log Analyzer — {filename}";
                ExportButton.IsEnabled = true;
                ClearButton.IsEnabled = true;
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ──────────────────────── PARSING ────────────────────────

        private static LogEntry ParseLine(string line, int lineNum)
        {
            foreach (var pattern in LogPatterns)
            {
                var m = pattern.Match(line);
                if (!m.Success) continue;

                return new LogEntry
                {
                    LineNumber = lineNum,
                    Timestamp  = m.Groups["ts"].Value.Trim(),
                    Level      = NormalizeLevel(m.Groups["lvl"].Value),
                    Source     = m.Groups["src"].Value.Trim(),
                    Message    = m.Groups["msg"].Value.Trim(),
                    RawLine    = line
                };
            }

            return new LogEntry
            {
                LineNumber = lineNum,
                Timestamp  = "",
                Level      = "",
                Source     = "",
                Message    = line,
                RawLine    = line
            };
        }

        private static string NormalizeLevel(string level) =>
            level.ToUpperInvariant() switch
            {
                "WARNING"  => "WARN",
                "CRITICAL" => "ERROR",
                "FATAL"    => "ERROR",
                var l      => l
            };

        // ──────────────────────── STATS ────────────────────────

        private void UpdateStats()
        {
            int err  = _allLogs.Count(l => l.Level == "ERROR");
            int warn = _allLogs.Count(l => l.Level == "WARN");
            int info = _allLogs.Count(l => l.Level == "INFO");

            ErrorCountText.Text = err  > 0 ? $"  {err}"  : "";
            WarnCountText.Text  = warn > 0 ? $"  {warn}" : "";
            InfoCountText.Text  = info > 0 ? $"  {info}" : "";
            TotalCountText.Text = _allLogs.Count > 0 ? $"{_allLogs.Count} entries" : "—";
        }

        private void UpdateStatusBar()
        {
            if (StatusText == null) return;
            int visible = _logsView.Cast<LogEntry>().Count();
            int total   = _allLogs.Count;
            StatusText.Text = visible == total
                ? $"{total} entries"
                : $"Showing {visible} of {total} entries";
        }

        // ──────────────────────── EVENTS ────────────────────────

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _logsView?.Refresh();
            UpdateStatusBar();
        }

        private void FilterCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            _logsView?.Refresh();
            UpdateStatusBar();
        }

        private void LogGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (LogGrid.SelectedItem is LogEntry entry)
                DetailBox.Text = entry.RawLine;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _allLogs.Clear();
            DetailBox.Text = "";
            FileNameText.Text = "No file loaded";
            Title = "Log Analyzer";
            ExportButton.IsEnabled = false;
            ClearButton.IsEnabled = false;
            UpdateStats();
            StatusText.Text = "Ready — Open a log file to start";
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        // ──────────────────────── EXPORT ────────────────────────

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            if (_allLogs.Count == 0)
            {
                MessageBox.Show("No logs to export.", "Export",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter   = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt",
                Title    = "Export Filtered Logs",
                FileName = "export.csv"
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                var rows = new List<string> { "Line,Timestamp,Level,Source,Message" };

                foreach (LogEntry entry in _logsView)
                    rows.Add($"{entry.LineNumber},{Csv(entry.Timestamp)},{Csv(entry.Level)},{Csv(entry.Source)},{Csv(entry.Message)}");

                File.WriteAllLines(dialog.FileName, rows);
                StatusText.Text = $"Exported {rows.Count - 1} entries → {Path.GetFileName(dialog.FileName)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string Csv(string val) => $"\"{val.Replace("\"", "\"\"")}\"";
    }
}

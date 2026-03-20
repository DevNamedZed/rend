using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Rend;

namespace Rend.PdfViewer
{
    public partial class MainWindow : Window
    {
        private PdfReader? _reader;
        private int _currentPage;
        private double _zoom = 1.0;
        private string _fileName = "";
        private int _renderGeneration;

        public MainWindow()
        {
            InitializeComponent();
            Closed += (_, _) => _reader?.Dispose();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs args)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
                Title = "Open PDF File"
            };

            if (dialog.ShowDialog() == true)
            {
                LoadPdf(dialog.FileName);
            }
        }

        private void Window_Drop(object sender, DragEventArgs args)
        {
            if (args.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])args.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && files[0].EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    LoadPdf(files[0]);
                }
            }
        }

        private void Window_DragOver(object sender, DragEventArgs args)
        {
            if (args.Data.GetDataPresent(DataFormats.FileDrop))
            {
                args.Effects = DragDropEffects.Copy;
            }
            else
            {
                args.Effects = DragDropEffects.None;
            }
            args.Handled = true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Left || args.Key == Key.PageUp)
            {
                PrevPage_Click(sender, args);
                args.Handled = true;
            }
            else if (args.Key == Key.Right || args.Key == Key.PageDown)
            {
                NextPage_Click(sender, args);
                args.Handled = true;
            }
            else if (args.Key == Key.OemPlus || args.Key == Key.Add)
            {
                ZoomIn_Click(sender, args);
                args.Handled = true;
            }
            else if (args.Key == Key.OemMinus || args.Key == Key.Subtract)
            {
                ZoomOut_Click(sender, args);
                args.Handled = true;
            }
            else if (args.Key == Key.D0 && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                _zoom = 1.0;
                ApplyZoom();
                args.Handled = true;
            }
        }

        private void LoadPdf(string filePath)
        {
            try
            {
                _reader?.Dispose();
                _reader = new PdfReader(filePath);
                _fileName = Path.GetFileName(filePath);
                _currentPage = 0;
                _zoom = 1.0;

                FileNameLabel.Text = _fileName;

                var metadata = _reader.Metadata;
                string titleSuffix = !string.IsNullOrEmpty(metadata.Title) ? metadata.Title : _fileName;
                Title = $"Rend PDF Viewer - {titleSuffix}";

                DropOverlay.Visibility = Visibility.Collapsed;
                PageScroller.Visibility = Visibility.Visible;

                UpdateNavigation();
                RenderCurrentPage();
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error loading PDF: {ex.Message}";
                MessageBox.Show($"Failed to load PDF:\n\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrevPage_Click(object sender, RoutedEventArgs args)
        {
            if (_reader != null && _currentPage > 0)
            {
                _currentPage--;
                UpdateNavigation();
                RenderCurrentPage();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs args)
        {
            if (_reader != null && _currentPage < _reader.PageCount - 1)
            {
                _currentPage++;
                UpdateNavigation();
                RenderCurrentPage();
            }
        }

        private void DpiCombo_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (_reader != null)
            {
                RenderCurrentPage();
            }
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs args)
        {
            _zoom = Math.Min(_zoom + 0.25, 5.0);
            ApplyZoom();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs args)
        {
            _zoom = Math.Max(_zoom - 0.25, 0.25);
            ApplyZoom();
        }

        private void ZoomFit_Click(object sender, RoutedEventArgs args)
        {
            if (PageImage.Source is BitmapSource bitmap)
            {
                double availableWidth = PageScroller.ActualWidth - 40;
                double availableHeight = PageScroller.ActualHeight - 40;
                double scaleX = availableWidth / bitmap.PixelWidth;
                double scaleY = availableHeight / bitmap.PixelHeight;
                _zoom = Math.Min(scaleX, scaleY);
                _zoom = Math.Max(0.1, Math.Min(_zoom, 5.0));
                ApplyZoom();
            }
        }

        private void ApplyZoom()
        {
            PageImage.LayoutTransform = new ScaleTransform(_zoom, _zoom);
            ZoomLabel.Text = $"{(int)(_zoom * 100)}%";
        }

        private void UpdateNavigation()
        {
            if (_reader == null)
            {
                return;
            }
            PageLabel.Text = $"Page {_currentPage + 1} / {_reader.PageCount}";
            BtnPrev.IsEnabled = _currentPage > 0;
            BtnNext.IsEnabled = _currentPage < _reader.PageCount - 1;
        }

        private int GetSelectedDpi()
        {
            if (DpiCombo.SelectedItem is ComboBoxItem item)
            {
                if (int.TryParse(item.Content.ToString(), out int dpi))
                {
                    return dpi;
                }
            }
            return 150;
        }

        private async void RenderCurrentPage()
        {
            if (_reader == null)
            {
                return;
            }

            int dpi = GetSelectedDpi();
            int page = _currentPage;
            var reader = _reader;
            int generation = ++_renderGeneration;

            StatusLabel.Text = $"Rendering page {page + 1} at {dpi} DPI...";
            IsEnabled = false;

            try
            {
                byte[] pngBytes = await Task.Run(() => reader.RenderPage(page, dpi));

                if (generation != _renderGeneration)
                {
                    return;
                }

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(pngBytes);
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                PageImage.Source = bitmapImage;
                ApplyZoom();

                var pageInfo = reader.GetPageInfo(page);
                int warningCount = reader.RenderWarnings.Count;
                StatusLabel.Text = $"{_fileName} - Page {page + 1}/{reader.PageCount} - " +
                                   $"{pageInfo.Width:F0} x {pageInfo.Height:F0} pt - " +
                                   $"{bitmapImage.PixelWidth} x {bitmapImage.PixelHeight} px - {dpi} DPI";

                if (warningCount > 0)
                {
                    StatusLabel.Text += $" ({warningCount} warnings)";
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Render error: {ex.Message}";
            }
            finally
            {
                IsEnabled = true;
            }
        }
    }
}

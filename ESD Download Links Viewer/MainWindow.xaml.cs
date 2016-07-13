using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Win32;

namespace ESD_Download_Links_Viewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<DownloadItem> fileList;

        double tempExpireDateUnixTime = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                EDBHelper edbHelper = new EDBHelper();
                fileList = await edbHelper.ReadDataAsync(@"C:\Windows.old\WINDOWS\SoftwareDistribution\DataStore\DataStore.edb");
                gridView.ItemsSource = fileList;

                radioFilterESD.IsChecked = true;
            }
            catch (EsentFileNotFoundException)
            {
                MessageBox.Show("Can't found C:\\Windows.old\\WINDOWS\\SoftwareDistribution\\DataStore\\DataStore.edb\n\n"+
                    "Please make sure you did not cleaned up the Previous Windows Installation.\n"+
                    "Or you can open a DataStore file manually.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool FilterESD(object item)
        {
            DownloadItem fileItem = item as DownloadItem;
            if (fileItem != null)
            {
                if (fileItem.FileType == ".esd")
                {
                    return true;
                }
            }
            return false;
        }

        private DateTime UnixTimeToDateTime(double unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);
        }

        private void radioFilterAll_Checked(object sender, RoutedEventArgs e)
        {
            ListCollectionView view = CollectionViewSource.GetDefaultView(gridView.ItemsSource) as ListCollectionView;
            if (view != null)
            {
                view.Filter = null;
            }
        }

        private void radioFilterESD_Checked(object sender, RoutedEventArgs e)
        {
            ListCollectionView view = CollectionViewSource.GetDefaultView(gridView.ItemsSource) as ListCollectionView;
            if (view != null)
            {
                view.Filter = new Predicate<object>(FilterESD);
            }
        }

        private void gridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tempExpireDateUnixTime = 0;
            txtUrlExpireDate.Text = "";
            txtUrlExpireDate.Foreground = Brushes.Black;

            if (gridView.SelectedItem != null)
            {
                DownloadItem item = gridView.SelectedItem as DownloadItem;

                if(!String.IsNullOrEmpty(item.Url))
                {
                    Regex matchUnixTime = new Regex(@"(?<=\D+)\d{10}(?=\D.+)");
                    string timeStr = matchUnixTime.Match(item.Url).Value;

                    if(!String.IsNullOrEmpty(timeStr))
                    {
                        double unixTime;
                        double.TryParse(timeStr, out unixTime);

                        tempExpireDateUnixTime = unixTime;

                        DateTime expireDate = UnixTimeToDateTime(unixTime);

                        txtUrlExpireDate.Text = expireDate.ToLocalTime().ToString();

                        if (DateTime.UtcNow > expireDate)
                        {
                            txtUrlExpireDate.Foreground = Brushes.Red;
                        }
                    }
                }
            }
        }

        private void btnCopyFileName_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtFileName.Text);
        }

        private void btnCopyUrl_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtUrl.Text);
        }

        private void btnCopySHA1_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtSHA1.Text);
        }

        private void btnCopySHA256_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtSHA256.Text);
        }

        private void btnCopyDetails_Click(object sender, RoutedEventArgs e)
        {
            string infos;

            if (tempExpireDateUnixTime != 0)
            {
                infos = "File Name: " + txtFileName.Text + "\r\n" +
                    "Url: " + txtUrl.Text + "\r\n" +
                    "Expire At: " + UnixTimeToDateTime(tempExpireDateUnixTime).ToString() + " UTC\r\n" +
                    "SHA-1: " + txtSHA1.Text + "\r\n" +
                    "SHA-256: " + txtSHA256.Text + "\r\n";
            }
            else
            {
                infos = "File Name: " + txtFileName.Text + "\r\n" +
                    "Url: " + txtUrl.Text + "\r\n" +
                    "SHA-1: " + txtSHA1.Text + "\r\n" +
                    "SHA-256: " + txtSHA256.Text + "\r\n";
            }

            Clipboard.SetText(infos);
        }

        private async void btnOpenDB_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.Filter = "Esent Database(*.edb)|*.edb";

            if ((bool)openfile.ShowDialog() && openfile.FileName != "")
            {
                try
                {
                    if ((bool)radioFilterESD.IsChecked)
                    {
                        radioFilterESD.IsChecked = false;
                    }

                    EDBHelper edbHelper = new EDBHelper();
                    fileList = await edbHelper.ReadDataAsync(openfile.FileName);
                    gridView.ItemsSource = fileList;

                    radioFilterESD.IsChecked = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void GitHubLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));

            e.Handled = true;
        }
    }
}

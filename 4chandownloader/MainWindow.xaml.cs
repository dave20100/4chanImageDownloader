using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using System.Windows.Forms;
using System.IO;

namespace _4chandownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FilePath.Text = Directory.GetCurrentDirectory();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            List<string> photos = new List<string>();
            WebClient wc = new WebClient();
            wc.Headers.Add("User-Agent: Other");
            string pageString = "";
            if (!page.Text.Contains("boards.4chan.org") || !page.Text.Contains("thread"))
            {
                System.Windows.MessageBox.Show( "This is not a thread link");
                return;
            }
            startdown.IsEnabled = false;
            try
            {
                wc.DownloadProgressChanged += Wc_DownloadProgressChanged; ;
                pageString = await wc.DownloadStringTaskAsync(page.Text);
            }
            catch (Exception exc)
            {
                page.Text = exc.Message;
            }
            
            photos = GetLinks(pageString);

            int count = 1;
            string mainFolderPath = createThreadFolderPath();
            foreach (string link in photos)
            {
                WebClient wctopics = new WebClient();
                wctopics.DownloadProgressChanged += Wctopics_DownloadProgressChanged;
                wctopics.DownloadFileAsync(new Uri("http://" + link), mainFolderPath + "\\File" + count + link.Substring(link.IndexOf(".", link.Length - 6)));
                count++;
            }
            startdown.IsEnabled = true;
        }

        private void Wctopics_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            pbpics.Value = e.ProgressPercentage;
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            pb.Value = e.ProgressPercentage;
        }

        public static List<string> GetLinks(string pageString)
        {
            string editToLink = "";
            List<string> photoLinks = new List<string>();
            int i = 0;
            while ((i = pageString.IndexOf("fileThumb", i)) != -1)
            {
                editToLink = (pageString.Substring(i, 80));
                editToLink = editToLink.Substring(editToLink.IndexOf("href") + 8);
                editToLink = editToLink.Substring(0, editToLink.IndexOf("\""));
                photoLinks.Add(editToLink);
                i++;
            }
            return photoLinks;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                FilePath.Text = dialog.SelectedPath;
            }
            if(FilePath.Text == "")
            {
                FilePath.Text = Directory.GetCurrentDirectory();
            }
            
        }

        public string createThreadFolderPath()
        {
            int counter = 1;
            while (Directory.Exists(FilePath.Text + "/Thread" + counter))
            {
                counter++;
            }
            Directory.CreateDirectory(FilePath.Text + "/Thread" + counter);
            return FilePath.Text + "/Thread" + counter;
        }
    }
    }

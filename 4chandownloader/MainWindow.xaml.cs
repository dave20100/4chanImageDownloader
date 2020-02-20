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
            FilePath.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        private void StartDownloading(object sender, RoutedEventArgs e)
        {
            startdown.IsEnabled = false;
            int pageType = this.CheckLinkType(page.Text);
            if(pageType == 1)
            {
                DownloadThread(page.Text);
            }
            else if(pageType == 2)
            {

            }
            else
            {
                startdown.IsEnabled = true;
            }
        }

        private async void DownloadThread(string threadURL)
        {
            List<string> photos = new List<string>();

            string threadString = await GetSourceAsString(threadURL);
            photos = GetLinks(threadString);
            pbThread.Value = 0;
            pbThread.Maximum = photos.Count;

            string threadTitle = ExtractTitleFromSource(threadString);
            string mainFolderPath = createThreadFolderPath(threadTitle);
            int count = 1;
            foreach (string link in photos)
            {
                WebClient wctopics = new WebClient();
                wctopics.DownloadFileCompleted += Wctopics_DownloadFileCompleted; ;
                wctopics.DownloadFileAsync(new Uri("http://" + link), mainFolderPath + "\\File" + count + link.Substring(link.LastIndexOf(".")));
                count++;
            }
        }

        private string ExtractTitleFromSource(string threadString) 
        {
            try
            {
                threadString = threadString.Substring(threadString.IndexOf("subject\">") + 9, threadString.Length/2);
                threadString = threadString.Substring(0, threadString.IndexOf("</span>"));
            }
            catch
            {
                threadString = "Thread";
            }
            return threadString;
        }

        private void Wctopics_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            pbThread.Value += 1;
            progressText.Text = pbThread.Value + "\\" + pbThread.Maximum;
            if (pbThread.Value == pbThread.Maximum)
            {
                startdown.IsEnabled = true;
                progressText.Text = "";
            }
        }


        public static List<string> GetLinks(string pageString)
        {
            string editToLink;
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

        private void SetFilePath(object sender, RoutedEventArgs e)
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

        public string createThreadFolderPath(string name = "Thread")
        {
            string pathToFile = FilePath.Text + "/" + name;
            int count = 1;
            if (Directory.Exists(pathToFile))
            {
                while (Directory.Exists(pathToFile + count)) {
                    count++;
                }
                Directory.CreateDirectory(pathToFile + count);
                return pathToFile + count;
            }
            Directory.CreateDirectory(pathToFile);
            return pathToFile;
        }

        private async Task<string> GetSourceAsString(string threadURL)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("User-Agent: Other");
            string pageString = "";
            try
            {
                pageString = await wc.DownloadStringTaskAsync(threadURL);
            }
            catch (Exception exc)
            {
                page.Text = exc.Message;
                startdown.IsEnabled = true;
            }
            return pageString;
        }

        private int CheckLinkType(string pageURL)
        {
            if (pageURL.Contains("boards.4chan.org") || pageURL.Contains("thread"))
            {
                return 1;
            }
            else if(pageURL.Contains("boards.4chan.org") || pageURL.Contains("catalog"))
            {
                return 2;
            }
            else
            {
                System.Windows.MessageBox.Show("This is not a thread link");
                return -1;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YYX.SearchChinese
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private const string DoubleQuotesRegexPatten = "\".*?\"";
        private const string ChineseRegexPatten = @"[\u4e00-\u9fa5]";
        //使用字段再次选择文件夹时，默认选择上一次打开的目录
        private readonly FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            var success = DialogResult.OK == folderBrowserDialog.ShowDialog();
            if (success)
            {
                textBoxFolderPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            buttonSearch.Enabled = false;

            var folderPath = textBoxFolderPath.Text;
            var exists = !string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath);
            if (!exists)
            {
                MessageBox.Show(@"文件夹路径不正确");
                return;
            }

            var directoryInfo = new DirectoryInfo(folderPath);
            var textList = new List<string>();

            Task.Factory.StartNew(() =>
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                Matcher(directoryInfo, textList);

                stopwatch.Stop();
                var timeSpan = stopwatch.Elapsed;
                var descriptionText = $"数量：{textList.Count}，耗时：{timeSpan}";

                var allText = string.Join(Environment.NewLine, textList);
                BeginInvoke((EventHandler)delegate
                {
                    richTextBox.Clear();
                    richTextBox.Text = allText;

                    labelDescription.Text = descriptionText;

                    buttonSearch.Enabled = true;
                });
            });
        }

        private static void Matcher(DirectoryInfo directoryInfo, List<string> textList)
        {
            //遍历文件
            foreach (var fileInfo in directoryInfo.GetFiles())
            {
                if (string.Equals(fileInfo.Extension, ".cs"))
                {
                    string text;
                    TextFile.Read(fileInfo.FullName, out text);
                    var matchCollection = Regex.Matches(text, DoubleQuotesRegexPatten);
                    var query =
                        from object item in matchCollection
                        let marchText = item.ToString().Trim('"')
                        where Regex.IsMatch(marchText, ChineseRegexPatten)
                        where textList.Contains(marchText) == false
                        select marchText;
                    textList.AddRange(query);
                }
            }

            //递归遍历文件夹
            foreach (var insideDirectoryInfo in directoryInfo.GetDirectories())
            {
                Matcher(insideDirectoryInfo, textList);
            }
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(richTextBox.Text);
        }
    }
}

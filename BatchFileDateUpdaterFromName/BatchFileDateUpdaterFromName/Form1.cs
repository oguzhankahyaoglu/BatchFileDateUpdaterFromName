using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BatchFileDateUpdaterFromName
{
    public partial class Form1 : Form
    {
    public class FindResult
    {
        public FileInfo File;
        public DateTime? Date;
        public String DateStr;
        public bool IsFileFound;
        public bool IsDateParsed;

        public FindResult(FileInfo fileInfo, DateTime? date, string dateStr, bool isFileFound, bool isDateParsed)
        {
            File = fileInfo;
            Date = date;
            DateStr = dateStr;
            IsFileFound = isFileFound;
            IsDateParsed = isDateParsed;
        }
    }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Escape: this.Close(); break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public Form1()
        {
            InitializeComponent();
        }

        private String Parse(String s, out DateTime? parsedDate)
        {
            parsedDate = null;
            try
            {
                var name = s.Substring(0, s.LastIndexOf('.'));
                var dateTimeFormat = txtDateTimeFormat.Text;
                parsedDate = DateTime.ParseExact(name, dateTimeFormat, CultureInfo.InvariantCulture);
                return parsedDate.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }


        private List<FindResult> FindFiles(Action<FindResult> forEachMatchedFile)
        {
            var path = folderBrowserDialog1.SelectedPath;
            var directory = new DirectoryInfo(path);
            var files = directory.GetFiles("*", checkBox1.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            var regex = new Regex(txtRegex.Text);
            var results = new List<FindResult>();
            Array.ForEach(files, file =>
            {
                var match = regex.Match(file.Name);
                if (!match.Success)
                {
                    var result = new FindResult(file, null, null, false, false);
                    forEachMatchedFile(result);
                    results.Add(result);
                    return;
                }
                DateTime? parsedDate;
                //Parse(file.Name, out parsedDate);
                var dateStr = Parse(match.Groups["date"].Value, out parsedDate);
                if (parsedDate == null)
                {
                    var findResult = new FindResult(file, null, dateStr, true, false);
                    forEachMatchedFile(findResult);
                    results.Add(findResult);
                }
                else
                {
                    var findResult = new FindResult(file, parsedDate.Value, dateStr, true, true);
                    forEachMatchedFile(findResult);
                    results.Add(findResult);
                }
            });
            return results;
        }

        private void btnFindFiles_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            var path = folderBrowserDialog1.SelectedPath;
            listView1.Items.Clear();
            if (String.IsNullOrEmpty(path))
                return;
            var results = FindFiles(result =>
            {
                var createDate = result.File.CreationTime.ToString();
                var modifyDate = result.File.LastWriteTime.ToString();
                var item = new ListViewItem(new[] { result.File.Name, createDate, modifyDate, result.DateStr ?? "" });
                if (result.IsDateParsed)
                    item.BackColor = Color.GreenYellow;
                else if (result.IsFileFound)
                    item.BackColor = Color.OrangeRed;
                listView1.Items.Add(item);
            });
            MessageBox.Show(String.Format("Found {0} files, {1} of them matched with regex, {2} of them is parsed successfully as date.", results.Count, results.Count(r=> r.IsFileFound), results.Count(r=> r.IsDateParsed)));
        }

        private void btnUpdateFiles_Click(object sender, EventArgs e)
        {
            var results = FindFiles(result =>
            {
                if (!result.IsDateParsed)
                    return;
                result.File.LastWriteTime = result.Date.Value;
                result.File.CreationTime = result.Date.Value;
            });
            MessageBox.Show(String.Format("Updated {0} files!", results.Count(r => r.IsDateParsed)));
        }

    }
}

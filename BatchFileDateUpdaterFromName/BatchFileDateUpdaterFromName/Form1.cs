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
using System.Windows.Forms;

namespace BatchFileDateUpdaterFromName
{
    public partial class Form1 : Form
    {
        public static String ParseFormat = ConfigurationManager.AppSettings["DateTimeParseFormat"];
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
                parsedDate = DateTime.ParseExact(name, ParseFormat, CultureInfo.InvariantCulture);
                return parsedDate.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var path = folderBrowserDialog1.SelectedPath;
            var directory = new DirectoryInfo(path);
            var files = directory.GetFiles("*", checkBox1.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            var count = 0;
            foreach (var file in files)
            {
                DateTime? parsedDate;
                Parse(file.Name, out parsedDate);
                if (parsedDate == null) 
                    continue;
                file.LastWriteTime = parsedDate.Value;
                file.CreationTime = parsedDate.Value;
                count ++;
            }
            MessageBox.Show("Updated " + count + " files!");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            var path = folderBrowserDialog1.SelectedPath;
            var directory = new DirectoryInfo(path);
            var files = directory.GetFiles("*", checkBox1.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            listView1.Items.Clear();
            foreach (var file in files)
            {
                var createDate = file.CreationTime.ToString();
                var modifyDate = file.LastWriteTime.ToString();
                DateTime? parsedDate;
                var dateStr = Parse(file.Name, out parsedDate);
                var item = new ListViewItem(new[] { file.Name, createDate, modifyDate, dateStr ?? "" });
                listView1.Items.Add(item);
            }
        }
    }
}

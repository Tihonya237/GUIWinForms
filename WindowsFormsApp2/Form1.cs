using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        public BindingSource Data { get; set; }

        Title title = new Title();

        public Form1()
        {
            Data = new BindingSource();

            InitializeComponent();
            DataGridViewTextBoxColumn fileNameColumn = new DataGridViewTextBoxColumn();
            fileNameColumn.HeaderText = "Имя файла";
            dataGridView1.Columns.Add(fileNameColumn);

            dataGridView2.DataSource = Data;
            Data.ListChanged += new ListChangedEventHandler(MyData);
            comboBox1.SelectedIndex = 0;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter(saveFileDialog1.FileName.ToString());

                for (int i = 0; i < dataGridView2.Rows.Count - 1; i++)
                {
                    for (int j = 0; j < dataGridView2.Columns.Count; j++)
                    {
                        file.Write(dataGridView2.Rows[i].Cells[j].Value.ToString() + " ");
                    }
                    file.WriteLine("");
                }
                file.Close();
                MessageBox.Show("Файл успешно сохранен", "Успешная операция", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                string selectedValue = comboBox1.SelectedItem.ToString();

                if (selectedValue == "Draw as lines")
                {
                    currentChartType = SeriesChartType.Line;
                }
                else if (selectedValue == "Draw as spline")
                {
                    currentChartType = SeriesChartType.Spline;
                }

                foreach (var series in chart1.Series)
                {
                    series.ChartType = currentChartType;
                }

                chart1.DataSource = null;
                chart1.DataSource = Data;
            }
        }
        private void MyData(object sender, ListChangedEventArgs e)
        {
            comboBox1_SelectedIndexChanged(sender, e);
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count)
            {
                dataGridView2.Rows.Clear();
                string fileName = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

                List<string> fileContent = ReadFileContent(filePath);
                AddFileContentToDataGridView2(fileContent);  
            }
        }

        private void Load_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string fileName in openFileDialog.FileNames)
                    {
                        dataGridView1.Rows.Add(Path.GetFileName(fileName));
                        dataGridView2.Rows.Clear();

                        List<string> fileContent = ReadFileContent(fileName);
                        AddFileContentToDataGridView2(fileContent);

                        Series loadedSeries = CreateChartSeries(Data, Path.GetFileName(fileName));
                    }
                }
            }
        }

        private List<string> ReadFileContent(string filePath)
        {
            List<string> fileContent = new List<string>();

            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        fileContent.Add(line);
                    }
                }
            }
            return fileContent;
        }

        private void AddFileContentToDataGridView2(List<string> fileContent)
        {
            foreach (string line in fileContent)
            {
                var values = line.Split(' '); // предполагаем, что значения разделены табуляцией
                double xx = double.Parse(values[0]), yy = double.Parse(values[1]);

                Data.Add(new Data() { X = xx, Y = yy });

                BindingSource loadedTable = new BindingSource();
                loadedTable.DataSource = Data;

                dataGridView2.DataSource = Data;
            }
        }
        private void AddButton_Click(object sender, EventArgs e)
        {
            Data.Add(new Data() { X = 0, Y = 0 });
        }
        private void DrawAsLines_Click(object sender, EventArgs e)
        {
            chart1.DataSource = null;
            chart1.Series[0].ChartType =
            System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart1.DataSource = Data;
        }
        private void DrawAsSpline_Click(object sender, EventArgs e)
        {
            chart1.DataSource = null;
            chart1.Series[0].ChartType =
            System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            chart1.DataSource = Data;
        }

        private Series CreateChartSeries(BindingSource table, string seriesName)
        {
            Series series = new Series(seriesName);
            series.ChartType = currentChartType;

            foreach (Data dataPoint in table.List)
            {
                series.Points.AddXY(dataPoint.X, dataPoint.Y);
            }

            chart1.Series.Add(series);
            return series;
        }
    }
    public class Data
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
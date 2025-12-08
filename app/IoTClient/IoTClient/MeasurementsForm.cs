using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IoTClient
{
    public partial class MeasurementsForm : Form
    {
        private List<Measurement> _data;

        public MeasurementsForm(List<Measurement> data, string title)
        {
            _data = data;
            InitializeComponent();
            Text = "Measurements – " + title;

            LoadTable();
        }

        private void LoadTable()
        {
            dataGrid.DataSource = _data;

            foreach (DataGridViewColumn col in dataGrid.Columns)
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            StyleGrid(dataGrid);
        }

        private void StyleGrid(DataGridView g)
        {
            g.BackgroundColor = Color.FromArgb(32, 32, 32);
            g.EnableHeadersVisualStyles = false;

            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            g.DefaultCellStyle.BackColor = Color.FromArgb(25, 25, 25);
            g.DefaultCellStyle.ForeColor = Color.White;

            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 70, 70);
            g.DefaultCellStyle.SelectionForeColor = Color.White;

            g.RowHeadersVisible = false;
        }
    }
}

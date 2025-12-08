namespace IoTClient
{
    partial class MeasurementsForm
    {
        private System.ComponentModel.IContainer components = null;
        private DataGridView dataGrid;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            dataGrid = new DataGridView();

            SuspendLayout();

            // === FORM ===
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(900, 600);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = true;

            // === GRID ===
            dataGrid.Dock = DockStyle.Fill;
            dataGrid.ReadOnly = true;
            dataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            Controls.Add(dataGrid);

            ResumeLayout(false);
        }
    }
}

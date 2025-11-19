namespace IoTClient
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblGateway;
        private System.Windows.Forms.Panel panelTop;

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.SplitContainer splitRight;

        private System.Windows.Forms.DataGridView dataGridSensors;
        private System.Windows.Forms.DataGridView dataGridPending;

        private System.Windows.Forms.GroupBox groupSensors;
        private System.Windows.Forms.GroupBox groupCharts;
        private System.Windows.Forms.GroupBox groupPending;

        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Button btnIgnore;

        private System.Windows.Forms.TabControl tabControlCharts;
        private System.Windows.Forms.TabPage tabTemp;
        private System.Windows.Forms.TabPage tabHum;
        private System.Windows.Forms.TabPage tabPress;

        private ScottPlot.WinForms.FormsPlot formsPlotTemp;
        private ScottPlot.WinForms.FormsPlot formsPlotHum;
        private ScottPlot.WinForms.FormsPlot formsPlotPress;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }


        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // ============================
            // TOP PANEL
            // ============================
            this.panelTop = new System.Windows.Forms.Panel();
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Height = 45;

            this.lblGateway = new System.Windows.Forms.Label();
            this.lblGateway.Text = "Gateway: ";
            this.lblGateway.AutoSize = true;
            this.lblGateway.Location = new System.Drawing.Point(15, 15);

            this.panelTop.Controls.Add(this.lblGateway);

            // ============================
            // MAIN SPLIT (LEFT: SENSORS, RIGHT: CHARTS)
            // ============================
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.splitMain.SplitterDistance = 400; // left side width

            // ============================
            // GROUP: SENSOR TABLE
            // ============================
            this.groupSensors = new System.Windows.Forms.GroupBox();
            this.groupSensors.Text = "Sensors";
            this.groupSensors.Dock = System.Windows.Forms.DockStyle.Fill;

            this.dataGridSensors = new System.Windows.Forms.DataGridView();
            this.dataGridSensors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridSensors.ReadOnly = true;
            this.dataGridSensors.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridSensors.MultiSelect = false;

            this.dataGridSensors.SelectionChanged += new System.EventHandler(this.dataGridSensors_SelectionChanged);

            this.groupSensors.Controls.Add(this.dataGridSensors);
            this.splitMain.Panel1.Controls.Add(this.groupSensors);

            // ============================
            // RIGHT SPLIT (Charts top, Pending bottom)
            // ============================
            this.splitRight = new System.Windows.Forms.SplitContainer();
            this.splitRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitRight.SplitterDistance = 350;

            this.splitMain.Panel2.Controls.Add(this.splitRight);

            // ============================
            // GROUP: CHARTS (TABCONTROL)
            // ============================
            this.groupCharts = new System.Windows.Forms.GroupBox();
            this.groupCharts.Text = "Charts";
            this.groupCharts.Dock = System.Windows.Forms.DockStyle.Fill;

            this.tabControlCharts = new System.Windows.Forms.TabControl();
            this.tabControlCharts.Dock = System.Windows.Forms.DockStyle.Fill;

            this.tabTemp = new System.Windows.Forms.TabPage();
            this.tabTemp.Text = "Temperature";

            this.tabHum = new System.Windows.Forms.TabPage();
            this.tabHum.Text = "Humidity";

            this.tabPress = new System.Windows.Forms.TabPage();
            this.tabPress.Text = "Pressure";

            // ScottPlot instances
            this.formsPlotTemp = new ScottPlot.WinForms.FormsPlot();
            this.formsPlotTemp.Dock = System.Windows.Forms.DockStyle.Fill;

            this.formsPlotHum = new ScottPlot.WinForms.FormsPlot();
            this.formsPlotHum.Dock = System.Windows.Forms.DockStyle.Fill;

            this.formsPlotPress = new ScottPlot.WinForms.FormsPlot();
            this.formsPlotPress.Dock = System.Windows.Forms.DockStyle.Fill;

            // Add to tabs
            this.tabTemp.Controls.Add(this.formsPlotTemp);
            this.tabHum.Controls.Add(this.formsPlotHum);
            this.tabPress.Controls.Add(this.formsPlotPress);

            this.tabControlCharts.Controls.Add(this.tabTemp);
            this.tabControlCharts.Controls.Add(this.tabHum);
            this.tabControlCharts.Controls.Add(this.tabPress);

            this.groupCharts.Controls.Add(this.tabControlCharts);
            this.splitRight.Panel1.Controls.Add(this.groupCharts);

            // ============================
            // GROUP: PENDING SENSORS
            // ============================
            this.groupPending = new System.Windows.Forms.GroupBox();
            this.groupPending.Text = "Pending Sensors";
            this.groupPending.Dock = System.Windows.Forms.DockStyle.Fill;

            this.dataGridPending = new System.Windows.Forms.DataGridView();
            this.dataGridPending.Dock = System.Windows.Forms.DockStyle.Top;
            this.dataGridPending.Height = 140;
            this.dataGridPending.ReadOnly = true;
            this.dataGridPending.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

            this.btnAccept = new System.Windows.Forms.Button();
            this.btnAccept.Text = "Accept";
            this.btnAccept.Width = 100;
            this.btnAccept.Location = new System.Drawing.Point(20, 150);

            this.btnIgnore = new System.Windows.Forms.Button();
            this.btnIgnore.Text = "Ignore";
            this.btnIgnore.Width = 100;
            this.btnIgnore.Location = new System.Drawing.Point(140, 150);

            this.groupPending.Controls.Add(this.dataGridPending);
            this.groupPending.Controls.Add(this.btnAccept);
            this.groupPending.Controls.Add(this.btnIgnore);

            this.splitRight.Panel2.Controls.Add(this.groupPending);

            // ============================
            // FORM CONFIG
            // ============================
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.panelTop);

            this.Text = "IoT Monitoring Dashboard";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        }
    }
}

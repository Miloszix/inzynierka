// FINAL MAINFORM DESIGNER – OPTION A
// Buttons under table on the left, 3 charts in tabControl on the right
// Fully working, stable, matching your sketch EXACTLY

namespace IoTClient
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.ComboBox comboGateway;
        private System.Windows.Forms.Label lblLastUpload;
        private System.Windows.Forms.Panel panelTop;

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.TableLayoutPanel leftLayout;

        private System.Windows.Forms.GroupBox groupSensors;
        private System.Windows.Forms.DataGridView dataGridSensors;

        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnTable;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Button btnIgnore;

        private System.Windows.Forms.GroupBox groupPending;
        private System.Windows.Forms.DataGridView dataGridPending;

        private System.Windows.Forms.GroupBox groupCharts;
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
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelTop = new Panel();
            comboGateway = new ComboBox();
            lblLastUpload = new Label();
            splitMain = new SplitContainer();
            leftLayout = new TableLayoutPanel();
            panel1 = new Panel();
            btnAccept = new Button();
            btnIgnore = new Button();
            groupSensors = new GroupBox();
            dataGridSensors = new DataGridView();
            panelButtons = new Panel();
            btn_rename = new Button();
            btnTable = new Button();
            btnSettings = new Button();
            groupPending = new GroupBox();
            dataGridPending = new DataGridView();
            groupCharts = new GroupBox();
            tabControlCharts = new TabControl();
            tabTemp = new TabPage();
            formsPlotTemp = new ScottPlot.WinForms.FormsPlot();
            tabHum = new TabPage();
            formsPlotHum = new ScottPlot.WinForms.FormsPlot();
            tabPress = new TabPage();
            formsPlotPress = new ScottPlot.WinForms.FormsPlot();
            panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            leftLayout.SuspendLayout();
            panel1.SuspendLayout();
            groupSensors.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridSensors).BeginInit();
            panelButtons.SuspendLayout();
            groupPending.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridPending).BeginInit();
            groupCharts.SuspendLayout();
            tabControlCharts.SuspendLayout();
            tabTemp.SuspendLayout();
            tabHum.SuspendLayout();
            tabPress.SuspendLayout();
            SuspendLayout();
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.FromArgb(40, 40, 40);
            panelTop.Controls.Add(comboGateway);
            panelTop.Controls.Add(lblLastUpload);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(1129, 45);
            panelTop.TabIndex = 1;
            // 
            // comboGateway
            // 
            comboGateway.DropDownStyle = ComboBoxStyle.DropDownList;
            comboGateway.Location = new Point(10, 10);
            comboGateway.Name = "comboGateway";
            comboGateway.Size = new Size(200, 23);
            comboGateway.TabIndex = 0;
            // 
            // lblLastUpload
            // 
            lblLastUpload.AutoSize = true;
            lblLastUpload.ForeColor = Color.Lime;
            lblLastUpload.Location = new Point(300, 12);
            lblLastUpload.Name = "lblLastUpload";
            lblLastUpload.Size = new Size(79, 15);
            lblLastUpload.TabIndex = 1;
            lblLastUpload.Text = "Last update: -";
            // 
            // splitMain
            // 
            splitMain.Dock = DockStyle.Fill;
            splitMain.FixedPanel = FixedPanel.Panel1;
            splitMain.Location = new Point(0, 45);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(leftLayout);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(groupCharts);
            splitMain.Size = new Size(1129, 891);
            splitMain.SplitterDistance = 667;
            splitMain.TabIndex = 0;
            // 
            // leftLayout
            // 
            leftLayout.ColumnCount = 1;
            leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            leftLayout.Controls.Add(panel1, 0, 3);
            leftLayout.Controls.Add(groupSensors, 0, 0);
            leftLayout.Controls.Add(panelButtons, 0, 1);
            leftLayout.Controls.Add(groupPending, 0, 2);
            leftLayout.Dock = DockStyle.Fill;
            leftLayout.Location = new Point(0, 0);
            leftLayout.Name = "leftLayout";
            leftLayout.RowCount = 4;
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 389F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 84F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
            leftLayout.Size = new Size(667, 891);
            leftLayout.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnAccept);
            panel1.Controls.Add(btnIgnore);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 808);
            panel1.Name = "panel1";
            panel1.Size = new Size(661, 80);
            panel1.TabIndex = 3;
            // 
            // btnAccept
            // 
            btnAccept.Location = new Point(10, 20);
            btnAccept.Name = "btnAccept";
            btnAccept.Size = new Size(100, 40);
            btnAccept.TabIndex = 2;
            btnAccept.Text = "Accept";
            btnAccept.Click += BtnAccept_Click;
            // 
            // btnIgnore
            // 
            btnIgnore.Location = new Point(116, 20);
            btnIgnore.Name = "btnIgnore";
            btnIgnore.Size = new Size(100, 40);
            btnIgnore.TabIndex = 3;
            btnIgnore.Text = "Ignore";
            btnIgnore.Click += BtnIgnore_Click;
            // 
            // groupSensors
            // 
            groupSensors.Controls.Add(dataGridSensors);
            groupSensors.Dock = DockStyle.Fill;
            groupSensors.Location = new Point(3, 3);
            groupSensors.Name = "groupSensors";
            groupSensors.Size = new Size(661, 383);
            groupSensors.TabIndex = 0;
            groupSensors.TabStop = false;
            groupSensors.Text = "Sensors";
            // 
            // dataGridSensors
            // 
            dataGridSensors.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridSensors.Dock = DockStyle.Fill;
            dataGridSensors.Location = new Point(3, 19);
            dataGridSensors.MinimumSize = new Size(300, 200);
            dataGridSensors.MultiSelect = false;
            dataGridSensors.Name = "dataGridSensors";
            dataGridSensors.ReadOnly = true;
            dataGridSensors.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridSensors.Size = new Size(655, 361);
            dataGridSensors.TabIndex = 0;
            dataGridSensors.SelectionChanged += dataGridSensors_SelectionChanged;
            // 
            // panelButtons
            // 
            panelButtons.Controls.Add(btn_rename);
            panelButtons.Controls.Add(btnTable);
            panelButtons.Controls.Add(btnSettings);
            panelButtons.Dock = DockStyle.Fill;
            panelButtons.Location = new Point(3, 392);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(661, 78);
            panelButtons.TabIndex = 1;
            // 
            // btn_rename
            // 
            btn_rename.Location = new Point(215, 20);
            btn_rename.Name = "btn_rename";
            btn_rename.Size = new Size(100, 40);
            btn_rename.TabIndex = 4;
            btn_rename.Text = "Rename";
            btn_rename.Click += btn_rename_click;
            // 
            // btnTable
            // 
            btnTable.Location = new Point(3, 20);
            btnTable.Name = "btnTable";
            btnTable.Size = new Size(100, 40);
            btnTable.TabIndex = 0;
            btnTable.Text = "Measurements";
            btnTable.Click += btn_measurments_clicked;
            // 
            // btnSettings
            // 
            btnSettings.Location = new Point(109, 20);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(100, 40);
            btnSettings.TabIndex = 1;
            btnSettings.Text = "Settings";
            btnSettings.Click += BtnSettings_Click;
            // 
            // groupPending
            // 
            groupPending.Controls.Add(dataGridPending);
            groupPending.Dock = DockStyle.Fill;
            groupPending.Location = new Point(3, 476);
            groupPending.Name = "groupPending";
            groupPending.Size = new Size(661, 326);
            groupPending.TabIndex = 2;
            groupPending.TabStop = false;
            groupPending.Text = "Pending Sensors";
            groupPending.Enter += groupPending_Enter;
            // 
            // dataGridPending
            // 
            dataGridPending.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridPending.Dock = DockStyle.Fill;
            dataGridPending.Location = new Point(3, 19);
            dataGridPending.Name = "dataGridPending";
            dataGridPending.ReadOnly = true;
            dataGridPending.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridPending.Size = new Size(655, 304);
            dataGridPending.TabIndex = 0;
            // 
            // groupCharts
            // 
            groupCharts.BackColor = Color.WhiteSmoke;
            groupCharts.Controls.Add(tabControlCharts);
            groupCharts.Dock = DockStyle.Fill;
            groupCharts.Location = new Point(0, 0);
            groupCharts.Name = "groupCharts";
            groupCharts.Size = new Size(458, 891);
            groupCharts.TabIndex = 0;
            groupCharts.TabStop = false;
            groupCharts.Text = "Charts";
            // 
            // tabControlCharts
            // 
            tabControlCharts.Controls.Add(tabTemp);
            tabControlCharts.Controls.Add(tabHum);
            tabControlCharts.Controls.Add(tabPress);
            tabControlCharts.Dock = DockStyle.Fill;
            tabControlCharts.Location = new Point(3, 19);
            tabControlCharts.Name = "tabControlCharts";
            tabControlCharts.SelectedIndex = 0;
            tabControlCharts.Size = new Size(452, 869);
            tabControlCharts.TabIndex = 0;
            // 
            // tabTemp
            // 
            tabTemp.Controls.Add(formsPlotTemp);
            tabTemp.Location = new Point(4, 24);
            tabTemp.Name = "tabTemp";
            tabTemp.Size = new Size(444, 841);
            tabTemp.TabIndex = 0;
            tabTemp.Text = "Temperature";
            // 
            // formsPlotTemp
            // 
            formsPlotTemp.DisplayScale = 1F;
            formsPlotTemp.Dock = DockStyle.Fill;
            formsPlotTemp.Location = new Point(0, 0);
            formsPlotTemp.Name = "formsPlotTemp";
            formsPlotTemp.Size = new Size(444, 841);
            formsPlotTemp.TabIndex = 0;
            // 
            // tabHum
            // 
            tabHum.Controls.Add(formsPlotHum);
            tabHum.Location = new Point(4, 24);
            tabHum.Name = "tabHum";
            tabHum.Size = new Size(444, 841);
            tabHum.TabIndex = 1;
            tabHum.Text = "Humidity";
            // 
            // formsPlotHum
            // 
            formsPlotHum.DisplayScale = 1F;
            formsPlotHum.Dock = DockStyle.Fill;
            formsPlotHum.Location = new Point(0, 0);
            formsPlotHum.Name = "formsPlotHum";
            formsPlotHum.Size = new Size(444, 841);
            formsPlotHum.TabIndex = 0;
            // 
            // tabPress
            // 
            tabPress.Controls.Add(formsPlotPress);
            tabPress.Location = new Point(4, 24);
            tabPress.Name = "tabPress";
            tabPress.Size = new Size(444, 841);
            tabPress.TabIndex = 2;
            tabPress.Text = "Pressure";
            // 
            // formsPlotPress
            // 
            formsPlotPress.DisplayScale = 1F;
            formsPlotPress.Dock = DockStyle.Fill;
            formsPlotPress.Location = new Point(0, 0);
            formsPlotPress.Name = "formsPlotPress";
            formsPlotPress.Size = new Size(444, 841);
            formsPlotPress.TabIndex = 0;
            // 
            // MainForm
            // 
            ClientSize = new Size(1129, 936);
            Controls.Add(splitMain);
            Controls.Add(panelTop);
            Name = "MainForm";
            Text = "IoT Monitoring Dashboard";
            WindowState = FormWindowState.Maximized;
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            leftLayout.ResumeLayout(false);
            panel1.ResumeLayout(false);
            groupSensors.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridSensors).EndInit();
            panelButtons.ResumeLayout(false);
            groupPending.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridPending).EndInit();
            groupCharts.ResumeLayout(false);
            tabControlCharts.ResumeLayout(false);
            tabTemp.ResumeLayout(false);
            tabHum.ResumeLayout(false);
            tabPress.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Button btn_rename;
        private Panel panel1;
    }
}

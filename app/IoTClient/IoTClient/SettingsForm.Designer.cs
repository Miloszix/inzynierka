namespace IoTClient
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel panelSensors;
        private Label lblSensors;
        private DataGridView dataSensors;
        private Button btnEditSensor;

        private Panel panelGateways;
        private Label lblGateways;
        private DataGridView dataGateways;
        private Button btnAddGateway;
        private Button btnDeleteGateway;

        private Button btnAcceptSensor;
        private Button btnIgnoreSensor;


        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelSensors = new Panel();
            lblSensors = new Label();
            dataSensors = new DataGridView();
            btnEditSensor = new Button();
            panelGateways = new Panel();
            lblGateways = new Label();
            dataGateways = new DataGridView();
            btnAddGateway = new Button();
            btnDeleteGateway = new Button();
            btnAcceptSensor = new Button();
            btnIgnoreSensor = new Button();
            panelSensors.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataSensors).BeginInit();
            panelGateways.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGateways).BeginInit();
            SuspendLayout();
            // 
            // panelSensors
            // 
            panelSensors.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            panelSensors.BackColor = Color.FromArgb(40, 40, 40);
            panelSensors.BorderStyle = BorderStyle.FixedSingle;
            panelSensors.Controls.Add(btnIgnoreSensor);
            panelSensors.Controls.Add(btnAcceptSensor);
            panelSensors.Controls.Add(lblSensors);
            panelSensors.Controls.Add(dataSensors);
            panelSensors.Controls.Add(btnEditSensor);
            panelSensors.Location = new Point(20, 20);
            panelSensors.Name = "panelSensors";
            panelSensors.Size = new Size(420, 520);
            panelSensors.TabIndex = 2;
            // 
            // lblSensors
            // 
            lblSensors.AutoSize = true;
            lblSensors.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblSensors.ForeColor = Color.White;
            lblSensors.Location = new Point(10, 10);
            lblSensors.Name = "lblSensors";
            lblSensors.Size = new Size(63, 20);
            lblSensors.TabIndex = 0;
            lblSensors.Text = "Sensors";
            // 
            // dataSensors
            // 
            dataSensors.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataSensors.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataSensors.Location = new Point(10, 40);
            dataSensors.Name = "dataSensors";
            dataSensors.ReadOnly = true;
            dataSensors.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataSensors.Size = new Size(394, 398);
            dataSensors.TabIndex = 1;
            // 
            // btnEditSensor
            // 
            btnEditSensor.Location = new Point(10, 450);
            btnEditSensor.Name = "btnEditSensor";
            btnEditSensor.Size = new Size(120, 40);
            btnEditSensor.TabIndex = 2;
            btnEditSensor.Text = "Edit";
            btnEditSensor.Click += btnEditSensor_Click;
            // 
            // panelGateways
            // 
            panelGateways.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelGateways.BackColor = Color.FromArgb(40, 40, 40);
            panelGateways.BorderStyle = BorderStyle.FixedSingle;
            panelGateways.Controls.Add(lblGateways);
            panelGateways.Controls.Add(dataGateways);
            panelGateways.Controls.Add(btnAddGateway);
            panelGateways.Controls.Add(btnDeleteGateway);
            panelGateways.Location = new Point(460, 20);
            panelGateways.Name = "panelGateways";
            panelGateways.Size = new Size(420, 520);
            panelGateways.TabIndex = 3;
            // 
            // lblGateways
            // 
            lblGateways.AutoSize = true;
            lblGateways.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblGateways.ForeColor = Color.White;
            lblGateways.Location = new Point(10, 10);
            lblGateways.Name = "lblGateways";
            lblGateways.Size = new Size(77, 20);
            lblGateways.TabIndex = 0;
            lblGateways.Text = "Gateways";
            // 
            // dataGateways
            // 
            dataGateways.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGateways.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGateways.Location = new Point(10, 40);
            dataGateways.Name = "dataGateways";
            dataGateways.ReadOnly = true;
            dataGateways.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGateways.Size = new Size(394, 398);
            dataGateways.TabIndex = 1;
            // 
            // btnAddGateway
            // 
            btnAddGateway.Location = new Point(10, 450);
            btnAddGateway.Name = "btnAddGateway";
            btnAddGateway.Size = new Size(100, 40);
            btnAddGateway.TabIndex = 2;
            btnAddGateway.Text = "Add";
            btnAddGateway.Click += btnAddGateway_Click;
            // 
            // btnDeleteGateway
            // 
            btnDeleteGateway.Location = new Point(120, 450);
            btnDeleteGateway.Name = "btnDeleteGateway";
            btnDeleteGateway.Size = new Size(100, 40);
            btnDeleteGateway.TabIndex = 3;
            btnDeleteGateway.Text = "Delete";
            btnDeleteGateway.Click += btnDeleteGateway_Click;
            // 
            // btnAcceptSensor
            // 
            btnAcceptSensor.Location = new Point(136, 450);
            btnAcceptSensor.Name = "btnAcceptSensor";
            btnAcceptSensor.Size = new Size(153, 40);
            btnAcceptSensor.TabIndex = 0;
            btnAcceptSensor.Text = "Accept";
            btnAcceptSensor.Click += btnAcceptSensor_Click;
            // 
            // btnIgnoreSensor
            // 
            btnIgnoreSensor.Location = new Point(295, 450);
            btnIgnoreSensor.Name = "btnIgnoreSensor";
            btnIgnoreSensor.Size = new Size(109, 40);
            btnIgnoreSensor.TabIndex = 1;
            btnIgnoreSensor.Text = "Ignore";
            btnIgnoreSensor.Click += btnIgnoreSensor_Click;
            // 
            // SettingsForm
            // 
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(900, 560);
            Controls.Add(panelSensors);
            Controls.Add(panelGateways);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "SettingsForm";
            Text = "Settings";
            panelSensors.ResumeLayout(false);
            panelSensors.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataSensors).EndInit();
            panelGateways.ResumeLayout(false);
            panelGateways.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGateways).EndInit();
            ResumeLayout(false);

            StyleGrid(dataSensors);
            StyleGrid(dataGateways);

            StyleButton(btnEditSensor);
            StyleButton(btnAddGateway);
            StyleButton(btnDeleteGateway);
            StyleButton(btnAcceptSensor);
            StyleButton(btnIgnoreSensor);

            lblSensors.ForeColor = Color.White;
            lblGateways.ForeColor = Color.White;

            // panel tła
            panelSensors.BackColor = Color.FromArgb(40, 40, 40);
            panelGateways.BackColor = Color.FromArgb(40, 40, 40);

            // ładowanie danych
            dataGateways.SelectionChanged += dataGateways_SelectionChanged;
            LoadGateways();
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

        private void StyleButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = Color.FromArgb(55, 55, 55);
            b.ForeColor = Color.White;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(75, 75, 75);
            b.FlatAppearance.MouseDownBackColor = Color.FromArgb(95, 95, 95);
        }

    }
}

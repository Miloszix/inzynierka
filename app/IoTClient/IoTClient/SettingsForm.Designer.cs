namespace IoTClient
{
    partial class SettingsForm
    {
        private System.Windows.Forms.DataGridView dataSensors;
        private System.Windows.Forms.Button btnEditSensor;

        private System.Windows.Forms.DataGridView dataGateways;
        private System.Windows.Forms.Button btnAddGateway;
        private System.Windows.Forms.Button btnDeleteGateway;

        private void InitializeComponent()
        {
            this.dataSensors = new DataGridView();
            this.btnEditSensor = new Button();

            this.dataGateways = new DataGridView();
            this.btnAddGateway = new Button();
            this.btnDeleteGateway = new Button();

            this.SuspendLayout();

            // LEFT — SENSORS LIST
            dataSensors.Location = new Point(20, 40);
            dataSensors.Size = new Size(400, 450);

            btnEditSensor.Text = "Edit";
            btnEditSensor.Location = new Point(20, 500);
            btnEditSensor.Click += btnEditSensor_Click;

            // RIGHT — GATEWAYS LIST
            dataGateways.Location = new Point(450, 40);
            dataGateways.Size = new Size(400, 450);

            btnAddGateway.Text = "Add";
            btnAddGateway.Location = new Point(450, 500);
            btnAddGateway.Click += btnAddGateway_Click;

            btnDeleteGateway.Text = "Delete";
            btnDeleteGateway.Location = new Point(550, 500);
            btnDeleteGateway.Click += btnDeleteGateway_Click;

            // WINDOW
            this.ClientSize = new Size(900, 560);
            this.Controls.Add(dataSensors);
            this.Controls.Add(btnEditSensor);
            this.Controls.Add(dataGateways);
            this.Controls.Add(btnAddGateway);
            this.Controls.Add(btnDeleteGateway);
            this.Text = "Settings";

            this.ResumeLayout(false);
        }

    }
}
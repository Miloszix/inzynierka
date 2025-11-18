namespace IoTClient
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dataGridSensors = new DataGridView();
            dataGridMeasurements = new DataGridView();
            txtSensorName = new TextBox();
            btnAcceptSensor = new Button();
            txtSensorMac = new TextBox();
            btnRename = new Button();
            lblGateway = new Label();
            ((System.ComponentModel.ISupportInitialize)dataGridSensors).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridMeasurements).BeginInit();
            SuspendLayout();
            // 
            // dataGridSensors
            // 
            dataGridSensors.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridSensors.Location = new Point(12, 12);
            dataGridSensors.Name = "dataGridSensors";
            dataGridSensors.Size = new Size(240, 150);
            dataGridSensors.TabIndex = 0;
            // 
            // dataGridMeasurements
            // 
            dataGridMeasurements.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridMeasurements.Location = new Point(548, 12);
            dataGridMeasurements.Name = "dataGridMeasurements";
            dataGridMeasurements.Size = new Size(240, 150);
            dataGridMeasurements.TabIndex = 1;
            dataGridMeasurements.CellContentClick += dataGridViewMeasurements_CellContentClick;
            // 
            // txtSensorName
            // 
            txtSensorName.Location = new Point(12, 198);
            txtSensorName.Name = "txtSensorName";
            txtSensorName.Size = new Size(100, 23);
            txtSensorName.TabIndex = 2;
            // 
            // btnAcceptSensor
            // 
            btnAcceptSensor.Location = new Point(713, 198);
            btnAcceptSensor.Name = "btnAcceptSensor";
            btnAcceptSensor.Size = new Size(75, 23);
            btnAcceptSensor.TabIndex = 3;
            btnAcceptSensor.Text = "button1";
            btnAcceptSensor.UseVisualStyleBackColor = true;
            // 
            // txtSensorMac
            // 
            txtSensorMac.Location = new Point(12, 263);
            txtSensorMac.Name = "txtSensorMac";
            txtSensorMac.Size = new Size(100, 23);
            txtSensorMac.TabIndex = 4;
            // 
            // btnRename
            // 
            btnRename.Location = new Point(713, 263);
            btnRename.Name = "btnRename";
            btnRename.Size = new Size(75, 23);
            btnRename.TabIndex = 5;
            btnRename.Text = "button1";
            btnRename.UseVisualStyleBackColor = true;
            // 
            // lblGateway
            // 
            lblGateway.AutoSize = true;
            lblGateway.Location = new Point(372, 32);
            lblGateway.Name = "lblGateway";
            lblGateway.Size = new Size(38, 15);
            lblGateway.TabIndex = 6;
            lblGateway.Text = "label1";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lblGateway);
            Controls.Add(btnRename);
            Controls.Add(txtSensorMac);
            Controls.Add(btnAcceptSensor);
            Controls.Add(txtSensorName);
            Controls.Add(dataGridMeasurements);
            Controls.Add(dataGridSensors);
            Name = "MainForm";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)dataGridSensors).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridMeasurements).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridSensors;
        private DataGridView dataGridMeasurements;
        private TextBox txtSensorName;
        private Button btnAcceptSensor;
        private TextBox txtSensorMac;
        private Button btnRename;
        private Label lblGateway;
    }
}

namespace IoTClient
{
    partial class RenameForm
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblInfo;
        private TextBox txtName;
        private Button btnOk;
        private Button btnCancel;
        private TableLayoutPanel layout;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblInfo = new Label();
            txtName = new TextBox();
            btnOk = new Button();
            btnCancel = new Button();
            layout = new TableLayoutPanel();
            bottomPanel = new FlowLayoutPanel();
            layout.SuspendLayout();
            bottomPanel.SuspendLayout();
            SuspendLayout();
            // 
            // lblInfo
            // 
            lblInfo.Dock = DockStyle.Fill;
            lblInfo.Font = new Font("Segoe UI", 10.5F);
            lblInfo.ForeColor = Color.White;
            lblInfo.Location = new Point(18, 15);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(384, 68);
            lblInfo.TabIndex = 0;
            lblInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtName
            // 
            txtName.BackColor = Color.FromArgb(50, 50, 50);
            txtName.BorderStyle = BorderStyle.FixedSingle;
            txtName.Dock = DockStyle.Fill;
            txtName.Font = new Font("Segoe UI", 11F);
            txtName.ForeColor = Color.White;
            txtName.Location = new Point(18, 86);
            txtName.Name = "txtName";
            txtName.Size = new Size(384, 27);
            txtName.TabIndex = 1;
            // 
            // btnOk
            // 
            btnOk.Anchor = AnchorStyles.Right;
            btnOk.BackColor = Color.FromArgb(60, 160, 80);
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.ForeColor = Color.White;
            btnOk.Location = new Point(271, 13);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(110, 40);
            btnOk.TabIndex = 0;
            btnOk.Text = "Zapisz";
            btnOk.UseVisualStyleBackColor = false;
            btnOk.Click += btnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Left;
            btnCancel.BackColor = Color.FromArgb(120, 120, 120);
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(155, 13);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(110, 40);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Anuluj";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += btnCancel_Click;
            // 
            // layout
            // 
            layout.ColumnCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            layout.Controls.Add(lblInfo, 0, 0);
            layout.Controls.Add(txtName, 0, 1);
            layout.Controls.Add(bottomPanel, 0, 2);
            layout.Dock = DockStyle.Fill;
            layout.Location = new Point(0, 0);
            layout.Name = "layout";
            layout.Padding = new Padding(15);
            layout.RowCount = 3;
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            layout.Size = new Size(420, 200);
            layout.TabIndex = 0;
            // 
            // bottomPanel
            // 
            bottomPanel.Controls.Add(btnOk);
            bottomPanel.Controls.Add(btnCancel);
            bottomPanel.Dock = DockStyle.Fill;
            bottomPanel.FlowDirection = FlowDirection.RightToLeft;
            bottomPanel.Location = new Point(18, 137);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Padding = new Padding(0, 10, 0, 0);
            bottomPanel.Size = new Size(384, 45);
            bottomPanel.TabIndex = 2;
            // 
            // RenameForm
            // 
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(420, 200);
            Controls.Add(layout);
            Font = new Font("Segoe UI", 10F);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "RenameForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Zmień nazwę sensora";
            layout.ResumeLayout(false);
            layout.PerformLayout();
            bottomPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private FlowLayoutPanel bottomPanel;
    }
}

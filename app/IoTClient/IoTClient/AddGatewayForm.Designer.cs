namespace IoTClient
{
    partial class AddGatewayForm
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblId;
        private Label lblName;

        private TextBox txtId;
        private TextBox txtName;

        private Button btnOk;
        private Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblId = new Label();
            this.lblName = new Label();

            this.txtId = new TextBox();
            this.txtName = new TextBox();

            this.btnOk = new Button();
            this.btnCancel = new Button();

            this.SuspendLayout();

            // =====================
            // FORM
            // =====================
            this.Text = "Add Gateway";
            this.ClientSize = new Size(300, 210);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.BackColor = Color.FromArgb(32, 32, 32);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // =====================
            // LABEL ID
            // =====================
            lblId.Text = "Gateway ID:";
            lblId.ForeColor = Color.White;
            lblId.Location = new Point(20, 20);
            lblId.AutoSize = true;

            // TEXTBOX ID
            txtId.Location = new Point(20, 45);
            txtId.Width = 250;
            txtId.BackColor = Color.FromArgb(45, 45, 45);
            txtId.ForeColor = Color.White;
            txtId.BorderStyle = BorderStyle.FixedSingle;

            // =====================
            // LABEL NAME
            // =====================
            lblName.Text = "Display Name:";
            lblName.ForeColor = Color.White;
            lblName.Location = new Point(20, 80);
            lblName.AutoSize = true;

            // TEXTBOX NAME
            txtName.Location = new Point(20, 105);
            txtName.Width = 250;
            txtName.BackColor = Color.FromArgb(45, 45, 45);
            txtName.ForeColor = Color.White;
            txtName.BorderStyle = BorderStyle.FixedSingle;

            // =====================
            // OK BUTTON
            // =====================
            btnOk.Text = "OK";
            btnOk.Location = new Point(20, 150);
            btnOk.Size = new Size(100, 35);
            StyleButton(btnOk);
            btnOk.Click += btnOk_Click;

            // =====================
            // CANCEL BUTTON
            // =====================
            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(170, 150);
            btnCancel.Size = new Size(100, 35);
            StyleButton(btnCancel);
            btnCancel.Click += (s, e) => this.Close();

            // =====================
            // ADD CONTROLS
            // =====================
            this.Controls.Add(lblId);
            this.Controls.Add(txtId);

            this.Controls.Add(lblName);
            this.Controls.Add(txtName);

            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);

            this.ResumeLayout(false);
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

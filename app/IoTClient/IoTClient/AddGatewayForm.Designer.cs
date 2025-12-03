namespace IoTClient
{
    partial class AddGatewayForm
    {
        private TextBox txtId;
        private TextBox txtName;
        private Button btnOk;
        private Button btnCancel;

        private void InitializeComponent()
        {
            txtId = new TextBox();
            txtName = new TextBox();
            btnOk = new Button();
            btnCancel = new Button();

            txtId.Location = new Point(30, 30);
            txtId.Width = 200;

            txtName.Location = new Point(30, 80);
            txtName.Width = 200;

            btnOk.Text = "OK";
            btnOk.Location = new Point(30, 130);
            btnOk.Click += btnOk_Click;

            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(120, 130);
            btnCancel.Click += (s, e) => this.Close();

            this.Text = "Add Gateway";
            this.ClientSize = new Size(260, 180);
            this.Controls.Add(txtId);
            this.Controls.Add(txtName);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);
        }

    }
}
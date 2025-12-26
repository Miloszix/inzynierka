namespace IoTClient
{
    partial class RegisterForm
    {
        private TextBox txtUser;
        private TextBox txtPass;
        private TextBox txtPassConfirm;
        private Button btnRegister;
        private Label lblUser;
        private Label lblPass;
        private Label lblPassConfirm;

        private void InitializeComponent()
        {
            txtUser = new TextBox();
            txtPass = new TextBox();
            txtPassConfirm = new TextBox();
            btnRegister = new Button();
            lblUser = new Label();
            lblPass = new Label();
            lblPassConfirm = new Label();

            SuspendLayout();

            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(360, 260);
            Text = "Register";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            // USERNAME
            lblUser.Text = "Username:";
            lblUser.ForeColor = Color.White;
            lblUser.Location = new Point(30, 30);
            lblUser.AutoSize = true;

            txtUser.Location = new Point(150, 28);
            txtUser.Width = 160;
            txtUser.BackColor = Color.FromArgb(45, 45, 45);
            txtUser.ForeColor = Color.White;

            // PASSWORD
            lblPass.Text = "Password:";
            lblPass.ForeColor = Color.White;
            lblPass.Location = new Point(30, 80);
            lblPass.AutoSize = true;

            txtPass.Location = new Point(150, 78);
            txtPass.Width = 160;
            txtPass.PasswordChar = '*';
            txtPass.BackColor = Color.FromArgb(45, 45, 45);
            txtPass.ForeColor = Color.White;

            // CONFIRM PASSWORD
            lblPassConfirm.Text = "Confirm:";
            lblPassConfirm.ForeColor = Color.White;
            lblPassConfirm.Location = new Point(30, 130);
            lblPassConfirm.AutoSize = true;

            txtPassConfirm.Location = new Point(150, 128);
            txtPassConfirm.Width = 160;
            txtPassConfirm.PasswordChar = '*';
            txtPassConfirm.BackColor = Color.FromArgb(45, 45, 45);
            txtPassConfirm.ForeColor = Color.White;

            // BUTTON
            btnRegister.Text = "CREATE ACCOUNT";
            btnRegister.Location = new Point(150, 180);
            btnRegister.Size = new Size(160, 35);
            btnRegister.Click += btnRegister_Click;
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.BackColor = Color.FromArgb(60, 60, 60);
            btnRegister.ForeColor = Color.White;

            Controls.Add(lblUser);
            Controls.Add(txtUser);
            Controls.Add(lblPass);
            Controls.Add(txtPass);
            Controls.Add(lblPassConfirm);
            Controls.Add(txtPassConfirm);
            Controls.Add(btnRegister);

            ResumeLayout(false);
        }
    }
}

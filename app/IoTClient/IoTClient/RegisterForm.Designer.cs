namespace IoTClient
{
    partial class RegisterForm
    {
        private TextBox txtUser;
        private TextBox txtPass;
        private TextBox txtEmail;
        private Button btnRegister;
        private Label lblUser;
        private Label lblPass;
        private Label lblEmail;

        private void InitializeComponent()
        {
            txtUser = new TextBox();
            txtPass = new TextBox();
            txtEmail = new TextBox();
            btnRegister = new Button();
            lblUser = new Label();
            lblPass = new Label();
            lblEmail = new Label();

            SuspendLayout();

            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(360, 260);
            Text = "Register";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            lblUser.Text = "Username:";
            lblUser.ForeColor = Color.White;
            lblUser.Location = new Point(30, 30);
            lblUser.AutoSize = true;

            txtUser.Location = new Point(130, 28);
            txtUser.Width = 180;
            txtUser.BackColor = Color.FromArgb(45, 45, 45);
            txtUser.ForeColor = Color.White;

            lblPass.Text = "Password:";
            lblPass.ForeColor = Color.White;
            lblPass.Location = new Point(30, 80);
            lblPass.AutoSize = true;

            txtPass.Location = new Point(130, 78);
            txtPass.Width = 180;
            txtPass.PasswordChar = '*';
            txtPass.BackColor = Color.FromArgb(45, 45, 45);
            txtPass.ForeColor = Color.White;

            lblEmail.Text = "Email:";
            lblEmail.ForeColor = Color.White;
            lblEmail.Location = new Point(30, 130);
            lblEmail.AutoSize = true;

            txtEmail.Location = new Point(130, 128);
            txtEmail.Width = 180;
            txtEmail.BackColor = Color.FromArgb(45, 45, 45);
            txtEmail.ForeColor = Color.White;

            btnRegister.Text = "CREATE ACCOUNT";
            btnRegister.Location = new Point(130, 180);
            btnRegister.Size = new Size(180, 35);
            btnRegister.Click += btnRegister_Click;
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.BackColor = Color.FromArgb(60, 60, 60);
            btnRegister.ForeColor = Color.White;

            Controls.Add(lblUser);
            Controls.Add(txtUser);
            Controls.Add(lblPass);
            Controls.Add(txtPass);
            Controls.Add(lblEmail);
            Controls.Add(txtEmail);
            Controls.Add(btnRegister);

            ResumeLayout(false);
        }
    }

}
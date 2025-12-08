namespace IoTClient
{
    partial class LoginForm
    {
        private Label lblUser;
        private Label lblPass;
        private Button btnRegister;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private void InitializeComponent()
        {
            lblUser = new Label();
            lblPass = new Label();
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            btnLogin = new Button();
            btnRegister = new Button();

            SuspendLayout();

            // --- FORM ---
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(380, 260);
            Text = "Login";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            // --- LABEL USER ---
            lblUser.Text = "Username:";
            lblUser.ForeColor = Color.White;
            lblUser.Location = new Point(40, 40);
            lblUser.AutoSize = true;

            // --- TEXTBOX USER ---
            txtUsername.Location = new Point(140, 38);
            txtUsername.Width = 180;
            txtUsername.BackColor = Color.FromArgb(45, 45, 45);
            txtUsername.ForeColor = Color.White;
            txtUsername.BorderStyle = BorderStyle.FixedSingle;

            // --- LABEL PASSWORD ---
            lblPass.Text = "Password:";
            lblPass.ForeColor = Color.White;
            lblPass.Location = new Point(40, 90);
            lblPass.AutoSize = true;

            // --- TEXTBOX PASSWORD ---
            txtPassword.Location = new Point(140, 88);
            txtPassword.Width = 180;
            txtPassword.PasswordChar = '*';
            txtPassword.BackColor = Color.FromArgb(45, 45, 45);
            txtPassword.ForeColor = Color.White;
            txtPassword.BorderStyle = BorderStyle.FixedSingle;

            // --- LOGIN BUTTON ---
            btnLogin.Text = "LOGIN";
            btnLogin.Location = new Point(140, 140);
            btnLogin.Size = new Size(180, 35);
            btnLogin.Click += btnLogin_Click;
            StyleButton(btnLogin);

            // --- REGISTER BUTTON ---
            btnRegister.Text = "REGISTER";
            btnRegister.Location = new Point(140, 185);
            btnRegister.Size = new Size(180, 35);
            btnRegister.Click += btnRegister_Click;
            StyleButton(btnRegister);

            Controls.Add(lblUser);
            Controls.Add(txtUsername);
            Controls.Add(lblPass);
            Controls.Add(txtPassword);
            Controls.Add(btnLogin);
            Controls.Add(btnRegister);

            ResumeLayout(false);
        }

        private void StyleButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = Color.FromArgb(60, 60, 60);
            b.ForeColor = Color.White;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 80, 80);
            b.FlatAppearance.MouseDownBackColor = Color.FromArgb(100, 100, 100);
        }

    }
}
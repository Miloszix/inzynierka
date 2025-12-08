using System.Net.Http.Json;

namespace IoTClient
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = txtPass.Text.Trim();
            string email = txtEmail.Text.Trim();

            if (username == "" || password == "")
            {
                MessageBox.Show("Username and password required.");
                return;
            }

            using var client = new HttpClient();

            var payload = new
            {
                username,
                password,
                email
            };

            var res = await client.PostAsJsonAsync("http://3.70.126.6:1880/register", payload);

            if (res.IsSuccessStatusCode)
            {
                MessageBox.Show("Account created!");
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Registration failed.");
            }
        }
    }
}

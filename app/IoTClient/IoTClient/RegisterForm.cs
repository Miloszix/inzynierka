using System.Net;
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
            string pass1 = txtPass.Text;
            string pass2 = txtPassConfirm.Text;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(pass1) ||
                string.IsNullOrWhiteSpace(pass2))
            {
                MessageBox.Show("All fields are required.");
                return;
            }

            if (pass1 != pass2)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            using var client = new HttpClient();

            var payload = new
            {
                username,
                password = pass1
            };

            HttpResponseMessage res;

            try
            {
                res = await client.PostAsJsonAsync(
                    "http://3.70.126.6:1880/register", payload);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection error: " + ex.Message);
                return;
            }

            if (res.StatusCode == HttpStatusCode.Created)
            {
                MessageBox.Show("Account created!");
                DialogResult = DialogResult.OK;
                Close();
            }
            else if (res.StatusCode == HttpStatusCode.Conflict)
            {
                MessageBox.Show("Username already exists.");
            }
            else
            {
                MessageBox.Show("Registration failed.");
            }
        }
    }
}

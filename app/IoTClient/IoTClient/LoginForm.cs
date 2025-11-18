using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IoTClient
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (username == "" || password == "")
            {
                MessageBox.Show("Please enter login and password.");
                return;
            }

            bool success = await LoginAsync(username, password);

            if (success)
            {
                // CREATE MAIN FORM
                MainForm m = new MainForm(Session.GatewayId);

                m.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Login failed. Wrong username or password.");
            }
        }

        private async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                using var client = new HttpClient();

                var payload = new
                {
                    username = username,
                    password = password
                };

                var response = await client.PostAsJsonAsync(
                    "http://3.70.126.6:1880/login", payload);

                if (!response.IsSuccessStatusCode)
                    return false;

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result != null && result.success)
                {
                    Session.Token = result.token;
                    Session.GatewayId = result.gateway_id;

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                return false;
            }
        }
    }

    public class LoginResponse
    {
        public bool success { get; set; }
        public string token { get; set; }
        public string gateway_id { get; set; }
    }
}

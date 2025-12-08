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

        private void btnRegister_Click(object sender, EventArgs e)
        {
            var f = new RegisterForm();
            f.ShowDialog();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter login and password.");
                return;
            }

            btnLogin.Enabled = false;

            bool success = await LoginAsync(username, password);

            if (!success)
            {
                MessageBox.Show("Login failed. Wrong username or password.");
                btnLogin.Enabled = true;
                return;
            }

            // 🔥 Pobierz gatewaye użytkownika
            await LoadUserGateways();

            // Nawet jeśli użytkownik nie ma gatewayów → MainForm i tak się otworzy
            MainForm main = new MainForm();
            main.Show();
            this.Hide();
        }

        private async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                using var client = new HttpClient();
                var payload = new { username, password };

                var response = await client.PostAsJsonAsync(
                    "http://3.70.126.6:1880/login", payload);

                if (!response.IsSuccessStatusCode)
                    return false;

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result != null && result.success)
                {
                    Session.Token = result.token;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Network error: " + ex.Message);
                return false;
            }
        }

        private async Task LoadUserGateways()
        {
            try
            {
                using var client = new HttpClient();

                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", Session.Token);

                var gateways = await client.GetFromJsonAsync<List<GatewayItem>>(
                    "http://3.70.126.6:1880/user/gateways");

                Session.Gateways = gateways ?? new List<GatewayItem>();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load user gateways: " + ex.Message);
                Session.Gateways = new List<GatewayItem>();
            }
        }
    }

    public class LoginResponse
    {
        public bool success { get; set; }
        public string token { get; set; }
    }
}

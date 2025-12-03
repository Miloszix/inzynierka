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

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter login and password.");
                return;
            }

            bool success = await LoginAsync(username, password);

            if (!success)
            {
                MessageBox.Show("Login failed. Wrong username or password.");
                return;
            }

            // 🔥 Po zalogowaniu pobierz listę gatewayów użytkownika
            await LoadUserGateways();

            if (Session.Gateways.Count == 0)
            {
                MessageBox.Show("You do not have any assigned gateways!");
                // Możemy i tak włączyć MainForm bez gateway_id
                MainForm m0 = new MainForm(null);
                m0.Show();
                this.Hide();
                return;
            }

            // Domyślnie wybieramy pierwszego
            Session.GatewayId = Session.Gateways[0].gateway_id;

            MainForm m = new MainForm(Session.GatewayId);
            m.Show();
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
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);

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

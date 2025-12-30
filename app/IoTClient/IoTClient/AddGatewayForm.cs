using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Forms;
using System.Xml.Linq;

namespace IoTClient
{
    public partial class AddGatewayForm : Form
    {
        public GatewayItem? AddedGateway { get; private set; }

        public AddGatewayForm()
        {
            InitializeComponent();
        }

        private HttpClient Client()
        {
            var c = new HttpClient();
            c.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.Token);
            return c;
        }

        private async void btnOk_Click(object sender, EventArgs e)
        {
            string id = txtId.Text.Trim();
            string name = txtName.Text.Trim();

            if (id == "")
            {
                MessageBox.Show("Enter gateway ID.");
                return;
            }

            try
            {
                using var client = Client();

                var payload = new { gateway_id = id, name = name };

                var res = await client.PostAsJsonAsync("http://3.70.126.6:1880/user/add_gateway", payload);

                if (res.IsSuccessStatusCode)
                {
                    AddedGateway = new GatewayItem
                    {
                        gateway_id = id,
                        name = name
                    };

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Add gateway failed.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gateway add error: " + ex.Message);
            }
        }
    }
}

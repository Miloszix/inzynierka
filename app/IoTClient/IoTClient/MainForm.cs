using ScottPlot;
using ScottPlot.WinForms;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Globalization;

namespace IoTClient
{
    public partial class MainForm : Form
    {
        private System.Windows.Forms.Timer autoRefreshTimer;

        // private string currentGateway = null;

        public MainForm()
        {
            InitializeComponent();

            comboGateway.SelectedIndexChanged += comboGateway_SelectedIndexChanged;

            _ = LoadGateways(); // async start

            autoRefreshTimer = new System.Windows.Forms.Timer();
            autoRefreshTimer.Interval = 5000; // 5000 ms = 5 sekund
            autoRefreshTimer.Tick += AutoRefreshTimer_Tick;
            autoRefreshTimer.Start();
        }

        // =====================================================================
        // HTTP AUTH CLIENT
        // =====================================================================
        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Session.Token);
            return client;
        }

        // =====================================================================
        // LOAD USER GATEWAYS
        // =====================================================================
        private async Task LoadGateways()
        {
            try
            {
                using var client = CreateClient();

                var gateways = Session.Gateways; // bo login już je pobrał

                comboGateway.Items.Clear();
                comboGateway.DisplayMember = "name";
                comboGateway.ValueMember = "gateway_id";

                foreach (var g in gateways)
                    comboGateway.Items.Add(g);

                if (gateways.Count > 0)
                {
                    comboGateway.SelectedIndex = 0;

                    Session.GatewayId = gateways[0].gateway_id;

                    LoadSensors();   // ← to było brakujące
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gateway load error: " + ex.Message);
            }
        }

        // =====================================================================
        // GATEWAY CHANGED
        // =====================================================================
        private void comboGateway_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboGateway.SelectedItem is GatewayItem gw)
            {
                Session.GatewayId = gw.gateway_id;

                LoadSensors();  // ← pobierz sensory dla tego gatewaya
            }
        }


        // =====================================================================
        // LOAD SENSORS FOR CURRENT GATEWAY
        // =====================================================================
        private async void LoadSensors()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Session.GatewayId))
                {
                    MessageBox.Show("No gateway selected.");
                    return;
                }

                using var client = CreateClient();

                // TYLKO JEDEN 'url' – poprawione
                string endpoint = $"http://3.70.126.6:1880/sensors?gateway_id={Session.GatewayId}";

                var allSensors = await client.GetFromJsonAsync<List<Sensor>>(endpoint);

                if (allSensors == null)
                    allSensors = new List<Sensor>();

                // ACCEPTED
                var sensors = allSensors
                    .Where(s => s.status == "accepted")
                    .ToList();

                // PENDING
                var pending = allSensors
                    .Where(s => s.status == "pending")
                    .ToList();

                dataGridPending.DataSource = pending;

                // budowanie tabeli z wartościami "latest"
                var table = new List<SensorLatest>();

                foreach (var s in sensors)
                {
                    string endpointMeas =
                            $"http://3.70.126.6:1880/measurements?sensor_mac={Uri.EscapeDataString(s.sensor_mac)}&gateway_id={Session.GatewayId}";


                    var meas = await client.GetFromJsonAsync<List<Measurement>>(endpointMeas);

                    var latest = meas?
                        .Where(m => !string.IsNullOrWhiteSpace(m.timestamp))
                        .OrderByDescending(m => m.timestamp)
                        .FirstOrDefault();

                    table.Add(new SensorLatest
                    {
                        name = s.name,
                        sensor_mac = s.sensor_mac,
                        temperature = latest?.temperature ?? 0,
                        humidity = latest?.humidity ?? 0,
                        pressure = latest?.pressure ?? 0,
                        timestamp = latest?.timestamp != null
                            ? DateTime.Parse(latest.timestamp).ToString("yyyy-MM-dd HH:mm:ss")
                            : "-"
                    });
                }

                dataGridSensors.DataSource = table;

                // AutoSize
                dataGridSensors.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                foreach (DataGridViewColumn col in dataGridSensors.Columns)
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sensor load error: {ex.Message}");
            }
        }


        // =====================================================================
        // ACCEPT SENSOR
        // =====================================================================
        private async void BtnAccept_Click(object sender, EventArgs e)
        {
            if (dataGridPending.SelectedRows.Count == 0)
            {
                MessageBox.Show("Wybierz sensor.");
                return;
            }

            var sensor = dataGridPending.SelectedRows[0].DataBoundItem as Sensor;
            if (sensor == null)
            {
                MessageBox.Show("Błąd – brak danych sensora.");
                return;
            }

            try
            {
                using var client = CreateClient();

                var payload = new
                {
                    sensor_mac = sensor.sensor_mac,
                    gateway_id = Session.GatewayId
                };

                var res = await client.PostAsJsonAsync(
                    "http://3.70.126.6:1880/accept_sensor", payload);

                if (res.IsSuccessStatusCode)
                    LoadSensors();
                else
                    MessageBox.Show("Błąd akceptacji sensora.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Accept error: " + ex.Message);
            }
        }

        // =====================================================================
        // IGNORE SENSOR
        // =====================================================================
        private async void BtnIgnore_Click(object sender, EventArgs e)
        {
            if (dataGridPending.SelectedRows.Count == 0)
            {
                MessageBox.Show("Wybierz sensor.");
                return;
            }

            var sensor = dataGridPending.SelectedRows[0].DataBoundItem as Sensor;

            try
            {
                using var client = CreateClient();

                var payload = new
                {
                    sensor_mac = sensor.sensor_mac,
                    gateway_id = Session.GatewayId
                };

                var res = await client.PostAsJsonAsync(
                    "http://3.70.126.6:1880/ignore_sensor", payload);

                if (res.IsSuccessStatusCode)
                    LoadSensors();
                else
                    MessageBox.Show("Błąd ignorowania sensora.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ignore error: " + ex.Message);
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            var sf = new SettingsForm();
            sf.ShowDialog();

            // po powrocie z ustawień trzeba odświeżyć gatewaye i sensory
            _ = LoadGateways();
        }


        // =====================================================================
        // SENSOR SELECTED → DRAW CHARTS
        // =====================================================================
        private void dataGridSensors_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridSensors.SelectedRows.Count == 0)
                return;

            var row = dataGridSensors.SelectedRows[0].DataBoundItem as SensorLatest;
            if (row != null)
                DrawCharts(row.sensor_mac);
        }

        // =====================================================================
        // DRAW CHARTS
        // =====================================================================
        private async void DrawCharts(string mac)
        {
            try
            {
                using var client = CreateClient();

                var m = await client.GetFromJsonAsync<List<Measurement>>(
                    $"http://3.70.126.6:1880/measurements?sensor_mac={Uri.EscapeDataString(mac)}&gateway_id={Session.GatewayId}");


                if (m == null || m.Count == 0)
                {
                    ClearAllPlots();
                    return;
                }

                var ordered = m
                    .Where(x => DateTime.TryParse(x.timestamp, out _))
                    .OrderBy(x => DateTime.Parse(x.timestamp))
                    .ToList();

                DateTime[] dts = ordered
                    .Select(x => DateTime.Parse(x.timestamp))
                    .ToArray();

                double[] temps = ordered.Select(x => x.temperature).ToArray();
                double[] hums = ordered.Select(x => x.humidity).ToArray();
                double[] press = ordered.Select(x => x.pressure).ToArray();

                // --- Temperature ---
                var tplt = formsPlotTemp.Plot;
                tplt.Clear();
                tplt.Add.Scatter(dts, temps).LegendText = "Temperature (°C)";
                tplt.Legend.IsVisible = true;
                tplt.Axes.AutoScale();
                tplt.Axes.DateTimeTicksBottom();

                // --- Humidity ---
                var hplt = formsPlotHum.Plot;
                hplt.Clear();
                hplt.Add.Scatter(dts, hums).LegendText = "Humidity (%)";
                hplt.Legend.IsVisible = true;
                hplt.Axes.AutoScale();
                hplt.Axes.DateTimeTicksBottom();

                // --- Pressure ---
                var pplt = formsPlotPress.Plot;
                pplt.Clear();
                pplt.Add.Scatter(dts, press).LegendText = "Pressure (hPa)";
                pplt.Legend.IsVisible = true;
                pplt.Axes.AutoScale();
                pplt.Axes.DateTimeTicksBottom();

                ApplyDarkStyle(tplt);
                ApplyDarkStyle(hplt);
                ApplyDarkStyle(pplt);

                formsPlotTemp.Refresh();
                formsPlotHum.Refresh();
                formsPlotPress.Refresh();

                lblLastUpload.Text =
                    "Last update: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                PositionLastUpdateLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Plot error: " + ex.Message);
            }
        }

        private void AutoRefreshTimer_Tick(object? sender, EventArgs e)
        {
            // Brak wybranego gateway — nic nie odświeżamy
            if (string.IsNullOrWhiteSpace(Session.GatewayId))
                return;

            // Pobierz sensory
            LoadSensors();

            // Jeśli jakiś sensor jest zaznaczony → odśwież wykres
            if (dataGridSensors.SelectedRows.Count > 0)
            {
                var row = dataGridSensors.SelectedRows[0].DataBoundItem as SensorLatest;
                if (row != null)
                {
                    DrawCharts(row.sensor_mac);
                }
            }
        }


        private void ClearAllPlots()
        {
            formsPlotTemp.Plot.Clear();
            formsPlotHum.Plot.Clear();
            formsPlotPress.Plot.Clear();

            formsPlotTemp.Refresh();
            formsPlotHum.Refresh();
            formsPlotPress.Refresh();
        }

        private void PositionLastUpdateLabel()
        {
            lblLastUpload.AutoSize = true;
            lblLastUpload.Refresh();

            int rightPadding = 12;

            lblLastUpload.Left =
                panelTop.ClientSize.Width - lblLastUpload.Width - rightPadding;

            lblLastUpload.Top =
                Math.Max((panelTop.ClientSize.Height - lblLastUpload.Height) / 2, 2);
        }

        private void ApplyDarkStyle(ScottPlot.Plot plt)
        {
            plt.FigureBackground.Color = ScottPlot.Colors.Black;
            plt.DataBackground.Color = ScottPlot.Color.FromHex("#1E1E1E");

            plt.Grid.MajorLineColor = ScottPlot.Color.FromHex("#333333");
            plt.Grid.MinorLineColor = ScottPlot.Color.FromARGB(0);

            plt.Axes.Color(ScottPlot.Colors.White);

            plt.Legend.BackgroundColor = ScottPlot.Color.FromHex("#222222");
            plt.Legend.FontColor = ScottPlot.Colors.White;
        }
    }

    // =====================================================================
    // MODELS
    // =====================================================================

    public class Sensor
    {
        public int id { get; set; }
        public string? gateway_id { get; set; }
        public string? sensor_mac { get; set; }
        public string? name { get; set; }
        public string? status { get; set; }
        public string? timestamp { get; set; }
    }

    public class Measurement
    {
        public double temperature { get; set; }
        public double humidity { get; set; }
        public double pressure { get; set; }
        public string? timestamp { get; set; }
    }

    public class SensorLatest
    {
        public string? name { get; set; }
        public string? sensor_mac { get; set; }
        public double temperature { get; set; }
        public double humidity { get; set; }
        public double pressure { get; set; }
        public string? timestamp { get; set; }
    }

    public class GatewayItem
    {
        public string gateway_id { get; set; }
        public string name { get; set; }
        public string added_at { get; set; }
    }
}

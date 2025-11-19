using ScottPlot;
using ScottPlot.WinForms;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IoTClient
{
    public partial class MainForm : Form
    {
        private readonly string gatewayId;

        public MainForm(string gatewayId)
        {
            InitializeComponent();
            this.gatewayId = gatewayId;

            lblGateway.Text = $"Gateway: {gatewayId}";

            LoadSensors();
            LoadPendingSensors();
        }

        // ================================================================
        // HTTP CLIENT WITH TOKEN
        // ================================================================
        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Session.Token);
            return client;
        }

        // ================================================================
        // LOAD SENSORS + THEIR LATEST MEASUREMENT
        // ================================================================
        private async void LoadSensors()
        {
            try
            {
                using var client = CreateClient();

                var sensors = await client.GetFromJsonAsync<List<Sensor>>(
                    "http://3.70.126.6:1880/sensors");

                var table = new List<SensorLatest>();

                foreach (var s in sensors)
                {
                    string url = $"http://3.70.126.6:1880/measurements?sensor_mac={Uri.EscapeDataString(s.sensor_mac)}";
                    var meas = await client.GetFromJsonAsync<List<Measurement>>(url);

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
                        timestamp = latest?.timestamp ?? "-"
                    });
                }

                dataGridSensors.DataSource = table;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sensor load error: {ex.Message}");
            }
        }

        // ================================================================
        // LOAD PENDING SENSORS
        // ================================================================
        private async void LoadPendingSensors()
        {
            try
            {
                using var client = CreateClient();

                var pending = await client.GetFromJsonAsync<List<Sensor>>(
                    "http://3.70.126.6:1880/pending");

                dataGridPending.DataSource = pending;
            }
            catch { }
        }

        // ================================================================
        // SELECTION → DRAW CHARTS
        // ================================================================
        private void dataGridSensors_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridSensors.SelectedRows.Count == 0)
                return;

            var row = dataGridSensors.SelectedRows[0].DataBoundItem as SensorLatest;
            if (row != null)
            {
                DrawCharts(row.sensor_mac);
            }
        }

        // ================================================================
        // DRAW 3 CHARTS (SCOTTPLOT 5 — SIMPLE API)
        // ================================================================
        private async void DrawCharts(string mac)
        {
            try
            {
                using var client = CreateClient();

                string url = $"http://3.70.126.6:1880/measurements?sensor_mac={Uri.EscapeDataString(mac)}";
                var m = await client.GetFromJsonAsync<List<Measurement>>(url);

                if (m == null || m.Count == 0)
                {
                    ClearAllPlots();
                    return;
                }

                // Parse & convert timestamps
                var ordered = m
                    .Where(x => DateTime.TryParse(x.timestamp, out _))
                    .OrderBy(x => DateTime.Parse(x.timestamp))
                    .ToList();

                double[] xs = ordered
                    .Select(x => DateTime.Parse(x.timestamp).ToOADate())
                    .ToArray();

                double[] temps = ordered.Select(x => x.temperature).ToArray();
                double[] hums = ordered.Select(x => x.humidity).ToArray();
                double[] press = ordered.Select(x => x.pressure).ToArray();

                // ------------------ TEMPERATURE ------------------
                var tplt = formsPlotTemp.Plot;
                tplt.Clear();
                var tline = tplt.Add.Scatter(xs, temps);
                tline.LegendText = "Temperature (°C)";
                tplt.Legend.IsVisible = true;

                // ------------------ HUMIDITY ---------------------
                var hplt = formsPlotHum.Plot;
                hplt.Clear();
                var hline = hplt.Add.Scatter(xs, hums);
                hline.LegendText = "Humidity (%)";
                hplt.Legend.IsVisible = true;

                // ------------------ PRESSURE ---------------------
                var pplt = formsPlotPress.Plot;
                pplt.Clear();
                var pline = pplt.Add.Scatter(xs, press);
                pline.LegendText = "Pressure (hPa)";
                pplt.Legend.IsVisible = true;

                // Refresh All
                formsPlotTemp.Refresh();
                formsPlotHum.Refresh();
                formsPlotPress.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Plot error: " + ex.Message);
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
    }

    // =====================================================================
    // CLEAN MODELS (NO DUPLICATES, ALL NULLABLE)
    // =====================================================================
    public class Sensor
    {
        public string? gateway_id { get; set; }
        public string? sensor_mac { get; set; }
        public string? name { get; set; }
        public string? status { get; set; }
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
}

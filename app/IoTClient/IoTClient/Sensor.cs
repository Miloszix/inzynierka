using System;
using System.Collections.Generic;
using System.Text;

namespace IoTClient
{
    public class Sensor
    {
        public int id { get; set; }
        public string gateway_id { get; set; }
        public string sensor_mac { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string added_at { get; set; }

        // helper for latest measurement (filled in client)
        public double? last_temperature { get; set; }
        public double? last_humidity { get; set; }
        public double? last_pressure { get; set; }
        public string last_timestamp { get; set; }
    }
}

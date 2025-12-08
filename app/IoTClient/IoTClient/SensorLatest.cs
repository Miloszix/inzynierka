using System;
using System.Collections.Generic;
using System.Text;

namespace IoTClient
{
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

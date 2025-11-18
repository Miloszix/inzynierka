using System;
using System.Collections.Generic;
using System.Text;

namespace IoTClient
{
    public class Measurement
    {
        public int id { get; set; }
        public string topic { get; set; }
        public double temperature { get; set; }
        public double humidity { get; set; }
        public double pressure { get; set; }
        public DateTime timestamp { get; set; }
    }
}

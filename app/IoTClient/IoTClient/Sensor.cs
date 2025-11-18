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
        public bool accepted { get; set; }
    }
}

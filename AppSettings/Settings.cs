using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prema.PriceHarbor.Scraper.AppSettings
{
    public class Settings
    {
        public List<string> ProxyServers { get; set; }

        public RabbitMqOptions RabbitOptions { get; set; }

    }

    public class RabbitMqOptions
    {
        public bool Enabled { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string HostName { get; set; }
        public ushort Port { get; set; }
        public string VHost { get; set; }
        public ushort PrefetchCount { get; set; }
    }
}

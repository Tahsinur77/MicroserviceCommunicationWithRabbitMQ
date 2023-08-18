using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share
{
    public class RabbitMqConfiguration
    {
        public string Url { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public int RetryCount { get; set; }
        public int RetryIntervalSecond { get; set; }
    }
}

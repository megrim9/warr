using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class Email
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Sender { get; set; }
        public string Target { get; set; }
        public DateTime Date { get; set; }

    }
}

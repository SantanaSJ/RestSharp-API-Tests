using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSharpAPITests
{
    public class Label
    {
        public long id { get; set; }
        public string name { get; set; }

        public string color { get; set; }

        public string description { get; set; }
    }
}

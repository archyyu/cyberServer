using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CashierLibrary.Model
{
    public class WebPage
    {

        public string name { get; set; }

        public string content { get; set; }

        public string contentType { get; set; }

        public UInt32 length { get; set; }

        public string acceptRanges { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchProcessingRevitFiles
{
    public class RevitError
    {
        public static RevitError Instance => new RevitError() { Error = "Error aaaaaaaa", ErrorTime = new DateTime(2021, 04, 16, 15, 23, 33) };
        public string Error { get; set; }
        public DateTime ErrorTime { get; set; }
    }
}

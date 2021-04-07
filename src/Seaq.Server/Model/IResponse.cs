using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Seaq.Server.Model
{
    public interface IResponse
    {
        bool IsSuccessful { get; set; }
        bool IsValid { get; set; }
        IEnumerable<string> Messages { get; set; }
        IEnumerable<string> Errors { get; set; }
        ITelemetry Telemetry { get; set; }
    }
}

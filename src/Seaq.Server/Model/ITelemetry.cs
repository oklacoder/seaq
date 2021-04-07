using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Seaq.Server.Model
{
    public interface ITelemetry
    {
        public string Scheme { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public string QueryString { get; set; }
        string Body { get; set; }
        long StartUtcTicks { get; set; }
        long FinishUtcTicks { get; set; }
        long? Duration { get; }
        double? DurationMs { get; }

        void Complete();
    }
}

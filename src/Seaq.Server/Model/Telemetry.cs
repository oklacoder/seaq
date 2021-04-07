using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Seaq.Server.Model
{
    public class Telemetry :
        ITelemetry
    {
        public string Scheme { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public string QueryString { get; set; }

        public string Body { get; set; }

        public long StartUtcTicks { get; set; }

        public long FinishUtcTicks { get; set; }

        public long? Duration { 
            get {
                if (FinishUtcTicks == default || StartUtcTicks == default)
                    return null;

                return FinishUtcTicks - StartUtcTicks;
            } 
        }

        public double? DurationMs
        {
            get
            {
                if (!Duration.HasValue)
                    return null;
                return (double)Duration / 10000;
            }
        }

        public void Complete()
        {
            FinishUtcTicks = DateTime.UtcNow.Ticks;
        }

        public Telemetry()
        {

        }
        public Telemetry(
            string scheme,
            string host,
            string path,
            string queryString,
            string body,
            long startUtcTicks)
        {
            Scheme = scheme;
            Host = host;
            Path = path;
            QueryString = queryString;
            Body = body;
            StartUtcTicks = startUtcTicks;
        }
        public static async Task<Telemetry> Create(
            HttpRequest request)
        {
            string body = string.Empty;

            using (var s = request.Body)
            using (var sr = new StreamReader(s))
                body = await sr.ReadToEndAsync();

            return new Telemetry(
                request.Scheme,
                request.Host.Value,
                request.Path,
                request.QueryString.Value,
                body,
                DateTime.UtcNow.Ticks
                );
        }
    }
}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Seaq.Server.Model
{
    public class BaseResponse :
        IResponse
    {
        public bool IsSuccessful { get; set; } = true;

        public bool IsValid { get; set; } = true;

        public IEnumerable<string> Messages { get; set; }

        public IEnumerable<string> Errors { get; set; }

        public ITelemetry Telemetry { get; set; }

    }

    public static class ResponseExtensions
    {
        public static SingleResponse<T> Complete<T>(this SingleResponse<T> response, T value)
        {
            response.Value = value;
            response.Telemetry.FinishUtcTicks = DateTime.UtcNow.Ticks;
            return response;
        }

        public static ListResponse<T> Complete<T>(this ListResponse<T> response, IEnumerable<T> values)
        {
            response.Values = values;
            response.Telemetry.FinishUtcTicks = DateTime.UtcNow.Ticks;
            return response;
        }
    }
}

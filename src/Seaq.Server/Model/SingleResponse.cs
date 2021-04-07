using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Seaq.Server.Model
{
    public class SingleResponse<T> : 
        BaseResponse
    {
        public T Value { get; set; }

        public static async Task<SingleResponse<T>> Start(HttpRequest request)
        {
            var r = new SingleResponse<T>();
            r.Telemetry = await Model.Telemetry.Create(request);
            return r;
        }

        public SingleResponse()
        {

        }
        public SingleResponse(
            T value)
        {
            Value = value;
        }
    }
}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Seaq.Server.Model
{
    public class ListResponse<T> :
        BaseResponse
    {
        public IEnumerable<T> Values { get; set; }

        public ListResponse()
        {

        }

        public ListResponse(
            IEnumerable<T> values)
        {
            Values = values;
        }

        public static async Task<ListResponse<T>> Start(HttpRequest request)
        {
            var r = new ListResponse<T>();
            r.Telemetry = await Model.Telemetry.Create(request);
            return r;
        }
    }
}

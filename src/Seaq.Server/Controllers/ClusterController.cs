using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seaq.Clusters;
using Seaq.Server.Model;

namespace Seaq.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ClusterController : ControllerBase
    {
        private readonly Cluster _cluster;

        public ClusterController(
            Cluster cluster)
        {
            this._cluster = cluster;
        }


        [HttpGet]
        public async Task<SingleResponse<bool>> Ping()
        {
            return (await SingleResponse<bool>
                .Start(this.Request))
                ?.Complete(_cluster.CanPing());
        }

        [HttpGet]
        public async Task<ListResponse<string>> GetCollections()
        {
            return (await ListResponse<string>
                .Start(this.Request))
                ?.Complete(_cluster.Collections.Select(x => x.CollectionName));
        }
    }
}
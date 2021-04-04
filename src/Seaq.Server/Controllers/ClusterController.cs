using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seaq.Clusters;

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
        public bool CanPingCluster()
        {
            return _cluster.CanPing();
        }
    }
}
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
    public class CollectionController : ControllerBase
    {

        private readonly Cluster _cluster;

        public CollectionController(
            Cluster cluster)
        {
            _cluster = cluster;
        }


        [HttpGet("{collectionName}")]
        public async Task<SingleResponse<Collection>> GetCollection(
            [FromRoute] string collectionName)
        {
            return (await SingleResponse<Collection>.Start(this.Request))
                .Complete(_cluster.Collections.FirstOrDefault(x => x.CollectionName.Equals(collectionName, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
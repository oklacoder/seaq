
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace seaq.Server.Controllers
{
    [Route("api/[controller]/{action}")]
    [ApiController]
    public class IndexController : BaseController
    {
        public IndexController(
            Cluster cluster)
            : base(cluster)
        {

        }

        [HttpPost]
        public async Task<SimpleQueryResults> QueryIndices(
            [FromBody] SimpleQuery query)
        {
            return await _cluster.QueryAsync<SimpleQueryResults>(query);            
        }

        [HttpGet("{name}")]
        public async Task<Index> Details(
            [FromRoute] string name)
        {
            return _cluster.Indices.FirstOrDefault(x => x.Name == name);
        }
    }
}

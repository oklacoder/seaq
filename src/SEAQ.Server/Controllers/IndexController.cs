
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
        public async Task<SimpleQueryResults<Index>> QueryIndices(
            [FromBody] SimpleQueryCriteria criteria)
        {
            return Query<Index, SimpleQueryResults<Index>>(criteria);

            //var query = new SimpleQuery<Index>(criteria.GetAsTyped<Index>());
            //var resp = _cluster.Query(query);
            //return resp as SimpleQueryResults<Index>;
        }

        [HttpGet("{name}")]
        public async Task<Index> Details(
            [FromRoute] string name)
        {
            return _cluster.Indices.FirstOrDefault(x => x.Name == name);
        }

        private TResp Query<T, TResp>(SimpleQueryCriteria criteria)
            where T : BaseDocument
            where TResp : class, ISeaqQueryResults<T>
        {
            return _cluster.Query(new SimpleQuery<T>(criteria.GetAsTyped<T>())) as TResp;
        }
    }
}

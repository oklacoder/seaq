using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace seaq.Server.Controllers
{
    public class BaseController : ControllerBase
    {
        protected readonly Cluster _cluster;

        public BaseController(
            Cluster cluster)
        {
            _cluster = cluster;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace STIKS.Server
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        [HttpPost("Authorization")]
        public async Task<ApiResult> Authorization(string login, string hash, int time)
        {
            return await UserEngines.Instance.Authorization(login, hash, time);
        }

        [HttpGet]
        public string Get()
        {
            return "STIKS API";
        }
    }
}
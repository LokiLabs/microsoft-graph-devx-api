﻿using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using GraphExplorerAppModeService.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Web;
using System.Net.Http;
using System.Net.Http.Headers;

namespace GraphWebApi.Controllers
{
    [ApiController]
    public class GraphExplorerAppModeController : ControllerBase
    {
        private readonly ITokenAcquisition tokenAcquisition;
        public GraphExplorerAppModeController(ITokenAcquisition tokenAcquisition)
        {
            this.tokenAcquisition = tokenAcquisition;
        }
        [Route("api/[controller]/{*all}")]
        [Route("graphproxy/{*all}")]
        [HttpGet]
        [AuthorizeForScopes(Scopes = new[] { "https://graph.microsoft.com/.default" })]
        public async Task<string> GetAsync(string all)
        {
            // Acquire the access token.
            string scopes = "https://graph.microsoft.com/.default";
            return await tokenAcquisition.GetAccessTokenForAppAsync(scopes);
            //return async Task<string> ITokenAcquisition.GetAccessTokenForAppAsync(string scope, string ? tenant = null);
            //return await ProcessRequestAsync("GET", all, null).ConfigureAwait(false);
        }

        [Route("api/[controller]/{*all}")]
        [Route("graphproxy/{*all}")]
        [HttpPost]
        public async Task<IActionResult> PostAsync(string all, [FromBody] object body)
        {
            return await ProcessRequestAsync("POST", all, body).ConfigureAwait(false);
        }

        [Route("api/[controller]/{*all}")]
        [Route("graphproxy/{*all}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(string all)
        {
            return await ProcessRequestAsync("DELETE", all, null).ConfigureAwait(false);
        }

        [Route("api/[controller]/{*all}")]
        [Route("graphproxy/{*all}")]
        [HttpPut]
        public async Task<IActionResult> PutAsync(string all, [FromBody] object body)
        {
            return await ProcessRequestAsync("PUT", all, body).ConfigureAwait(false);
        }

        [Route("api/[controller]/{*all}")]
        [Route("graphproxy/{*all}")]
        [HttpPatch]
        public async Task<IActionResult> PatchAsync(string all, [FromBody] object body)
        {
            return await ProcessRequestAsync("PATCH", all, body).ConfigureAwait(false);
        }

        private async Task<IActionResult> ProcessRequestAsync(string method, string all, object content)
        {
            return Ok();
        }
    }
}

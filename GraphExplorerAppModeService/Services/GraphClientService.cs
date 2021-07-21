using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphExplorerAppModeService.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GraphExplorerAppModeService.Services
{
    class GraphClientService : IGraphServiceClient
    {
        public GraphClientService(IConfiguration configuration)
        {

        }
    }
}

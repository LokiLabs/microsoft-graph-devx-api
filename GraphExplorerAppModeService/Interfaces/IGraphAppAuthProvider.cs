using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphExplorerAppModeService.Interfaces
{
    public interface IGraphAppAuthProvider
    {
        GraphServiceClient GetAuthenticatedGraphClient(string accessToken);
    }
}

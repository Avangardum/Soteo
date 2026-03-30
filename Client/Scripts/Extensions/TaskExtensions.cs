using System.Threading.Tasks;
using Soteo.Client.Nodes.Autoloads;

namespace Soteo.Client.Extensions;

public static class TaskExtensions
{
    extension (Task self)
    {
        public async void CollectException()
        {
            try
            {
                await self;
            }
            catch (Exception e)
            {
                AsyncExceptionCollector.Collect(e);
            }
        }
    }
}
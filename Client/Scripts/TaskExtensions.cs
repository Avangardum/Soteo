using System.Threading.Tasks;
using Godot;

namespace Soteo.Client;

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
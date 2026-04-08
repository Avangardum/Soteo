using Soteo.Shared.Nodes.Autoloads;

namespace Soteo.Shared.Extensions;

public static class TaskExtensions
{
    extension (Task self)
    {
        /// <summary>
        /// Executes the action after the task is completed. Unlike ContinueWith, respects SynchronizationContext.
        /// </summary>
        public async Task ContinueWithinContext(Action<Task> action)
        {
            await self;
            action(self);
        }
        
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
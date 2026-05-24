namespace Soteo.Util.Extensions;

public static class TaskExtensions
{
    extension (Task self)
    {
        /// <summary>
        /// Executes the action after the task is completed. Unlike ContinueWith, respects SynchronizationContext.
        /// If the task throws, the failed task is still passed to the action. If the action ignores the task,
        /// the other overload that takes Action without arguments and throws when the task throws should be used to
        /// avoid losing exceptions.
        /// </summary>
        public async Task ContinueWithinContext(Action<Task> action)
        {
            try
            {
                await self;
            }
            finally
            {
                action(self);
            }
        }
        
        /// <summary>
        /// Executes the action after the task is completed. Unlike ContinueWith, respects SynchronizationContext.
        /// If the task throws, this method throws instead of calling the action.
        /// </summary>
        public async Task ContinueWithinContext(Action action)
        {
            await self;
            action();
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
namespace Soteo.Shared.Extensions;

public static class TaskExtensions
{
    extension (Task self)
    {
        /// <summary>
        /// Executes the action after the task is completed. Unlike ContinueWith, respects SynchronizationContext.
        /// </summary>
        public async void ContinueWithinContext(Action<Task> action)
        {
            await self;
            action(self);
        }
    }
}
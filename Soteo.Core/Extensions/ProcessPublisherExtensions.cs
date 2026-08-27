using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Extensions;

public static class ProcessPublisherExtensions
{
    extension (IProcessPublisher self)
    {
        public IDisposable SubscribeToProcess(Action handler, ProcessPriorityEnum priority, bool callWhenPaused)
        {
            return self.SubscribeToProcess(_ => handler(), priority, callWhenPaused);
        }
        
        public IDisposable SubscribeToPhysicsProcess(Action handler, ProcessPriorityEnum priority, bool callWhenPaused)
        {
            return self.SubscribeToPhysicsProcess(_ => handler(), priority, callWhenPaused);
        }
    }
}

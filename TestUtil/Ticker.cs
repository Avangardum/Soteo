using Soteo.Core.Shared;
using Soteo.Util;

namespace TestUtil;

public static class Ticker
{
    public static Builder1 Tick(Action<double> tick) => new(tick);

    public class Builder1(Action<double> tick)
    {
        public Builder2 WithInterval(double interval) => new(tick, interval);
        
        public Builder2 WithDefaultInterval() => WithInterval(Const.TickInterval);
    }
    
    public class Builder2(Action<double> tick, double interval)
    {
        public void ForAtLeast(double time) => For(time, Maths.CeilToInt);
        
        public void ForAtMost(double time) => For(time, Maths.FloorToInt);
        
        private void For(double time, Func<double, int> round)
        {
            for (int ticksRemaining = round(time / interval); ticksRemaining > 0; ticksRemaining--)
                tick(interval);
        }
    }
}
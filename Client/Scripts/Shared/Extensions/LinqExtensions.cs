namespace Soteo.Shared.Extensions;

public static class LinqExtensions
{
    extension<T> (IEnumerable<T?> self)
    {
        public IEnumerable<T> WhereNotNull() => self.Where(it => it != null)!;
    }
    
    extension<T> (IEnumerable<T> self)
    {
        public IOrderedEnumerable<T> Order() => self.OrderBy(it => it);
        
        public IOrderedEnumerable<T> OrderDescending() => self.OrderByDescending(it => it);
        
        public T FirstOrDefault(T defaultValue) => self.Any() ? self.First() : defaultValue;
        
        public float Product(Func<T, float> selector) => self.Select(selector).Product();
        
        public double Product(Func<T, double> selector) => self.Select(selector).Product();
    }
    
    extension (IEnumerable<float> self)
    {
        public float Product() => self.Aggregate(1f, (a, b) => a * b);
    }
    
    extension (IEnumerable<double> self)
    {
        public double Product() => self.Aggregate(1.0, (a, b) => a * b);
    }
}
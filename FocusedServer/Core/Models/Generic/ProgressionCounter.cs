namespace Core.Models.Generic
{
    public class ProgressionCounter<T>
    {
        public T Current { get; set; }
        public T Target { get; set; }
    }
}

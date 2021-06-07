namespace Core.Dtos
{
    public class ActivityBreakdownDto
    {
        public double Regular { get; set; }
        public double Recurring { get; set; }
        public double Interruption { get; set; }
        public double Overlearning { get; set; }
    }
}

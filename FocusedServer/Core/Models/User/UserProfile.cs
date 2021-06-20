namespace Core.Models.User
{
    public class UserProfile : DatabaseEntry
    {
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public string FocusSessionId { get; set; }
        public string BreakSessionId { get; set; }
        public PerformanceRating Ratings { get; set; }
    }
}

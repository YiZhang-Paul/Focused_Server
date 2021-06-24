namespace Core.Models.User
{
    public class UserProfile : DatabaseEntry
    {
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public PerformanceRating Ratings { get; set; }
    }
}

using Core.Models.Generic;

namespace Core.Models.User
{
    public class UserProfile : DatabaseEntry
    {
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public PerformanceRating Ratings { get; set; }
        public TimeInfo TimeInfo { get; set; } = new TimeInfo();
    }
}

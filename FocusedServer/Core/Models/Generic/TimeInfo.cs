using System;

namespace Core.Models.Generic
{
    public class TimeInfo
    {
        public DateTime Creation { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }
}

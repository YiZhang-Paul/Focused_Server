using System;

namespace Core.Models.Generic
{
    public class TimeInfo
    {
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}

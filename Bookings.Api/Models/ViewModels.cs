using System;
using System.Collections.Generic;

namespace Bookings.Api.Models
{
    public static class StatusModel
    {
        public static long RefToId(this string str)
        {
            return long.Parse(str.Replace("-", ""));
        }
    }

    public class Availability
    {
        public long InstanceId { get; set; }
        public long ResourceId { get; set; }
        public DateTime Start { get; set; }
        public int AvailableCapacity { get; set; }
    }

    public class InstanceModel
    {
        public int Capacity { get; set; }
        public long ResourceId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class HoldModel
    {
        public HoldModel()
        {
            Qty = 1;
        }

        public long InstanceId { get; set; }
        public int Qty { get; set; }
    }
}

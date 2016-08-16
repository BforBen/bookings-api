using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Bookings.Models
{
    /// <summary>
    /// Booking status
    /// </summary>
    public enum BookingStatus
    {
        /// <summary>
        /// The booking is on hold
        /// </summary>
        Held,
        /// <summary>
        /// The booking is confirmed
        /// </summary>
        Confirmed,
        /// <summary>
        /// The booking was on hold but was released before it was confirmed
        /// </summary>
        Expired,
        /// <summary>
        /// The booking was cancelled
        /// </summary>
        Cancelled
    }

    public class Resource
    {
        public Resource()
        {
            Id = Flake.Id.Next<long>();
        }

        /// <summary>
        /// The resource ID
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; private set; }
        /// <summary>
        /// The name of the resource
        /// </summary>
        public string Name { get; set; }

        [JsonIgnore]
        public virtual ICollection<Instance> Instances { get; set; }
    }

    public class Instance
    {
        public Instance()
        {
            Id = Flake.Id.Next<long>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; private set; }
        public long ResourceId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int Capacity { get; set; }

        [JsonIgnore]
        public virtual bool Spaces
        {
            get
            {
                var Taken = 0;
                if (Bookings != null)
                {
                    Taken = Bookings.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Held).Count();
                }

                return Capacity > Taken;
            }
        }

        public virtual int AvailableSpaces
        {
            get
            {
                var Taken = 0;
                if (Bookings != null)
                {
                    Taken = Bookings.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Held).Count();
                }

                return Capacity - Taken;
            }
        }

        [JsonIgnore]
        public virtual Resource Resource { get; set; }
        [JsonIgnore]
        public virtual ICollection<Booking> Bookings { get; set; }
    }

    public class Booking
    {
        public Booking()
        {
            Id = Flake.Id.Next<long>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; private set; }
        public long InstanceId { get; set; }
        public BookingStatus Status { get; set; }
    }

    public class BookingsData : DbContext
    {
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Instance> Instances { get; set; }
        public DbSet<Resource> Resources { get; set; }

        public BookingsData()
            : base("BookingsData")
        {
        }

        public static BookingsData Create()
        {
            return new BookingsData();
        }
    }
}

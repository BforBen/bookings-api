using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Bookings.Api.Models;
using System.Data.Entity.SqlServer;
using Bookings.Models;

namespace Bookings.Api.Controllers.v1
{
    /// <summary>
    /// Bookings controller
    /// </summary>
    [RoutePrefix("bookings/v1")]
    public class BookingsController : ApiController
    {
        private BookingsData db = new BookingsData();

        /// <summary>
        /// Availability
        /// </summary>
        /// <param name="resourceId">Resource ID</param>
        /// <param name="date">Date to show availability from</param>
        /// <returns>The next three available slots</returns>
        [HttpGet]
        [Route("availability")]
        [Route("resources/{resourceId:long}/availability")]
        [ResponseType(typeof(IEnumerable<Availability>))]
        public async Task<IHttpActionResult> Availability(CancellationToken cancellationToken, long resourceId = 0, DateTime? date = null)
        {
            if (resourceId == 0)
            {
                return BadRequest("You must specify the resource ID.");
            }

            var Date = date ?? DateTime.Today;

            var Resource = await db.Resources.FindAsync(cancellationToken, resourceId);

            if (Resource == null)
            {
                return NotFound();
            }

            var AvailableInstances = Resource.Instances.OrderBy(i => i.Start).Where(i => i.Start > Date && i.Spaces).Take(3)
                .Select(i => new Availability 
                { 
                    ResourceId = i.ResourceId, 
                    InstanceId = i.Id,
                    Start = i.Start,
                    AvailableCapacity = i.AvailableSpaces
                });

            return Ok(AvailableInstances);
        }

        /// <summary>
        /// Book a slot for the specified instance
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="Hold"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("book")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> Book(CancellationToken cancellationToken, HoldModel Hold)
        {
            if (Hold.Qty < 1)
            {
                return BadRequest("You can't hold less than 1 space!");
            }

            if (Hold.InstanceId == 0)
            {
                return BadRequest("Invalid instance ID specified.");
            }

            var Instance = await db.Instances.FindAsync(cancellationToken, Hold.InstanceId);

            if (Instance == null)
            {
                return BadRequest("Instance not found.");
            }

            if (Instance.AvailableSpaces >= Hold.Qty)
            {
                var Booking = new Booking()
                {
                    InstanceId = Hold.InstanceId,
                    Status = BookingStatus.Confirmed
                };

                db.Bookings.Add(Booking);

                var x = await db.SaveChangesAsync(cancellationToken);

                var BookingId = await Flake.Id.Format(Booking.Id);

                return Ok(new { BookingId = BookingId });
            }

            return NotFound();
        }

        /// <summary>
        /// Update a booking
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="Hold"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("book/{bookingId}")]
        [ResponseType(typeof(Booking))]
        public async Task<IHttpActionResult> UpdateBooking(CancellationToken cancellationToken, string bookingId, HoldModel Hold)
        {
            var Booking = await db.Bookings.FindAsync(cancellationToken, bookingId.RefToId());

            if (Booking == null)
            {
                return NotFound();
            }

            if (Hold.Qty < 1)
            {
                return BadRequest("You can't book less than 1 space!");
            }

            if (Hold.InstanceId == 0)
            {
                return BadRequest("Invalid instance ID specified.");
            }

            var Instance = await db.Instances.FindAsync(cancellationToken, Hold.InstanceId);

            if (Instance == null)
            {
                return BadRequest("Instance not found.");
            }

            if (Instance.AvailableSpaces >= Hold.Qty)
            {
                Booking.InstanceId = Hold.InstanceId;

                var x = await db.SaveChangesAsync(cancellationToken);

                var BookingId = await Flake.Id.Format(Booking.Id);

                return Ok(Booking);
            }

            return NotFound();
        }

        /// <summary>
        /// Hold a slot for the specified instance
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="Hold"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("hold")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> Hold(CancellationToken cancellationToken, HoldModel Hold)
        {
            if (Hold.Qty < 1)
            {
                return BadRequest("You can't hold less than 1 space!");
            }

            if (Hold.InstanceId == 0)
            {
                return BadRequest("Invalid instance ID specified.");
            }

            var Instance = await db.Instances.FindAsync(cancellationToken, Hold.InstanceId);

            if (Instance == null)
            {
                return BadRequest("Instance not found.");
            }

            if (Instance.AvailableSpaces >= Hold.Qty)
            {
                var Booking = new Booking()
                    {
                        InstanceId = Hold.InstanceId,
                        Status = BookingStatus.Held
                    };

                db.Bookings.Add(Booking);

                var x = await db.SaveChangesAsync(cancellationToken);

                var BookingId = await Flake.Id.Format(Booking.Id);

                return Ok(new { BookingId = BookingId });
            }

            return NotFound();
        }

        [HttpPost]
        [Route("cancel/{bookingId}")]
        [ResponseType(typeof(Booking))]
        public async Task<IHttpActionResult> Cancel(CancellationToken cancellationToken, string bookingId)
        {
            var Booking = await db.Bookings.FindAsync(cancellationToken, bookingId.RefToId());

            if (Booking == null)
            {
                return NotFound();
            }

            Booking.Status = BookingStatus.Cancelled;

            var x = await db.SaveChangesAsync();

            return Ok(Booking);
        }
        
        [HttpGet]
        [Route("status/{bookingId}")]
        [ResponseType(typeof(BookingStatus))]
        public async Task<IHttpActionResult> Status(CancellationToken cancellationToken, string bookingId)
        {
            var Booking = await db.Bookings.FindAsync(cancellationToken, bookingId.RefToId());

            if (Booking == null)
            {
                return NotFound();
            }

            return Ok(new { Status = Booking.Status });
        }

        [HttpGet]
        [Route("resolve/{bookingId}")]
        [ResponseType(typeof(BookingStatus))]
        public async Task<IHttpActionResult> Resolve(CancellationToken cancellationToken, string bookingId)
        {
            var Booking = await db.Bookings.FindAsync(cancellationToken, bookingId.RefToId());

            if (Booking == null)
            {
                return NotFound();
            }
            //return Ok(new { Status = Booking.Status });

            return Ok(new { Status = BookingStatus.Confirmed });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
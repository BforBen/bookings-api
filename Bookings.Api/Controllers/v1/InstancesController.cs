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
using GuildfordBoroughCouncil.Linq;

namespace Bookings.Api.Controllers.v1
{
    [RoutePrefix("bookings/v1/instances")]
    public class InstancesController : ApiController
    {
        private BookingsData db = new BookingsData();

        [HttpGet]
        [Route]
        [Route("~/bookings/v1/resources/{resourceId:long}/instances")]
        [ResponseType(typeof(IEnumerable<Instance>))]
        public async Task<IHttpActionResult> Get(CancellationToken cancellationToken, long? resourceId = null)
        {
            var Instances = await db.Instances
                .OrderBy(i => i.Resource.Name).ThenBy(i => i.Start)
                .WhereIf(resourceId.HasValue, i => i.ResourceId == resourceId.Value)
                .ToListAsync(cancellationToken);

            return Ok(Instances);
        }

        [HttpGet]
        [Route("{id}")]
        [ResponseType(typeof(Instance))]
        public async Task<IHttpActionResult> Get(CancellationToken cancellationToken, long id)
        {
            var instance = await db.Instances.FindAsync(cancellationToken, id);

            if (instance != null)
            {
                return Ok(instance);
            }

            return NotFound();
        }

        [HttpPost]
        [Route]
        [ResponseType(typeof(long))]
        public async Task<IHttpActionResult> New(CancellationToken cancellationToken, [FromBody] InstanceModel i)
        {
            var instance = new Instance();
            instance.Capacity = i.Capacity;
            instance.End = i.End;
            instance.ResourceId = i.ResourceId;
            instance.Start = i.Start;

            if (ModelState.IsValid)
            {
                db.Instances.Add(instance);
                var x = await db.SaveChangesAsync(cancellationToken);

                return Ok(instance.Id);
            }

            return BadRequest(ModelState);
        }

        [HttpPut]
        [Route("{id}")]
        [ResponseType(typeof(Instance))]
        public async Task<IHttpActionResult> Update(CancellationToken cancellationToken, long id, [FromBody] InstanceModel i)
        {
            var instance = await db.Instances.FindAsync(cancellationToken, id);

            if (instance != null)
            {
                instance.Capacity = i.Capacity;
                instance.End = i.End;
                instance.Start = i.Start;

                if (ModelState.IsValid)
                {
                    var x = await db.SaveChangesAsync(cancellationToken);

                    return Ok(instance);
                }

                return BadRequest(ModelState);
            }

            return NotFound();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}

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
    [RoutePrefix("bookings/v1/resources")]
    public class ResourcesController : ApiController
    {
        private BookingsData db = new BookingsData();

        [HttpGet]
        [Route]
        [ResponseType(typeof(IEnumerable<Resource>))]
        public async Task<IHttpActionResult> Get(CancellationToken cancellationToken)
        {
            return Ok(await db.Resources.OrderBy(r => r.Name).ToListAsync(cancellationToken));
        }
        
        [HttpPost]
        [Route]
        [ResponseType(typeof(long))]
        public async Task<IHttpActionResult> New(CancellationToken cancellationToken, [FromBody] string Name)
        {
            var r = new Resource();
            r.Name = Name;

            if (ModelState.IsValid)
            {
                db.Resources.Add(r);
                var x = await db.SaveChangesAsync(cancellationToken);

                return Ok(r.Id);
            }

            return BadRequest(ModelState);
        }

        [HttpGet]
        [Route("{id}")]
        [ResponseType(typeof(Resource))]
        public async Task<IHttpActionResult> Get(CancellationToken cancellationToken, long id)
        {
            var resource = await db.Resources.FindAsync(cancellationToken, id);

            if (resource != null)
            {
                return Ok(resource);
            }

            return NotFound();
        }

        [HttpPut]
        [Route("{id}")]
        [ResponseType(typeof(Resource))]
        public async Task<IHttpActionResult> Update(CancellationToken cancellationToken, long id, [FromBody] string Name)
        {
            var resource = await db.Resources.FindAsync(cancellationToken, id);

            if (resource != null)
            {
                resource.Name = Name;

                if (ModelState.IsValid)
                {
                    var x = await db.SaveChangesAsync(cancellationToken);

                    return Ok(resource);
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

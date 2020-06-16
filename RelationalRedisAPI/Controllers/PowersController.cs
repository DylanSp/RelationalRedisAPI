using Adapters.Interfaces;
using Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalRedisAPI.Controllers
{
    [Route("api/powers")]
    [ApiController]
    public class PowersController : ControllerBase
    {
        private IPowerAdapter PowerAdapter { get; }

        public PowersController(IPowerAdapter powerAdapter)
        {
            this.PowerAdapter = powerAdapter;
        }

        // GET api/powers
        [HttpGet]
        public ActionResult<IEnumerable<Power>> GetAllPowers()
        {
            return PowerAdapter.ReadAll().ToList();
        }

        // GET api/powers/32c64485-d35c-4a01-b412-06a9cb84c19c
        [HttpGet("{id:guid}", Name = nameof(GetPower))] // named so Create() can refer to it with CreatedAtRoute()
        public ActionResult<Power> GetPower([FromRoute]Guid id)
        {
            var maybePower = PowerAdapter.Read(id);
            if (maybePower.HasValue)
            {
                return maybePower.Value;
            }

            return NotFound();
        }

        // POST api/powers
        [HttpPost]
        public ActionResult<Power> CreatePower([FromBody] Power power)
        {
            var powerExists = PowerAdapter.Read(power.Id).HasValue;
            if (powerExists)
            {
                return UnprocessableEntity();
            }

            PowerAdapter.Save(power);

            return CreatedAtRoute(nameof(GetPower), new { power.Id }, power);
        }

        // POST api/powers/32c64485-d35c-4a01-b412-06a9cb84c19c
        [HttpPost("{id:guid}")]
        public ActionResult<Power> UpdatePower([FromRoute] Guid id, [FromBody] Power power)
        {
            var powerExists = PowerAdapter.Read(id).HasValue;
            if (!powerExists)
            {
                return NotFound();
            }

            if (id != power.Id)
            {
                return BadRequest();
            }

            PowerAdapter.Save(power);
            return power;
        }

        // DELETE api/powers/32c64485-d35c-4a01-b412-06a9cb84c19c
        [HttpDelete("{id:guid}")]
        public ActionResult DeletePower([FromRoute] Guid id)
        {
            var powerExists = PowerAdapter.Read(id).HasValue;
            if (!powerExists)
            {
                return NotFound();
            }

            PowerAdapter.Delete(id);
            return NoContent();
        }
    }
}

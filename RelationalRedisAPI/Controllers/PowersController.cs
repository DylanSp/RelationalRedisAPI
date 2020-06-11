using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace RelationalRedisAPI.Controllers
{
    [Route("api/powers")]
    [ApiController]
    public class PowersController : ControllerBase
    {
        private IEntityAdapter<Power> PowerAdapter { get; }

        public PowersController(IEntityAdapter<Power> powerAdapter)
        {
            this.PowerAdapter = powerAdapter;
        }

        // GET api/powers
        [HttpGet]
        public ActionResult<IEnumerable<Power>> Get()
        {
            return PowerAdapter.ReadAll().ToList();
        }

        // GET api/powers/32c64485-d35c-4a01-b412-06a9cb84c19c
        [HttpGet("{id:guid}")]
        public ActionResult<Power> Get([FromRoute]Guid id)
        {
            var maybePower = PowerAdapter.Read(id);
            if (maybePower.HasValue)
            {
                return maybePower.Value;
            }

            return NotFound();
        }

        /*
        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
        */
    }
}

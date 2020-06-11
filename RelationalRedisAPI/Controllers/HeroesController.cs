using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RelationalRedisAPI.Controllers
{
    [Route("api/heroes")]
    [ApiController]
    public class HeroesController : ControllerBase
    {
        private IEntityAdapter<Hero> HeroAdapter { get; }

        public HeroesController(IEntityAdapter<Hero> heroAdapter)
        {
            this.HeroAdapter = heroAdapter;
        }

        // GET api/heroes
        [HttpGet]
        public ActionResult<IEnumerable<Hero>> Get()
        {
            return HeroAdapter.ReadAll().ToList();
        }

        // GET api/heroes/32c64485-d35c-4a01-b412-06a9cb84c19c
        [HttpGet("{id:guid}")]
        public ActionResult<Hero> Get([FromRoute]Guid id)
        {
            var maybeHero = HeroAdapter.Read(id);
            if (maybeHero.HasValue)
            {
                return maybeHero.Value;
            }

            return NotFound();
        }
    }
}
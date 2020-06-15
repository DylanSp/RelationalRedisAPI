using Data;
using Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalRedisAPI.Controllers
{
    [Route("api/heroes")]
    [ApiController]
    public class HeroesController : ControllerBase
    {
        private IEntityAdapter<Hero> HeroAdapter { get; }
        private IEntityAdapter<Power> PowerAdapter { get; }

        public HeroesController(IEntityAdapter<Hero> heroAdapter, IEntityAdapter<Power> powerAdapter)
        {
            this.HeroAdapter = heroAdapter;
            this.PowerAdapter = powerAdapter;
        }

        // GET api/heroes
        [HttpGet]
        public ActionResult<IEnumerable<Hero>> GetAll()
        {
            return HeroAdapter.ReadAll().ToList();
        }

        // GET api/heroes/32c64485-d35c-4a01-b412-06a9cb84c19c
        [HttpGet("{id:guid}")]
        public ActionResult<Hero> Get([FromRoute] Guid id)
        {
            var maybeHero = HeroAdapter.Read(id);
            if (maybeHero.HasValue)
            {
                return maybeHero.Value;
            }

            return NotFound();
        }

        // POST api/heroes
        [HttpPost]
        public ActionResult<Hero> Create([FromBody] Hero hero)
        {
            var heroExists = HeroAdapter.Read(hero.Id).HasValue;
            if (heroExists)
            {
                return UnprocessableEntity();
            }

            // TODO - is this needed?
            foreach (var power in hero.Powers)
            {
                PowerAdapter.Save(power);
            }
            HeroAdapter.Save(hero);

            return Created($"{Request.Scheme}://{Request.Host}{Request.Path}/{hero.Id}", hero);
        }

        // POST api/heroes/32c64485-d35c-4a01-b412-06a9cb84c19c
        [HttpPost("{id:guid}")]
        public ActionResult<Hero> Update([FromRoute]Guid id, [FromBody] Hero hero)
        {
            var heroExists = HeroAdapter.Read(id).HasValue;
            if (!heroExists)
            {
                return NotFound();
            }

            if (id != hero.Id)
            {
                return BadRequest();
            }

            HeroAdapter.Save(hero);
            return hero;
        }
    }
}
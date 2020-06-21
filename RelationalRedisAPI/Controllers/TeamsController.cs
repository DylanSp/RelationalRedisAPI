using Adapters.Interfaces;
using Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RelationalRedisAPI.Controllers
{
    [Route("api/teams")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private ITeamAdapter TeamAdapter { get; }

        public TeamsController(ITeamAdapter teamAdapter)
        {
            this.TeamAdapter = teamAdapter;
        }

        // GET api/teams
        [HttpGet]
        public ActionResult<IEnumerable<Team>> GetTeams([FromQuery] string teamName, [FromQuery] string heroId, [FromQuery] string heroName)
        {
            throw new NotImplementedException();
        }

        // GET api/teams/32c64485-d35c-4a01-b412-06a9cb84c19c
        [HttpGet("{id:guid}", Name = nameof(GetTeam))] // named so Create() can refer to it with CreatedAtRoute()
        public ActionResult<Team> GetTeam([FromRoute] Guid id)
        {
            throw new NotImplementedException();
        }

        // POST api/teams
        [HttpPost]
        public ActionResult<Team> CreateTeam([FromBody] Team team)
        {
            throw new NotImplementedException();
        }

        // POST api/teams/32c64485-d35c-4a01-b412-06a9cb84c19c
        [HttpPost("{id:guid}")]
        public ActionResult<Team> UpdateTeam([FromRoute] Guid id, [FromBody] Team team)
        {
            throw new NotImplementedException();
        }

        // DELETE api/teams/32c64485-d35c-4a01-b412-06a9cb84c19c
        [HttpDelete("{id:guid}")]
        public ActionResult DeleteTeam([FromRoute] Guid id)
        {
            throw new NotImplementedException();
        }
    }
}

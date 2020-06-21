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
            if (!string.IsNullOrWhiteSpace(teamName))
            {
                return TeamAdapter.SearchTeams(teamName).ToList();
            }

            if (Guid.TryParse(heroId, out var heroGuid))
            {
                var possiblyMatchingTeam = TeamAdapter.SearchTeamsByMember(heroGuid);
                var teams = new List<Team>();
                if (possiblyMatchingTeam.HasValue)
                {
                    teams.Add(possiblyMatchingTeam.Value);
                }
                return teams;
            }

            if (!string.IsNullOrWhiteSpace(heroName))
            {
                var possiblyMatchingTeam = TeamAdapter.SearchTeamsByMember(heroName);
                var teams = new List<Team>();
                if (possiblyMatchingTeam.HasValue)
                {
                    teams.Add(possiblyMatchingTeam.Value);
                }
                return teams;
            }

            return TeamAdapter.ReadAll().ToList();
        }

        // GET api/teams/32c64485-d35c-4a01-b412-06a9cb84c19c
        [HttpGet("{id:guid}", Name = nameof(GetTeam))] // named so Create() can refer to it with CreatedAtRoute()
        public ActionResult<Team> GetTeam([FromRoute] Guid id)
        {
            var maybeTeam = TeamAdapter.Read(id);
            if (maybeTeam.HasValue)
            {
                return maybeTeam.Value;
            }

            return NotFound();
        }

        // POST api/teams
        [HttpPost]
        public ActionResult<Team> CreateTeam([FromBody] Team team)
        {
            var teamExists = TeamAdapter.Read(team.Id).HasValue;
            if (teamExists)
            {
                return UnprocessableEntity();
            }

            TeamAdapter.Save(team);

            return CreatedAtRoute(nameof(GetTeam), new { team.Id }, team);
        }

        // POST api/teams/32c64485-d35c-4a01-b412-06a9cb84c19c
        [HttpPost("{id:guid}")]
        public ActionResult<Team> UpdateTeam([FromRoute] Guid id, [FromBody] Team team)
        {
            var teamExists = TeamAdapter.Read(id).HasValue;
            if (!teamExists)
            {
                return NotFound();
            }

            if (id != team.Id)
            {
                return BadRequest();
            }

            TeamAdapter.Save(team);
            return team;
        }

        // DELETE api/teams/32c64485-d35c-4a01-b412-06a9cb84c19c
        [HttpDelete("{id:guid}")]
        public ActionResult DeleteTeam([FromRoute] Guid id)
        {
            var teamExists = TeamAdapter.Read(id).HasValue;
            if (!teamExists)
            {
                return NotFound();
            }

            TeamAdapter.Delete(id);
            return NoContent();
        }
    }
}

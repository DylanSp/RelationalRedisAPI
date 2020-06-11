using Data;
using Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace Adapters
{
    public class TeamRedisAdapter : IEntityAdapter<Team>
    {
        private readonly IDatabase Db;
        private readonly IEntityAdapter<Hero> HeroAdapter;

        public TeamRedisAdapter(string connectionString, IEntityAdapter<Hero> heroAdapter)
        {
            var redis = ConnectionMultiplexer.Connect(connectionString);
            Db = redis.GetDatabase();

            this.HeroAdapter = heroAdapter;
        }

        // takes n + 1 queries - is there a way to do this in 1?
        public IEnumerable<Team> ReadAll()
        {
            var teams = new List<Team>();

            var teamIds = Db.SetMembers("allteams");
            foreach (var stringId in teamIds)
            {
                var guidId = Guid.Parse(stringId);
                var team = Read(guidId);
                if (team.HasValue)      // should always fire, assuming integrity of allteams set is maintained
                {
                    teams.Add(team.Value);
                }
            }

            return teams;
        }

        public Team? Read(Guid id)
        {
            var isPresent = Db.KeyExists($"team:{id.ToString()}");
            if (!isPresent)
            {
                return null;
            }

            var name = Db.HashGet($"team:{id.ToString()}", "name");

            var members = new List<Hero>();

            var heroIds = Db.SetMembers($"teammembers:{id.ToString()}");
            // n queries to load heroes; can we do better?
            foreach (var heroId in heroIds)
            {
                var hero = HeroAdapter.Read(Guid.Parse(heroId));
                if (hero.HasValue)      // should always fire, assuming consistency of teammembers set
                {
                    members.Add(hero.Value);
                }
            }

            return new Team(id, name, members);
        }

        public void Save(Team newTeamData)
        {
            var isPresent = Db.KeyExists(newTeamData.Id.ToString());
            if (!isPresent)     // if inserting, update "allteams" set as well
            {
                Db.SetAdd("allteams", newTeamData.Id.ToString());
            }

            Db.HashSet($"team:{newTeamData.Id.ToString()}", new HashEntry[] { new HashEntry("name", newTeamData.Name) });

            // n queries - can we do better?
            foreach (var hero in newTeamData.Members)
            {
                HeroAdapter.Save(hero);
                Db.SetAdd($"teammembers:{newTeamData.Id.ToString()}", hero.Id.ToString());
            }
            // TODO - compare new members to old members in update case, remove members in DB
        }

        public void Delete(Guid id)
        {
            Db.KeyDelete($"team:{id.ToString()}");

            // delete member heroes
            var heroIds = Db.SetMembers($"teammembers:{id.ToString()}");
            // n queries to delete heroes; can we do better?
            foreach (var heroId in heroIds)
            {
                HeroAdapter.Delete(Guid.Parse(heroId));
            }

            Db.KeyDelete($"teammembers:{id.ToString()}");
        }
    }
}

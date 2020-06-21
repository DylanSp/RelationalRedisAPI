using Adapters.Interfaces;
using Dapper;
using Data;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Adapters
{
    public class TeamSqlAdapter : ITeamAdapter
    {
        private readonly string ConnectionString;
        private readonly IEntityAdapter<Hero> HeroAdapter;

        public TeamSqlAdapter(string connectionString, IEntityAdapter<Hero> heroAdapter)
        {
            this.ConnectionString = connectionString;
            this.HeroAdapter = heroAdapter;
        }

        public IEnumerable<Team> ReadAll()
        {
            var teams = new List<Team>();

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var heroQuery = @"SELECT Id, TeamId
                                  FROM Heroes";
                var heroCommand = new SqlCommand(heroQuery, connection);

                var teamIdsWithHeroIds = new Dictionary<Guid, List<Guid>>();
                using (var reader = heroCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var heroId = (Guid)reader["Id"];
                        var teamId = reader["TeamId"] == DBNull.Value ? null : (Guid?)reader["TeamId"];
                        if (teamId.HasValue)
                        {
                            if (!teamIdsWithHeroIds.ContainsKey(teamId.Value))
                            {
                                teamIdsWithHeroIds[teamId.Value] = new List<Guid>();
                            }
                            teamIdsWithHeroIds[teamId.Value].Add(heroId);
                        }
                    }
                }

                var teamQueryString = "SELECT Id, Name FROM Teams";
                var teamCommand = new SqlCommand(teamQueryString, connection);

                using (var reader = teamCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var heroes = new List<Hero>();
                        var teamId = (Guid)reader["Id"];
                        var teamName = (string)reader["Name"];
                        foreach(var heroId in teamIdsWithHeroIds.GetValueOrDefault(teamId) ?? new List<Guid>())
                        {
                            var maybeHero = HeroAdapter.Read(heroId);
                            if (maybeHero.HasValue) // should always fire
                            {
                                heroes.Add(maybeHero.Value);
                            }
                        }
                        teams.Add(new Team(teamId, teamName, heroes));
                    }
                }
            }
                

            return teams;
        }

        public IEnumerable<Team> SearchTeams(string name)
        {
            var builder = new SqlBuilder();
            var template = builder.AddTemplate(@"SELECT Id
                                                 FROM Teams
                                                 /**where**/");

            if (!string.IsNullOrWhiteSpace(name))
            {
                builder.Where("Name = @Name", new { name });
            }

            IEnumerable<Guid> teamIds;
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                teamIds = connection.Query<Guid>(template.RawSql, template.Parameters);
            }

            // TODO - optimize this by doing joins in original query, read entire teams from that query (but how to handle getting powers for heroes?)
            var teams = new List<Team>();
            foreach (var teamId in teamIds)
            {
                var possibleHero = Read(teamId);
                if (possibleHero.HasValue)
                {
                    teams.Add(possibleHero.Value);
                }
            }
            return teams;
        }

        public Team? SearchTeamsByMember(Guid heroId)
        {
            var allTeams = ReadAll();
            var matchingTeams = allTeams.Where(team => team.Members.Select(hero => hero.Id).Contains(heroId));
            if (matchingTeams.Any())
            {
                return matchingTeams.First();
            }

            return null;
        }

        public Team? SearchTeamsByMember(string heroName)
        {
            var allTeams = ReadAll();
            var matchingTeams = allTeams.Where(team => team.Members.Select(hero => hero.Name).Contains(heroName));
            if (matchingTeams.Any())
            {
                return matchingTeams.First();
            }

            return null;
        }

        public Team? Read(Guid id)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var teamQueryString = "SELECT Id, Name FROM Teams WHERE Id = @id";
                var teamCommand = new SqlCommand(teamQueryString, connection);
                teamCommand.Parameters.AddWithValue("@id", id);

                connection.Open();

                Guid teamId;
                string teamName;

                using (var reader = teamCommand.ExecuteReader())
                {
                    var readResult = reader.Read();
                    if (!readResult)
                    {
                        return null;    // team doesn't exist
                    }

                    teamId = (Guid)reader["Id"];
                    teamName = (string)reader["Name"];
                }

                IEnumerable<Hero> teamMembers = new List<Hero>();   // need to explicitly type this as IEnumerable<Hero>,
                                                                    // else Append() call below will have a type error

                // TODO - n + 1 query problem, because we need to use heroAdapter.Read() to fetch heroes w/ powers
                // TODO - if we didn't need that, we could just do a single SELECT FROM Heroes WHERE TeamId = id
                var heroQueryString = "SELECT Id FROM Heroes WHERE TeamId = @teamid";
                var heroCommand = new SqlCommand(heroQueryString, connection);
                heroCommand.Parameters.AddWithValue("@teamid", id);

                using (var reader = heroCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var heroId = (Guid)reader["Id"];
                        var hero = HeroAdapter.Read(heroId);
                        if (hero.HasValue)
                        {
                            teamMembers = teamMembers.Append(hero.Value);
                        }
                    }
                }

                return new Team(teamId, teamName, teamMembers);
            }
        }

        public void Save(Team newTeamData)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var teamQueryString = @"IF EXISTS ( SELECT * FROM Teams WHERE Id = @id )
                                            UPDATE Teams SET Name = @name WHERE Id = @id
                                        ELSE
                                            INSERT Teams (Id, Name) VALUES (@id, @name)";
                var teamCommand = new SqlCommand(teamQueryString, connection);
                teamCommand.Parameters.AddWithValue("@id", newTeamData.Id);
                teamCommand.Parameters.AddWithValue("@name", newTeamData.Name);

                connection.Open();
                teamCommand.ExecuteNonQuery();

                // TODO - n + 1 problem? but how to solve, especially with powers?
                foreach (var newHeroData in newTeamData.Members)
                {
                    HeroAdapter.Save(newHeroData);

                    // augment saved heroes with team membership
                    var heroQueryString = @"UPDATE Heroes SET TeamId = @teamid WHERE Id = @heroid";
                    var heroCommand = new SqlCommand(heroQueryString, connection);
                    heroCommand.Parameters.AddWithValue(@"teamid", newTeamData.Id);
                    heroCommand.Parameters.AddWithValue(@"heroid", newHeroData.Id);

                    heroCommand.ExecuteNonQuery();

                }
                // TODO - compare new members to old members in update case, remove members in DB
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var queryString = "DELETE FROM Teams WHERE Id = @id";
                var command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}

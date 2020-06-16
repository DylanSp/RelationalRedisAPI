using Adapters.Interfaces;
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
                var heroes = HeroAdapter.ReadAll(); // TODO - up here so it doesn't conflict with connection opened below, but is that necessary?

                var teamQueryString = "SELECT Id, Name FROM Teams";
                var teamCommand = new SqlCommand(teamQueryString, connection);

                connection.Open();

                using (var reader = teamCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        teams.Add(new Team((Guid)reader["Id"], (string)reader["Name"], new List<Hero>()));
                    }
                }

                // add in teams' heroes
                // TODO - n + 1 query problem; to avoid, seems like we'd have to replicate hero-reading logic from HeroSqlAdapter here
                var heroQueryString = "SELECT Id, TeamId FROM Heroes";
                var heroCommand = new SqlCommand(heroQueryString, connection);
                
                using (var reader = heroCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var heroId = (Guid)reader["Id"];
                        var dbTeamId = reader["TeamId"];
                        Guid? teamId = dbTeamId == DBNull.Value
                                        ? null
                                        : (Guid?)dbTeamId;

                        var matchingTeams = teams.Where(team => team.Id == teamId);
                        if (matchingTeams.Count() > 0)
                        {
                            var hero = HeroAdapter.Read(heroId);    // TODO - issues with creating new DB connection while executing this one?
                                                                    // TODO - if everything is in a transaction, can you have nested transactions?
                            if (hero.HasValue)  // should always fire, assuming database doesn't change between ExecuteReader() call and Read()
                            {
                                // First() should never throw; the matchingTeams.Count() > 0 condition requires at least one match
                                matchingTeams.First().Members.Append(hero.Value);
                            }
                        }
                    }
                }
            }

            return teams;
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

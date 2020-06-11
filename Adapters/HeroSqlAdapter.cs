using Data;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Adapters
{
    public class HeroSqlAdapter : IEntityAdapter<Hero>
    {
        private readonly string ConnectionString;
        private readonly IEntityAdapter<Power> PowerAdapter;

        public HeroSqlAdapter(string connectionString, IEntityAdapter<Power> powerAdapter)
        {
            this.ConnectionString = connectionString;
            this.PowerAdapter = powerAdapter;
        }

        public IEnumerable<Hero> ReadAll()
        {
            var heroes = new List<Hero>();

            using (var connection = new SqlConnection(ConnectionString))
            {
                var heroQueryString = "SELECT Id, Name, Location FROM Heroes";
                var heroCommand = new SqlCommand(heroQueryString, connection);

                connection.Open();

                using (var reader = heroCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        heroes.Add(new Hero((Guid)reader["Id"], (string)reader["Name"], (string)reader["Location"], new List<Power>()));
                    }
                }

                // TODO n + 1 query problem, kind of; is there a way to avoid this?
                // actually, O(|Heroes| * |Powers|)
                foreach (var hero in heroes)
                {
                    var allPowers = PowerAdapter.ReadAll();
                    foreach (var power in allPowers)
                    {
                        var associationQueryString = "SELECT * FROM HeroesPowersAssociation WHERE HeroId = @heroid AND PowerId = @powerid";
                        var associationCommand = new SqlCommand(associationQueryString, connection);
                        associationCommand.Parameters.AddWithValue("@heroid", hero.Id);
                        associationCommand.Parameters.AddWithValue("@powerid", power.Id);

                        using (var reader = associationCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hero.Powers.Append(power);
                            }
                        }
                    }
                }
            }

            return heroes;
        }

        public Hero? Read(Guid id)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var heroQueryString = "SELECT Id, Name, Location FROM Heroes WHERE Id = @id";
                var heroCommand = new SqlCommand(heroQueryString, connection);
                heroCommand.Parameters.AddWithValue("@id", id);

                connection.Open();

                Guid heroId;
                string heroName;
                string heroLocation;

                using (var reader = heroCommand.ExecuteReader())
                {
                    var readResult = reader.Read();
                    if (!readResult)
                    {
                        return null;    // hero not present
                    }

                    heroId = (Guid)reader["Id"];
                    heroName = (string)reader["Name"];
                    heroLocation = (string)reader["Location"];
                }

                var powersQueryString = @"SELECT Powers.Id as Id, Powers.Name as Name
                                          FROM Powers 
                                          INNER JOIN HeroesPowersAssociation 
                                            ON Powers.Id = HeroesPowersAssociation.PowerId
                                          WHERE HeroesPowersAssociation.HeroId = @heroid";
                var powersCommand = new SqlCommand(powersQueryString, connection);
                powersCommand.Parameters.AddWithValue("@heroid", id);

                var powers = new List<Power>();

                using (var reader = powersCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var powerId = (Guid)reader["Id"];
                        var powerName = (string)reader["Name"];
                        powers.Add(new Power(powerId, powerName));
                    }
                }

                return new Hero(heroId, heroName, heroLocation, powers);
            }
        }

        // TODO - wrap this all in a single transaction, so it executes atomically
        public void Save(Hero newHeroData)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                // update heroes table
                var heroQueryString = @"IF EXISTS ( SELECT * FROM Heroes WHERE Id = @id)
                                            UPDATE Heroes SET Name = @name, Location = @location WHERE Id = @id
                                        ELSE
                                            INSERT Heroes (Id, Name, Location) VALUES (@id, @name, @location)";
                var heroCommand = new SqlCommand(heroQueryString, connection);
                heroCommand.Parameters.AddWithValue("@id", newHeroData.Id);
                heroCommand.Parameters.AddWithValue("@name", newHeroData.Name);
                heroCommand.Parameters.AddWithValue("@location", newHeroData.Location);

                connection.Open();

                heroCommand.ExecuteNonQuery();

                // TODO - this requires n queries. how to limit # of queries?
                // TODO - Have a PowerAdapter as dependency, call RetrieveAll() from that, compare results against newHeroData.Powers, run 1 insert query, 1 update query?
                foreach (var power in newHeroData.Powers)
                {
                    // update powers table
                    var powerQueryString = @"IF EXISTS ( SELECT * FROM Powers WHERE Id = @id)
                                                UPDATE Powers SET Name = @name WHERE Id = @id
                                             ELSE
                                                INSERT Powers (Id, Name) VALUES (@id, @name)";
                    var powerCommand = new SqlCommand(powerQueryString, connection);
                    powerCommand.Parameters.AddWithValue("@id", power.Id);
                    powerCommand.Parameters.AddWithValue("@name", power.Name);

                    powerCommand.ExecuteNonQuery();

                    // update hero-power association table
                    // TODO - is there a more elegant way than deleting everything and re-adding current associations?
                    var heroPowerDeleteString = @"DELETE FROM HeroesPowersAssociation WHERE HeroId = @heroid";
                    var heroPowerDeleteCommand = new SqlCommand(heroPowerDeleteString, connection);
                    heroPowerDeleteCommand.Parameters.AddWithValue("@heroid", newHeroData.Id);

                    heroPowerDeleteCommand.ExecuteNonQuery();

                    var heroPowerCreateString = @"INSERT HeroesPowersAssociation (HeroId, PowerId) VALUES (@heroid, @powerid)";
                    var heroPowerCreateCommand = new SqlCommand(heroPowerCreateString, connection);
                    heroPowerCreateCommand.Parameters.AddWithValue("@heroid", newHeroData.Id);
                    heroPowerCreateCommand.Parameters.AddWithValue("@powerid", power.Id);

                    heroPowerCreateCommand.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var queryString = "DELETE FROM Heroes WHERE Id = @id";
                var command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}

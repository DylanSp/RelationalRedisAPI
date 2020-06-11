using Data;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Adapters
{
    public class PowerSqlAdapter : IEntityAdapter<Power>
    {
        private readonly string ConnectionString;

        public PowerSqlAdapter(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public IEnumerable<Power> ReadAll()
        {
            var powers = new List<Power>();

            using (var connection = new SqlConnection(ConnectionString))
            {
                var queryString = "SELECT Id, Name FROM Powers";
                var command = new SqlCommand(queryString, connection);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        powers.Add(new Power((Guid)reader["Id"], (string)reader["Name"]));
                    }
                }
            }

            return powers;
        }

        public Power? Read(Guid id)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var queryString = "SELECT Id, Name FROM Powers WHERE Id = @id";
                var command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Power((Guid)reader["Id"], (string)reader["Name"]);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public void Save(Power newPowerData)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var queryString = @"IF EXISTS ( SELECT * FROM Powers WHERE Id = @id)
                                        UPDATE Powers SET Name = @name WHERE Id = @id
                                    ELSE
                                        INSERT Powers (Id, Name) VALUES (@id, @name)";
                var command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@id", newPowerData.Id);
                command.Parameters.AddWithValue("@name", newPowerData.Name);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var queryString = "DELETE FROM Powers WHERE Id = @id";
                var command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}

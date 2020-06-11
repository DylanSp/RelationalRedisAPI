using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace RelationalRedisAPI.Tests.Helpers
{
    public static class AdapterTestHelpers
    {
        public static int CountSqlPowers(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var queryString = "SELECT COUNT(*) FROM Powers";
                var command = new SqlCommand(queryString, connection);

                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }

        public static int CountSqlTeams(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var queryString = "SELECT COUNT(*) FROM Teams";
                var command = new SqlCommand(queryString, connection);

                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }

        public static int CountSqlHeroes(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var queryString = "SELECT COUNT(*) FROM Heroes";
                var command = new SqlCommand(queryString, connection);

                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }

        public static int CountRedisPowers(string connectionString)
        {
            var conn = ConnectionMultiplexer.Connect(connectionString);
            var endpoints = conn.GetEndPoints();
            var server = conn.GetServer(endpoints[0]);
            return server.Keys(pattern:"power:*").Count();
        }

        public static int CountRedisTeams(string connectionString)
        {
            var conn = ConnectionMultiplexer.Connect(connectionString);
            var endpoints = conn.GetEndPoints();
            var server = conn.GetServer(endpoints[0]);
            return server.Keys(pattern: "team:*").Count();
        }

        public static int CountRedisHeroes(string connectionString)
        {
            var conn = ConnectionMultiplexer.Connect(connectionString);
            var endpoints = conn.GetEndPoints();
            var server = conn.GetServer(endpoints[0]);
            return server.Keys(pattern: "hero:*").Count();
        }
    }
}

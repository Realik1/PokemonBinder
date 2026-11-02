using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace PokemonBinder.Data
{
    public class ConnectionManager
    {
        private readonly string _connectionString;

        public ConnectionManager(string databasePath)
        {
            _connectionString = $"Data Source={databasePath};";
        }

        public SqliteConnection GetConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        public async Task<T> ExecuteQueryAsync<T>(
            string query,
            Func<SqliteDataReader, T> mapFunction,
            SqliteParameter[] parameters = null)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return mapFunction(reader);
                        }
                    }
                }
            }
            return default;
        }

        public async Task<List<T>> ExecuteQueryListAsync<T>(
            string query,
            Func<SqliteDataReader, T> mapFunction,
            SqliteParameter[] parameters = null)
        {
            var results = new List<T>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(mapFunction(reader));
                        }
                    }
                }
            }

            return results;
        }

        public async Task<int> ExecuteNonQueryAsync(
            string query,
            SqliteParameter[] parameters = null)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<object> ExecuteScalarAsync(
            string query,
            SqliteParameter[] parameters = null)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    return await command.ExecuteScalarAsync();
                }
            }
        }
    }
}

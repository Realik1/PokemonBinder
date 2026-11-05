using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using PokemonBinder.Data;
using PokemonBinder.Models;

namespace PokemonBinder.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly ConnectionManager _connectionManager;

        public CardRepository(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task<Card?> GetByIdAsync(int id)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, UniqueID, Name, Artist, PackName, PokedexNo, Type, Rarity, 
                       CardNumber, Stage, HP, CardImageId, DateAdded 
                FROM Cards 
                WHERE Id = @Id;";
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToCard(reader);
            }

            return null;
        }

        public async Task<Card?> GetByUniqueIdAsync(string uniqueId)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, UniqueID, Name, Artist, PackName, PokedexNo, Type, Rarity, 
                       CardNumber, Stage, HP, CardImageId, DateAdded 
                FROM Cards 
                WHERE UniqueID = @UniqueID;";
            command.Parameters.AddWithValue("@UniqueID", uniqueId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToCard(reader);
            }

            return null;
        }

        public async Task<List<Card>> GetAllAsync()
        {
            var cards = new List<Card>();
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, UniqueID, Name, Artist, PackName, PokedexNo, Type, Rarity, 
                       CardNumber, Stage, HP, CardImageId, DateAdded 
                FROM Cards;";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                cards.Add(MapReaderToCard(reader));
            }

            return cards;
        }

        public async Task<List<Card>> SearchAsync(string name, string packName = null)
        {
            var cards = new List<Card>();
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            var query = @"
                SELECT Id, UniqueID, Name, Artist, PackName, PokedexNo, Type, Rarity, 
                       CardNumber, Stage, HP, CardImageId, DateAdded 
                FROM Cards WHERE 1=1";

            if (!string.IsNullOrEmpty(name))
            {
                query += " AND Name LIKE @Name";
                command.Parameters.AddWithValue("@Name", $"%{name}%");
            }
            if (!string.IsNullOrEmpty(packName))
            {
                query += " AND PackName LIKE @PackName";
                command.Parameters.AddWithValue("@PackName", $"%{packName}%");
            }

            command.CommandText = query;

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                cards.Add(MapReaderToCard(reader));
            }

            return cards;
        }

        public async Task<int> AddAsync(Card card)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Cards (UniqueID, Name, Artist, PackName, PokedexNo, Type, Rarity, CardNumber, Stage, HP, CardImageId, DateAdded)
                VALUES (@UniqueID, @Name, @Artist, @PackName, @PokedexNo, @Type, @Rarity, @CardNumber, @Stage, @HP, @CardImageId, @DateAdded);
                SELECT last_insert_rowid();";

            AddCardParameters(command, card);
            var newId = await command.ExecuteScalarAsync();
            return Convert.ToInt32(newId);
        }

        public async Task<bool> UpdateAsync(Card card)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Cards 
                SET UniqueID = @UniqueID, 
                    Name = @Name, 
                    Artist = @Artist, 
                    PackName = @PackName, 
                    PokedexNo = @PokedexNo, 
                    Type = @Type, 
                    Rarity = @Rarity, 
                    CardNumber = @CardNumber, 
                    Stage = @Stage, 
                    HP = @HP, 
                    CardImageId = @CardImageId 
                WHERE Id = @Id;";

            command.Parameters.AddWithValue("@Id", card.Id);
            AddCardParameters(command, card);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Cards WHERE Id = @Id;";
            command.Parameters.AddWithValue("@Id", id);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<int> AddImageAsync(CardImage image)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO CardImages (ImageData, ImagePath, MimeType)
                VALUES (@ImageData, @ImagePath, @MimeType);
                SELECT last_insert_rowid();";

            command.Parameters.AddWithValue("@ImageData", image.ImageData);
            command.Parameters.AddWithValue("@ImagePath", image.ImagePath ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@MimeType", image.MimeType ?? "image/jpeg");

            var newId = await command.ExecuteScalarAsync();
            return Convert.ToInt32(newId);
        }

        public async Task<CardImage?> GetImageByIdAsync(int id)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, ImageData, ImagePath, MimeType FROM CardImages WHERE Id = @Id;";
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new CardImage
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    ImageData = (byte[])reader["ImageData"],
                    ImagePath = reader["ImagePath"]?.ToString(),
                    MimeType = reader["MimeType"]?.ToString() ?? "image/jpeg"
                };
            }

            return null;
        }

        private void AddCardParameters(SqliteCommand command, Card card)
        {
            command.Parameters.AddWithValue("@UniqueID", card.UniqueID ?? string.Empty);
            command.Parameters.AddWithValue("@Name", card.Name ?? string.Empty);
            command.Parameters.AddWithValue("@Artist", (object?)card.Artist ?? DBNull.Value);
            command.Parameters.AddWithValue("@PackName", card.PackName ?? string.Empty);
            command.Parameters.AddWithValue("@PokedexNo", card.PokedexNo);
            command.Parameters.AddWithValue("@Type", card.Type ?? string.Empty);
            command.Parameters.AddWithValue("@Rarity", card.Rarity ?? string.Empty);
            command.Parameters.AddWithValue("@CardNumber", card.CardNumber ?? string.Empty);
            command.Parameters.AddWithValue("@Stage", card.Stage ?? string.Empty);
            command.Parameters.AddWithValue("@HP", card.HP);
            command.Parameters.AddWithValue("@CardImageId", card.CardImageId > 0 ? (object)card.CardImageId : DBNull.Value);
            command.Parameters.AddWithValue("@DateAdded", card.DateAdded == default ? DateTime.Now : card.DateAdded);
        }

        private Card MapReaderToCard(SqliteDataReader reader)
        {
            return new Card
            {
                Id = Convert.ToInt32(reader["Id"]),
                UniqueID = reader["UniqueID"]?.ToString() ?? string.Empty,
                Name = reader["Name"]?.ToString() ?? string.Empty,
                Artist = reader["Artist"]?.ToString() ?? string.Empty,
                PackName = reader["PackName"]?.ToString() ?? string.Empty,
                PokedexNo = reader["PokedexNo"] != DBNull.Value ? Convert.ToInt32(reader["PokedexNo"]) : 0,
                Type = reader["Type"]?.ToString() ?? string.Empty,
                Rarity = reader["Rarity"]?.ToString() ?? string.Empty,
                CardNumber = reader["CardNumber"]?.ToString() ?? string.Empty,
                Stage = reader["Stage"]?.ToString() ?? string.Empty,
                HP = reader["HP"] != DBNull.Value ? Convert.ToInt32(reader["HP"]) : 0,
                CardImageId = reader["CardImageId"] != DBNull.Value ? Convert.ToInt32(reader["CardImageId"]) : 0,
                DateAdded = reader["DateAdded"] != DBNull.Value ? Convert.ToDateTime(reader["DateAdded"]) : DateTime.Now
            };
        }
    }
}
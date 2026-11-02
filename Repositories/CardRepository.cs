using Microsoft.Data.Sqlite;
using PokemonBinder.Data;
using PokemonBinder.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonBinder.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly ConnectionManager _connectionManager;

        public CardRepository(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task<Card> GetByIdAsync(int id)
        {
            var query = @"
                SELECT Id, UniqueID, Name, Artist, PackName, PokedexNo, Type, 
                       IsEX, Rarity, IsFullArt, IsHolo, HoloType, CardImageId, DateAdded
                FROM Cards
                WHERE Id = @Id
            ";

            var parameters = new[] { new SqliteParameter("@Id", id) };

            return await _connectionManager.ExecuteQueryAsync(query, reader =>
                MapCard(reader), parameters);
        }

        public async Task<Card> GetByUniqueIdAsync(string uniqueId)
        {
            var query = @"
                SELECT Id, UniqueID, Name, Artist, PackName, PokedexNo, Type, 
                       IsEX, Rarity, IsFullArt, IsHolo, HoloType, CardImageId, DateAdded
                FROM Cards
                WHERE UniqueID = @UniqueID
            ";

            var parameters = new[] { new SqliteParameter("@UniqueID", uniqueId) };

            return await _connectionManager.ExecuteQueryAsync(query, reader =>
                MapCard(reader), parameters);
        }

        public async Task<List<Card>> GetAllAsync()
        {
            var query = @"
                SELECT Id, UniqueID, Name, Artist, PackName, PokedexNo, Type, 
                       IsEX, Rarity, IsFullArt, IsHolo, HoloType, CardImageId, DateAdded
                FROM Cards
                ORDER BY Name
            ";

            return await _connectionManager.ExecuteQueryListAsync(query, reader =>
                MapCard(reader));
        }

        public async Task<List<Card>> SearchAsync(string name, string packName = null)
        {
            var query = @"
                SELECT Id, UniqueID, Name, Artist, PackName, PokedexNo, Type, 
                       IsEX, Rarity, IsFullArt, IsHolo, HoloType, CardImageId, DateAdded
                FROM Cards
                WHERE 1=1
            ";

            var parameters = new List<SqliteParameter>();

            if (!string.IsNullOrEmpty(name))
            {
                query += " AND Name LIKE @Name";
                parameters.Add(new SqliteParameter("@Name", $"%{name}%"));
            }

            if (!string.IsNullOrEmpty(packName))
            {
                query += " AND PackName = @PackName";
                parameters.Add(new SqliteParameter("@PackName", packName));
            }

            query += " ORDER BY Name";

            return await _connectionManager.ExecuteQueryListAsync(query, reader =>
                MapCard(reader), parameters.ToArray());
        }

        public async Task<int> AddAsync(Card card)
        {
            var query = @"
                INSERT INTO Cards (UniqueID, Name, Artist, PackName, PokedexNo, Type, 
                                   IsEX, Rarity, IsFullArt, IsHolo, HoloType, CardImageId)
                VALUES (@UniqueID, @Name, @Artist, @PackName, @PokedexNo, @Type, 
                        @IsEX, @Rarity, @IsFullArt, @IsHolo, @HoloType, @CardImageId);
                SELECT last_insert_rowid();
            ";

            var parameters = new[]
            {
                new SqliteParameter("@UniqueID", card.UniqueID),
                new SqliteParameter("@Name", card.Name),
                new SqliteParameter("@Artist", card.Artist ?? (object)DBNull.Value),
                new SqliteParameter("@PackName", card.PackName ?? (object)DBNull.Value),
                new SqliteParameter("@PokedexNo", card.PokedexNo),
                new SqliteParameter("@Type", card.Type ?? (object)DBNull.Value),
                new SqliteParameter("@IsEX", card.IsEX ? 1 : 0),
                new SqliteParameter("@Rarity", card.Rarity ?? (object)DBNull.Value),
                new SqliteParameter("@IsFullArt", card.IsFullArt ? 1 : 0),
                new SqliteParameter("@IsHolo", card.IsHolo ? 1 : 0),
                new SqliteParameter("@HoloType", card.HoloType ?? (object)DBNull.Value),
                new SqliteParameter("@CardImageId", card.CardImageId != 0 ? (object)card.CardImageId : DBNull.Value)
            };

            var result = await _connectionManager.ExecuteScalarAsync(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<bool> UpdateAsync(Card card)
        {
            var query = @"
                UPDATE Cards
                SET Name = @Name, Artist = @Artist, PackName = @PackName, PokedexNo = @PokedexNo,
                    Type = @Type, IsEX = @IsEX, Rarity = @Rarity, IsFullArt = @IsFullArt,
                    IsHolo = @IsHolo, HoloType = @HoloType, CardImageId = @CardImageId
                WHERE Id = @Id
            ";

            var parameters = new[]
            {
                new SqliteParameter("@Id", card.Id),
                new SqliteParameter("@Name", card.Name),
                new SqliteParameter("@Artist", card.Artist ?? (object)DBNull.Value),
                new SqliteParameter("@PackName", card.PackName ?? (object)DBNull.Value),
                new SqliteParameter("@PokedexNo", card.PokedexNo),
                new SqliteParameter("@Type", card.Type ?? (object)DBNull.Value),
                new SqliteParameter("@IsEX", card.IsEX ? 1 : 0),
                new SqliteParameter("@Rarity", card.Rarity ?? (object)DBNull.Value),
                new SqliteParameter("@IsFullArt", card.IsFullArt ? 1 : 0),
                new SqliteParameter("@IsHolo", card.IsHolo ? 1 : 0),
                new SqliteParameter("@HoloType", card.HoloType ?? (object)DBNull.Value),
                new SqliteParameter("@CardImageId", card.CardImageId != 0 ? (object)card.CardImageId : DBNull.Value)
            };

            var rowsAffected = await _connectionManager.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM Cards WHERE Id = @Id";
            var parameters = new[] { new SqliteParameter("@Id", id) };

            var rowsAffected = await _connectionManager.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<int> AddImageAsync(CardImage image)
        {
            var query = @"
                INSERT INTO CardImages (ImageData, ImagePath, MimeType)
                VALUES (@ImageData, @ImagePath, @MimeType);
                SELECT last_insert_rowid();
            ";

            var parameters = new[]
            {
                new SqliteParameter("@ImageData", image.ImageData),
                new SqliteParameter("@ImagePath", image.ImagePath ?? (object)DBNull.Value),
                new SqliteParameter("@MimeType", image.MimeType ?? "image/png")
            };

            var result = await _connectionManager.ExecuteScalarAsync(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<CardImage> GetImageByIdAsync(int id)
        {
            var query = @"
                SELECT Id, ImageData, ImagePath, MimeType
                FROM CardImages
                WHERE Id = @Id
            ";

            var parameters = new[] { new SqliteParameter("@Id", id) };

            return await _connectionManager.ExecuteQueryAsync(query, reader =>
                new CardImage
                {
                    Id = reader.GetInt32(0),
                    ImageData = (byte[])reader.GetValue(1),
                    ImagePath = reader.IsDBNull(2) ? null : reader.GetString(2),
                    MimeType = reader.IsDBNull(3) ? "image/png" : reader.GetString(3)
                }, parameters);
        }

        private Card MapCard(SqliteDataReader reader)
        {
            return new Card
            {
                Id = reader.GetInt32(0),
                UniqueID = reader.GetString(1),
                Name = reader.GetString(2),
                Artist = reader.IsDBNull(3) ? null : reader.GetString(3),
                PackName = reader.IsDBNull(4) ? null : reader.GetString(4),
                PokedexNo = reader.GetInt32(5),
                Type = reader.IsDBNull(6) ? null : reader.GetString(6),
                IsEX = reader.GetInt32(7) == 1,
                Rarity = reader.IsDBNull(8) ? null : reader.GetString(8),
                IsFullArt = reader.GetInt32(9) == 1,
                IsHolo = reader.GetInt32(10) == 1,
                HoloType = reader.IsDBNull(11) ? null : reader.GetString(11),
                CardImageId = reader.IsDBNull(12) ? 0 : reader.GetInt32(12),
                DateAdded = reader.GetDateTime(13)
            };
        }
    }
}

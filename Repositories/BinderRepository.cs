using Microsoft.Data.Sqlite;
using PokemonBinder.Data;
using PokemonBinder.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonBinder.Repositories
{
    public class BinderRepository : IBinderRepository
    {
        private readonly ConnectionManager _connectionManager;
        private readonly ICardRepository _cardRepository;

        public BinderRepository(ConnectionManager connectionManager, ICardRepository cardRepository)
        {
            _connectionManager = connectionManager;
            _cardRepository = cardRepository;
        }

        public async Task<Binder> GetByIdAsync(int id)
        {
            var query = @"
                SELECT Id, Name, Description, FrontCoverImage, BackCoverImage, 
                       CoverColor, CreatedDate, LastModified
                FROM Binders
                WHERE Id = @Id
            ";

            var parameters = new[] { new SqliteParameter("@Id", id) };

            var binder = await _connectionManager.ExecuteQueryAsync(query, reader =>
                MapBinder(reader), parameters);

            if (binder != null)
            {
                binder.Pages = await GetPagesByBinderIdAsync(id);
            }

            return binder;
        }

        public async Task<List<Binder>> GetAllAsync()
        {
            var query = @"
                SELECT Id, Name, Description, FrontCoverImage, BackCoverImage, 
                       CoverColor, CreatedDate, LastModified
                FROM Binders
                ORDER BY CreatedDate DESC
            ";

            var binders = await _connectionManager.ExecuteQueryListAsync(query, reader =>
                MapBinder(reader));

            // Load pages for each binder
            foreach (var binder in binders)
            {
                binder.Pages = await GetPagesByBinderIdAsync(binder.Id);
            }

            return binders;
        }

        public async Task<int> AddAsync(Binder binder)
        {
            var query = @"
                INSERT INTO Binders (Name, Description, FrontCoverImage, CoverColor)
                VALUES (@Name, @Description, @FrontCoverImage, @CoverColor);
                SELECT last_insert_rowid();
            ";

            var parameters = new[]
            {
                new SqliteParameter("@Name", binder.Name),
                new SqliteParameter("@Description", binder.Description ?? (object)DBNull.Value),
                new SqliteParameter("@FrontCoverImage", binder.FrontCoverImage ?? (object)DBNull.Value),
                new SqliteParameter("@CoverColor", binder.CoverColor ?? (object)DBNull.Value)
            };

            var result = await _connectionManager.ExecuteScalarAsync(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<bool> UpdateAsync(Binder binder)
        {
            var query = @"
                UPDATE Binders
                SET Name = @Name, Description = @Description, FrontCoverImage = @FrontCoverImage,
                    CoverColour = @CoverColour, LastModified = CURRENT_TIMESTAMP
                WHERE Id = @Id
            ";

            var parameters = new[]
            {
                new SqliteParameter("@Id", binder.Id),
                new SqliteParameter("@Name", binder.Name),
                new SqliteParameter("@Description", binder.Description ?? (object)DBNull.Value),
                new SqliteParameter("@FrontCoverImage", binder.FrontCoverImage ?? (object)DBNull.Value),
                new SqliteParameter("@CoverColor", binder.CoverColor ?? (object)DBNull.Value)
            };

            var rowsAffected = await _connectionManager.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "DELETE FROM Binders WHERE Id = @Id";
            var parameters = new[] { new SqliteParameter("@Id", id) };

            var rowsAffected = await _connectionManager.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<int> AddPageAsync(BinderPage page)
        {
            var query = @"
                INSERT INTO BinderPages (BinderId, PageNumber, GridSize)
                VALUES (@BinderId, @PageNumber, @GridSize);
                SELECT last_insert_rowid();
            ";

            var parameters = new[]
            {
                new SqliteParameter("@BinderId", page.BinderId),
                new SqliteParameter("@PageNumber", page.PageNumber),
                new SqliteParameter("@GridSize", (int)page.GridSize)
            };

            var result = await _connectionManager.ExecuteScalarAsync(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<List<BinderPage>> GetPagesByBinderIdAsync(int binderId)
        {
            var query = @"
                SELECT Id, BinderId, PageNumber, GridSize
                FROM BinderPages
                WHERE BinderId = @BinderId
                ORDER BY PageNumber
            ";

            var parameters = new[] { new SqliteParameter("@BinderId", binderId) };

            var pages = await _connectionManager.ExecuteQueryListAsync(query, reader =>
            {
                var page = new BinderPage
                {
                    Id = reader.GetInt32(0),
                    BinderId = reader.GetInt32(1),
                    PageNumber = reader.GetInt32(2),
                    GridSize = (GridSize)reader.GetInt32(3)
                };
                return page;
            }, parameters);

            // Load slots for each page
            foreach (var page in pages)
            {
                page.Slots = await GetSlotsByPageIdAsync(page.Id);
            }

            return pages;
        }

        public async Task<BinderPage> GetPageByIdAsync(int pageId)
        {
            var query = @"
                SELECT Id, BinderId, PageNumber, GridSize
                FROM BinderPages
                WHERE Id = @Id
            ";

            var parameters = new[] { new SqliteParameter("@Id", pageId) };

            var page = await _connectionManager.ExecuteQueryAsync(query, reader =>
                new BinderPage
                {
                    Id = reader.GetInt32(0),
                    BinderId = reader.GetInt32(1),
                    PageNumber = reader.GetInt32(2),
                    GridSize = (GridSize)reader.GetInt32(3)
                }, parameters);

            if (page != null)
            {
                page.Slots = await GetSlotsByPageIdAsync(pageId);
            }

            return page;
        }

        public async Task<int> AddSlotAsync(BinderSlot slot)
        {
            var query = @"
                INSERT INTO BinderSlots (BinderPageId, SlotPosition, CardId, IsPlaceholder)
                VALUES (@BinderPageId, @SlotPosition, @CardId, @IsPlaceholder);
                SELECT last_insert_rowid();
            ";

            var parameters = new[]
            {
                new SqliteParameter("@BinderPageId", slot.BinderPageId),
                new SqliteParameter("@SlotPosition", slot.SlotPosition),
                new SqliteParameter("@CardId", slot.CardId ?? (object)DBNull.Value),
                new SqliteParameter("@IsPlaceholder", slot.IsPlaceholder ? 1 : 0)
            };

            var result = await _connectionManager.ExecuteScalarAsync(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<List<BinderSlot>> GetSlotsByPageIdAsync(int pageId)
        {
            var query = @"
                SELECT Id, BinderPageId, SlotPosition, CardId, IsPlaceholder
                FROM BinderSlots
                WHERE BinderPageId = @BinderPageId
                ORDER BY SlotPosition
            ";

            var parameters = new[] { new SqliteParameter("@BinderPageId", pageId) };

            return await _connectionManager.ExecuteQueryListAsync(query, reader =>
                new BinderSlot
                {
                    Id = reader.GetInt32(0),
                    BinderPageId = reader.GetInt32(1),
                    SlotPosition = reader.GetInt32(2),
                    CardId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    IsPlaceholder = reader.GetInt32(4) == 1
                }, parameters);
        }

        public async Task<bool> UpdateSlotAsync(BinderSlot slot)
        {
            var query = @"
                UPDATE BinderSlots
                SET CardId = @CardId, IsPlaceholder = @IsPlaceholder
                WHERE Id = @Id
            ";

            var parameters = new[]
            {
                new SqliteParameter("@Id", slot.Id),
                new SqliteParameter("@CardId", slot.CardId ?? (object)DBNull.Value),
                new SqliteParameter("@IsPlaceholder", slot.IsPlaceholder ? 1 : 0)
            };

            var rowsAffected = await _connectionManager.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        private Binder MapBinder(SqliteDataReader reader)
        {
            return new Binder
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                FrontCoverImage = reader.IsDBNull(3) ? null : (byte[])reader.GetValue(3),
                BackCoverImage = reader.IsDBNull(4) ? null : (byte[])reader.GetValue(4),   
                CoverColor = reader.IsDBNull(5) ? "#1e293b" : reader.GetString(5),
                CreatedDate = reader.GetDateTime(6),
                LastModified = reader.GetDateTime(7)
            };
        }
    }
}
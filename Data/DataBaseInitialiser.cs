using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonBinder.Data
{
    public class DatabaseInitializer
    {
        private readonly string _databasePath;
        private readonly ConnectionManager _connectionManager;

        public DatabaseInitializer(string databasePath)
        {
            _databasePath = databasePath;
            _connectionManager = new ConnectionManager(databasePath);
        }

        public async Task InitializeAsync()
        {
            // Create database file if it doesn't exist
            if (!File.Exists(_databasePath))
            {
                File.Create(_databasePath).Dispose();
            }

            // Check if tables already exist
            bool tablesExist = await TablesExistAsync();

            if (!tablesExist)
            {
                await CreateTablesAsync();
            }
        }

        private async Task<bool> TablesExistAsync()
        {
            try
            {
                var query = @"
                    SELECT COUNT(*) FROM sqlite_master 
                    WHERE type='table' AND name='Cards'
                ";

                var result = await _connectionManager.ExecuteScalarAsync(query);
                return result != null && (long)result > 0;
            }
            catch
            {
                return false;
            }
        }

        private async Task CreateTablesAsync()
        {
            using (var connection = _connectionManager.GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // Create CardImages table
                        await ExecuteCommandAsync(connection, @"
                            CREATE TABLE CardImages (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                ImageData BLOB NOT NULL,
                                ImagePath TEXT,
                                MimeType TEXT DEFAULT 'image/png'
                            )
                        ");

                        // Create Cards table
                        await ExecuteCommandAsync(connection, @"
                            CREATE TABLE Cards (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                UniqueID TEXT NOT NULL UNIQUE,
                                Name TEXT NOT NULL,
                                Artist TEXT,
                                PackName TEXT,
                                PokedexNo INTEGER,
                                Type TEXT,
                                Rarity TEXT,
                                CardNumber TEXT,
                                Stage TEXT,
                                HP INTEGER DEFAULT 0,
                                CardImageId INTEGER,
                                DateAdded DATETIME DEFAULT CURRENT_TIMESTAMP,
                                FOREIGN KEY(CardImageId) REFERENCES CardImages(Id)
                            )");

                        // Create Binders table
                        await ExecuteCommandAsync(connection, @"
                            CREATE TABLE Binders (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                Name TEXT NOT NULL,
                                Description TEXT,
                                FrontCoverImage BLOB,
                                BackCoverImage BLOB,
                                CoverColor TEXT DEFAULT '#1e293b',
                                CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                                LastModified DATETIME DEFAULT CURRENT_TIMESTAMP
                            )
                        ");

                        // Create BinderPages table
                        await ExecuteCommandAsync(connection, @"
                            CREATE TABLE BinderPages (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                BinderId INTEGER NOT NULL,
                                PageNumber INTEGER NOT NULL,
                                GridSize INTEGER NOT NULL,
                                FOREIGN KEY(BinderId) REFERENCES Binders(Id) ON DELETE CASCADE
                            )
                        ");

                        // Create BinderSlots table
                        await ExecuteCommandAsync(connection, @"
                            CREATE TABLE BinderSlots (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                BinderPageId INTEGER NOT NULL,
                                SlotPosition INTEGER NOT NULL,
                                CardId INTEGER,
                                IsPlaceholder INTEGER DEFAULT 1,
                                FOREIGN KEY(BinderPageId) REFERENCES BinderPages(Id) ON DELETE CASCADE,
                                FOREIGN KEY(CardId) REFERENCES Cards(Id) ON DELETE SET NULL
                            )
                        ");

                        // Create indexes for better query performance
                        await ExecuteCommandAsync(connection, @"
                            CREATE INDEX idx_cards_name ON Cards(Name)
                        ");

                        await ExecuteCommandAsync(connection, @"
                            CREATE INDEX idx_cards_uniqueid ON Cards(UniqueID)
                        ");

                        await ExecuteCommandAsync(connection, @"
                            CREATE INDEX idx_binderpages_binder ON BinderPages(BinderId)
                        ");

                        await ExecuteCommandAsync(connection, @"
                            CREATE INDEX idx_binderslots_page ON BinderSlots(BinderPageId)
                        ");

                        await transaction.CommitAsync();
                        Console.WriteLine("Database initialized successfully.");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception("Failed to initialize database", ex);
                    }
                }
            }
        }

        private async Task ExecuteCommandAsync(SqliteConnection connection, string commandText)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}

using PokemonBinder.Models;
using PokemonBinder.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace PokemonBinder.Services
{
    public class CardService
    {
        private readonly ICardRepository _cardRepository;

        public CardService(ICardRepository cardRepository)
        {
            _cardRepository = cardRepository;
        }

        public async Task<Card> GetCardByIdAsync(int id)
        {
            return await _cardRepository.GetByIdAsync(id);
        }

        public async Task<Card> GetCardByUniqueIdAsync(string uniqueId)
        {
            return await _cardRepository.GetByUniqueIdAsync(uniqueId);
        }

        public async Task<List<Card>> GetAllCardsAsync()
        {
            return await _cardRepository.GetAllAsync();
        }

        public async Task<List<Card>> SearchCardsAsync(string name, string packName = null)
        {
            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(packName))
            {
                return await _cardRepository.GetAllAsync();
            }

            return await _cardRepository.SearchAsync(name, packName);
        }

        public async Task<int> ImportCardsFromCsvAsync(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0) return 0;

            string packName = Path.GetFileNameWithoutExtension(csvFile.FileName);
            
            int processedCount = 0;
            using var stream = csvFile.OpenReadStream();
            using var reader = new StreamReader(stream);

            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(headerLine)) return 0;

            var httpClient = new HttpClient();
            var processedProductIds = new HashSet<string>();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = line.Split(',');
                if (values.Length < 4) continue;

                string productId = values[0].Trim();
                string rawName = values[1].Trim('"', ' ');
                string imageUrl = values[3].Trim('"', ' ');
                string rarity = values.Length > 5 ? values[5].Trim('"', ' ') : "Common";
                string cardType = values.Length > 7 ? values[7].Trim('"', ' ') : "";

                if (processedProductIds.Contains(productId)) continue;
                processedProductIds.Add(productId);

                string cleanedName = Regex.Replace(rawName, @"\s*-\s*\d+/\d+", "").Trim();
                string uniqueId = $"{packName}-{productId}";

                // 1. Check if card already exists
                var existingCard = await _cardRepository.GetByUniqueIdAsync(uniqueId);

                int cardImageId = existingCard?.CardImageId ?? 0;

                // 2. Download/refresh image if URL is present (or if image doesn't exist yet)
                if (!string.IsNullOrEmpty(imageUrl) && imageUrl.StartsWith("http"))
                {
                    try
                    {
                        var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                        if (imageBytes != null && imageBytes.Length > 0)
                        {
                            var cardImage = new CardImage
                            {
                                ImageData = imageBytes,
                                ImagePath = imageUrl,
                                MimeType = "image/jpeg"
                            };
                            cardImageId = await _cardRepository.AddImageAsync(cardImage);
                        }
                    }
                    catch { /* Fallback to existing image if download fails */ }
                }

                if (existingCard != null)
                {
                    // 3. Update existing card details
                    existingCard.Name = cleanedName;
                    existingCard.PackName = packName;
                    existingCard.Rarity = rarity;
                    existingCard.Type = cardType;
                    if (cardImageId > 0)
                    {
                        existingCard.CardImageId = cardImageId;
                    }

                    await _cardRepository.UpdateAsync(existingCard);
                }
                else
                {
                    // 4. Insert new card if it doesn't exist
                    var newCard = new Card
                    {
                        UniqueID = uniqueId,
                        Name = cleanedName,
                        PackName = packName,
                        Rarity = rarity,
                        Type = cardType,
                        CardImageId = cardImageId,
                        DateAdded = DateTime.Now
                    };

                    await _cardRepository.AddAsync(newCard);
                }

                processedCount++;
            }

            return processedCount;
        }

        public async Task<int> AddCardAsync(Card card)
        {
            if (string.IsNullOrWhiteSpace(card.UniqueID))
                throw new ArgumentException("UniqueID is required");

            if (string.IsNullOrWhiteSpace(card.Name))
                throw new ArgumentException("Card name is required");

            var existingCard = await _cardRepository.GetByUniqueIdAsync(card.UniqueID);
            if (existingCard != null)
                throw new InvalidOperationException($"Card with UniqueID '{card.UniqueID}' already exists");

            return await _cardRepository.AddAsync(card);
        }

        public async Task<bool> UpdateCardAsync(Card card)
        {
            var existingCard = await _cardRepository.GetByIdAsync(card.Id);
            if (existingCard == null)
                return false;

            return await _cardRepository.UpdateAsync(card);
        }

        public async Task<bool> DeleteCardAsync(int id)
        {
            return await _cardRepository.DeleteAsync(id);
        }

        public async Task<CardImage> AddCardImageAsync(byte[] imageData, string imagePath)
        {
            if (imageData == null || imageData.Length == 0)
                throw new ArgumentException("Image data is required");

            var image = new CardImage
            {
                ImageData = imageData,
                ImagePath = imagePath,
                MimeType = "image/png"
            };

            var imageId = await _cardRepository.AddImageAsync(image);
            image.Id = imageId;
            return image;
        }

        public async Task<CardImage> GetCardImageAsync(int imageId)
        {
            return await _cardRepository.GetImageByIdAsync(imageId);
        }

        public async Task<bool> AssignImageToCardAsync(int cardId, int imageId)
        {
            var card = await _cardRepository.GetByIdAsync(cardId);
            if (card == null)
                return false;

            card.CardImageId = imageId;
            return await _cardRepository.UpdateAsync(card);
        }
    }
}
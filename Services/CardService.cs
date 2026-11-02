using PokemonBinder.Models;
using PokemonBinder.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
using PokemonBinder.Models;
using PokemonBinder.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonBinder.Services
{
    public class BinderService
    {
        private readonly IBinderRepository _binderRepository;

        public BinderService(IBinderRepository binderRepository)
        {
            _binderRepository = binderRepository;
        }

        public async Task<Binder> GetBinderByIdAsync(int id)
        {
            return await _binderRepository.GetByIdAsync(id);
        }

        public async Task<List<Binder>> GetAllBindersAsync()
        {
            return await _binderRepository.GetAllAsync();
        }

        public async Task<int> CreateBinderAsync(string name, string description,
            byte[] frontCover, string CoverColor)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Binder name is required");

            var binder = new Binder
            {
                Name = name,
                Description = description,
                FrontCoverImage = frontCover,                                
                CoverColor = CoverColor,
                CreatedDate = DateTime.Now,
                LastModified = DateTime.Now
            };

            return await _binderRepository.AddAsync(binder);
        }

        public async Task<bool> UpdateBinderAsync(Binder binder)
        {
            var existingBinder = await _binderRepository.GetByIdAsync(binder.Id);
            if (existingBinder == null)
                return false;

            binder.LastModified = DateTime.Now;
            return await _binderRepository.UpdateAsync(binder);
        }

        public async Task<bool> DeleteBinderAsync(int id)
        {
            return await _binderRepository.DeleteAsync(id);
        }

        public async Task<int> AddPageToBinderAsync(int binderId, GridSize gridSize)
        {
            var binder = await _binderRepository.GetByIdAsync(binderId);
            if (binder == null)
                throw new InvalidOperationException($"Binder with ID {binderId} not found");

            var pageNumber = binder.Pages.Count + 1;
            var page = new BinderPage
            {
                BinderId = binderId,
                PageNumber = pageNumber,
                GridSize = gridSize
            };

            var pageId = await _binderRepository.AddPageAsync(page);

            // Create slots for the page
            int slotCount = (int)gridSize;
            for (int i = 0; i < slotCount; i++)
            {
                var slot = new BinderSlot
                {
                    BinderPageId = pageId,
                    SlotPosition = i,
                    IsPlaceholder = true
                };
                await _binderRepository.AddSlotAsync(slot);
            }

            return pageId;
        }

        public async Task<bool> AddCardToSlotAsync(int slotId, int cardId)
        {
            var slot = new BinderSlot { Id = slotId, CardId = cardId, IsPlaceholder = false };
            return await _binderRepository.UpdateSlotAsync(slot);
        }

        public async Task<bool> RemoveCardFromSlotAsync(int slotId)
        {
            var slot = new BinderSlot { Id = slotId, CardId = null, IsPlaceholder = true };
            return await _binderRepository.UpdateSlotAsync(slot);
        }

        public async Task<BinderPage> GetPageAsync(int pageId)
        {
            return await _binderRepository.GetPageByIdAsync(pageId);
        }

        public async Task<List<BinderPage>> GetBinderPagesAsync(int binderId)
        {
            return await _binderRepository.GetPagesByBinderIdAsync(binderId);
        }
    }
}
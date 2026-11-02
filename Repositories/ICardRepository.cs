using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonBinder.Models;

namespace PokemonBinder.Repositories
{
    public interface ICardRepository
    {
        Task<Card> GetByIdAsync(int id);
        Task<Card> GetByUniqueIdAsync(string uniqueId);
        Task<List<Card>> GetAllAsync();
        Task<List<Card>> SearchAsync(string name, string packName = null);
        Task<int> AddAsync(Card card);
        Task<bool> UpdateAsync(Card card);
        Task<bool> DeleteAsync(int id);
        Task<int> AddImageAsync(CardImage image);
        Task<CardImage> GetImageByIdAsync(int id);
    }
}

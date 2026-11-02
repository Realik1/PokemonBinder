using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonBinder.Models;

namespace PokemonBinder.Repositories
{
    public interface IBinderRepository
    {
        Task<Binder> GetByIdAsync(int id);
        Task<List<Binder>> GetAllAsync();
        Task<int> AddAsync(Binder binder);
        Task<bool> UpdateAsync(Binder binder);
        Task<bool> DeleteAsync(int id);
        Task<int> AddPageAsync(BinderPage page);
        Task<List<BinderPage>> GetPagesByBinderIdAsync(int binderId);
        Task<BinderPage> GetPageByIdAsync(int pageId);
        Task<int> AddSlotAsync(BinderSlot slot);
        Task<List<BinderSlot>> GetSlotsByPageIdAsync(int pageId);
        Task<bool> UpdateSlotAsync(BinderSlot slot);
    }
}

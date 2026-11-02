using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonBinder.Models
{
    public class BinderPage
    {
        public int Id { get; set; }
        public int BinderId { get; set; }
        public int PageNumber { get; set; }
        public GridSize GridSize { get; set; }
        public List<BinderSlot> Slots { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonBinder.Models
{
    public class Card
    {
        public int Id { get; set; }
        public string UniqueID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string PackName { get; set; } = string.Empty;
        public int PokedexNo { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty; 
        public string Stage { get; set; } = string.Empty;      
        public int HP { get; set; } 
        public int CardImageId { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
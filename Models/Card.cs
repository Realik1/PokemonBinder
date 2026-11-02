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
        public string UniqueID { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string PackName { get; set; }
        public int PokedexNo { get; set; }
        public string Type { get; set; }
        public bool IsEX { get; set; }
        public string Rarity { get; set; }
        public bool IsFullArt { get; set; }
        public bool IsHolo { get; set; }
        public string HoloType { get; set; }
        public int CardImageId { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
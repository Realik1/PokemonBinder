using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonBinder.Models
{
    public class CardImage
    {
        public int Id { get; set; }
        public byte[] ImageData { get; set; }
        public string ImagePath { get; set; }
        public string MimeType { get; set; }
    }
}

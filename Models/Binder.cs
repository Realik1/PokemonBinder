using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonBinder.Models
{
    public class Binder
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] FrontCoverImage { get; set; }
        public byte[] BackCoverImage { get; set; }
        public string CoverColor { get; set; } = "#1e293b";
        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }
        public List<BinderPage> Pages { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Categoria : NamedEntity
    {
        public string Descripcion { get; set; }
        public List<Prenda> Prendas { get; set; }

        public Categoria()
        {
            Prendas = new List<Prenda>();
        }
    }
}

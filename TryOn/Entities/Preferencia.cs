using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Preferencia : BaseEntity
    {
        public string Categoria { get; set; }
        public string Valor { get; set; }
        public int Prioridad { get; set; }
    }

}

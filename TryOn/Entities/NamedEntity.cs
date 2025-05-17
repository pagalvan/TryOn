using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public abstract class NamedEntity : BaseEntity
    {
        public string Nombre { get; set; }
    }

}

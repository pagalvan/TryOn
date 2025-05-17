using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Administrador : Usuario
    {
        public string Cargo { get; set; }
        public string Departamento { get; set; }
    }

}

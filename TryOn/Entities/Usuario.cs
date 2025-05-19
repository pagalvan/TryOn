using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Usuario : Persona
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public DateTime FechaRegistro { get; set; }

        public Boolean Activo { get; set; }
    }
}                                                           

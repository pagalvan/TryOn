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
        public string Contrasena { get; set; }
        public override string NombreCompleto()
        {
            return $"{Nombre} {Apellido}";
        }
    }

}

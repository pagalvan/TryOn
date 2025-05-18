using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Cliente : Persona
    {
        public string Direccion { get; set; }
        public List<Medida> Medidas { get; set; }
        public List<Preferencia> Preferencias { get; set; }

        public Cliente()
        {
            Medidas = new List<Medida>();
            Preferencias = new List<Preferencia>();
        }
    }
}

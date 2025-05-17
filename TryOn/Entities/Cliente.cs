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
        public List<Medida> Medidas { get; set; } = new List<Medida>();
        public List<Preferencia> Preferencias { get; set; } = new List<Preferencia>();
        public override string NombreCompleto()
        {
            return $"{Nombre} {Apellido}";
        }
    }
}

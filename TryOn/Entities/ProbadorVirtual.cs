using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class ProbadorVirtual
    {
        public Cliente Cliente { get; set; }
        public Prenda PrendaSeleccionada { get; set; }
        public List<PruebaPrenda> HistorialPruebas { get; set; } = new List<PruebaPrenda>();
    }

    public class PruebaPrenda : BaseEntity
    {
        public Cliente Cliente { get; set; }
        public Prenda Prenda { get; set; }
        public DateTime Fecha { get; set; }
        public string Comentarios { get; set; }
    }


}

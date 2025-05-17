using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Promocion : BaseEntity
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string CodigoPromo { get; set; }
        public double PorcentajeDescuento { get; set; }
        public double MontoDescuento { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activo { get; set; } = true;
        public List<Categoria> CategoriasAplicables { get; set; } = new List<Categoria>();
        public List<Prenda> PrendasAplicables { get; set; } = new List<Prenda>();
    }
}

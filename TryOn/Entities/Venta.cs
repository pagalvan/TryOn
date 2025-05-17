using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Venta : BaseEntity
    {
        public Pedido Pedido { get; set; }
        public DateTime FechaVenta { get; set; }
        public string MetodoPago { get; set; }
        public double MontoTotal { get; set; }
    }

}

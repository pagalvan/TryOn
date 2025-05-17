using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Pedido : BaseEntity
    {
        public Cliente Cliente { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public List<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
        public double Total => Detalles.Sum(d => d.Subtotal);
    }
}

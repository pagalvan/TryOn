using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class DetallePedido : BaseEntity
    {
        public Pedido Pedido { get; set; }
        public Prenda Prenda { get; set; }
        public int Cantidad { get; set; }
        public double Subtotal => Cantidad * (Prenda?.Precio ?? 0);
    }

}

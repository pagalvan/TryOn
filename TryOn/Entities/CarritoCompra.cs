using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class CarritoCompra : BaseEntity
    {
        public Cliente Cliente { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<ItemCarrito> Items { get; set; } = new List<ItemCarrito>();
        public bool Activo { get; set; } = true;

        public double Total => Items.Sum(i => i.Subtotal);
    }

    public class ItemCarrito : BaseEntity
    {
        public CarritoCompra Carrito { get; set; }
        public Prenda Prenda { get; set; }
        public int Cantidad { get; set; }
        public double Subtotal => Cantidad * (Prenda?.Precio ?? 0);
    }
}

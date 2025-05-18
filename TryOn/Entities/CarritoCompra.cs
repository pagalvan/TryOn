using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class CarritoCompra
    {
        public int Id { get; set; }
        public Cliente Cliente { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
        public List<ItemCarrito> Items { get; set; }

        public CarritoCompra()
        {
            Items = new List<ItemCarrito>();
            FechaCreacion = DateTime.Now;
            Activo = true;
        }

        public double Total
        {
            get
            {
                return Items.Sum(i => i.Subtotal);
            }
        }

        public int CantidadItems
        {
            get
            {
                return Items.Sum(i => i.Cantidad);
            }
        }

        public void AgregarItem(Prenda prenda, int cantidad)
        {
            var itemExistente = Items.FirstOrDefault(i => i.Prenda.Id == prenda.Id);

            if (itemExistente != null)
            {
                itemExistente.Cantidad += cantidad;
            }
            else
            {
                Items.Add(new ItemCarrito
                {
                    Prenda = prenda,
                    Cantidad = cantidad
                });
            }
        }

        public void EliminarItem(int prendaId)
        {
            var item = Items.FirstOrDefault(i => i.Prenda.Id == prendaId);
            if (item != null)
            {
                Items.Remove(item);
            }
        }

        public void ActualizarCantidad(int prendaId, int nuevaCantidad)
        {
            var item = Items.FirstOrDefault(i => i.Prenda.Id == prendaId);
            if (item != null)
            {
                item.Cantidad = nuevaCantidad;
            }
        }

        public void VaciarCarrito()
        {
            Items.Clear();
        }
    }

    public class ItemCarrito : BaseEntity
    {
        public CarritoCompra Carrito { get; set; }
        public Prenda Prenda { get; set; }
        public int Cantidad { get; set; }
        public DateTime FechaAgregado { get; set; }
        public ItemCarrito()
        {
            FechaAgregado = DateTime.Now;
            Cantidad = 1;
        }

        public double Subtotal
        {
            get
            {
                return Prenda.PrecioFinal * Cantidad;
            }
        }
    }
}

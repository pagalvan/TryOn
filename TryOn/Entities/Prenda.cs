using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Entities
{
    public class Prenda : BaseEntity
    {
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public string Talla { get; set; }
        public string Color { get; set; }
        public double Precio { get; set; }
        public double? PrecioDescuento { get; set; }
        public int Stock { get; set; }
        public string Imagen { get; set; }
        public string Modelo3D { get; set; }
        public string Descripcion { get; set; }
        public Categoria Categoria { get; set; }
        public bool Destacado { get; set; }
        public bool Activo { get; set; }
        public List<string> ImagenesAdicionales { get; set; }

        public int Cantidad { get; set; }
        public string Ubicacion { get; set; }

        public Prenda()
        {
            ImagenesAdicionales = new List<string>();
            Activo = true;
            Destacado = false;
        }

        public double PrecioFinal
        {
            get
            {
                return PrecioDescuento.HasValue ? PrecioDescuento.Value : Precio;
            }
        }

        public bool TieneDescuento
        {
            get
            {
                return PrecioDescuento.HasValue && PrecioDescuento.Value < Precio;
            }
        }

        public double PorcentajeDescuento
        {
            get
            {
                if (TieneDescuento)
                {
                    return Math.Round((1 - (PrecioDescuento.Value / Precio)) * 100, 2);
                }
                return 0;
            }
        }
    }
}

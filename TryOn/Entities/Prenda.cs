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
        public double PrecioDescuento { get; set; }
        public int Stock { get; set; }
        public string Imagen { get; set; }
        public List<string> ImagenesAdicionales { get; set; } = new();
        public string Modelo3D { get; set; }
        public string Descripcion { get; set; }
        public Categoria Categoria { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Destacado { get; set; }
        public bool Activo { get; set; } = true;
    } 
}

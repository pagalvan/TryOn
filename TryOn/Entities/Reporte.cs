using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Reporte : BaseEntity
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Tipo { get; set; } // Ventas, Inventario, Clientes, etc.
        public string Formato { get; set; } // PDF, Excel, etc.
        public string DatosJSON { get; set; } // Datos del reporte en formato JSON
        public Administrador CreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}

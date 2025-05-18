using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Reporte
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Tipo { get; set; }
        public string Formato { get; set; }
        public string DatosJSON { get; set; }
        public Administrador CreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; }

        public Reporte()
        {
            FechaCreacion = DateTime.Now;
            FechaInicio = DateTime.Now.AddMonths(-1);
            FechaFin = DateTime.Now;
            Formato = "JSON";
        }
    }
}
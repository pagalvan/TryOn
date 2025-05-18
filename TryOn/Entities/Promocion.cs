using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Promocion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string CodigoPromo { get; set; }
        public double PorcentajeDescuento { get; set; }
        public double? MontoDescuento { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activo { get; set; }
        public List<Categoria> CategoriasAplicables { get; set; }
        public List<Prenda> PrendasAplicables { get; set; }

        public Promocion()
        {
            CategoriasAplicables = new List<Categoria>();
            PrendasAplicables = new List<Prenda>();
            Activo = true;
            FechaInicio = DateTime.Now;
            FechaFin = DateTime.Now.AddMonths(1);
        }

        public bool EstaVigente
        {
            get
            {
                DateTime ahora = DateTime.Now;
                return Activo && ahora >= FechaInicio && ahora <= FechaFin;
            }
        }

        public double CalcularDescuento(double montoOriginal)
        {
            if (MontoDescuento.HasValue)
            {
                return Math.Min(montoOriginal, MontoDescuento.Value);
            }
            else
            {
                return montoOriginal * (PorcentajeDescuento / 100);
            }
        }

        public bool AplicaACategoria(int categoriaId)
        {
            return CategoriasAplicables.Exists(c => c.Id == categoriaId);
        }

        public bool AplicaAPrenda(int prendaId)
        {
            return PrendasAplicables.Exists(p => p.Id == prendaId);
        }
    }
}

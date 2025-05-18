using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace BLL
{
    public interface IService<T>
    {
        string Guardar(T entidad);
        List<T> Consultar();
        T BuscarPorId(int id);
        string Modificar(T entidad);
        string Eliminar(int id);
    }
}

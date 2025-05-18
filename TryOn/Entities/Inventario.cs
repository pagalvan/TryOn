using Entities;
using System;
using System.Collections.Generic;

public class Inventario
{
    public int Id { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public List<Prenda> Prendas { get; set; } = new List<Prenda>();
}

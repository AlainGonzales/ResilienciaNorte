using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResilienciaNorte.Domain
{
    // Stock de un bien dentro de un almacén concreto (clave compuesta AlmacenId + RecursoId).
    [Table("ExistenciasAlmacen")]
    public class ExistenciaAlmacen
    {
        public int AlmacenId { get; set; }
        public Almacen? Almacen { get; set; }

        public int RecursoId { get; set; }
        public RecursoAlmacen? Recurso { get; set; }

        [Range(0, 1000000)]
        public int StockDisponible { get; set; } = 0;

        [Range(0, 100000)]
        public int StockMinimo { get; set; } = 10;

        public bool EsStockCritico() => StockDisponible <= StockMinimo;

        public void DescontarStock(int cantidad)
        {
            if (cantidad > StockDisponible)
                throw new System.InvalidOperationException($"Stock insuficiente para {Recurso?.Nombre}.");
            StockDisponible -= cantidad;
        }

        public void IncrementarStock(int cantidad)
        {
            StockDisponible += cantidad;
        }
    }
}

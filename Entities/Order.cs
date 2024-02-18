using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Order
    {
        public int Id { get; set; }

        public int UserID { get; set; }

        public string ProductName { get; set; }

        public int ProductId { get; set; }
        [Column(TypeName =("Decimal(18,2)"))]

        public decimal ProductPrice { get; set; }

        public bool Status { get; set; }
        [Column(TypeName = ("Decimal(18,2)"))]
        public decimal TotalPrice { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Visualization.Models
{
    public class Infection
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public int RegionId { get; set; }
        public Region Region { get; set; }
        public int Infected { get; set; }
        public int Recovered { get; set; }
        public int Deaths { get; set; }
    }
}

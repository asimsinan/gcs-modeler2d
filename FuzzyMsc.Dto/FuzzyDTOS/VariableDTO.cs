using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyMsc.Dto.FuzzyDTOS
{
    public class VariableDTO
    {
        public string Name { get; set; }
        public string VisibleName { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
    }
}

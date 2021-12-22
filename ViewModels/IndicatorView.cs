using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models;

namespace ktradesystem.ViewModels
{
    class IndicatorView //описывает объект Indicator в виде, удобном для представления
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ParameterTemplate> ParameterTemplates { get; set; }
        public string CalculateText { get; set; }
    }
}

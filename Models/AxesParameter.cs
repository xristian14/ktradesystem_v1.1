using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    class AxesParameter //ось параметра, определяет оси двумерной плоскости для поиска топ модели с соседями
    {
        IndicatorParameterTemplate IndicatorParameterTemplate { get; set; }
        AlgorithmParameter AlgorithmParameter { get; set; }
    }
}

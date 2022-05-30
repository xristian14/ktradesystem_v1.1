using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    [Serializable]
    //класс содержит комбинацию значений параметров индикатора алгоритма и название файла с сериализованным объектом со значениями данного индикатора алгоритма
    public class AlgorithmIndicatorCatalogElement
    {
        public List<AlgorithmParameterValue> AlgorithmParameterValues { get; set; }
        public string FileName { get; set; }
    }
}

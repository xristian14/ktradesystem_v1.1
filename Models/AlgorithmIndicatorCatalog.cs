using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    //класс представляет собой каталог с комбинацией параметров индикатора алгоритма и пути к файлу со значениями данного индикатора алгоритма
    public class AlgorithmIndicatorCatalog
    {
        public AlgorithmIndicator AlgorithmIndicator { get; set; }
        public string AlgorithmIndicatorFolderName { get; set; } //имя папки с значениями индикатора алгоритма
        public List<AlgorithmIndicatorCatalogElement> AlgorithmIndicatorCatalogElements { get; set; }
    }
}

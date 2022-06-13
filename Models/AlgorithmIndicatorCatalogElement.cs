using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    //класс содержит комбинацию значений параметров индикатора алгоритма и название файла с сериализованным объектом со значениями данного индикатора алгоритма
    [Serializable]
    public class AlgorithmIndicatorCatalogElement
    {
        public List<AlgorithmParameterValue> AlgorithmParameterValues { get; set; }
        public string FileName { get; set; }
        [NonSerialized]
        public AlgorithmIndicatorValues AlgorithmIndicatorValues; //значения индикатора алгоритма
        private readonly object locker = new object();
        private bool _isComplete { get; set; } //завершен ли рассчет значений индикатора
        public bool IsComplete //реализация потокобезопасного получения и установки свойства
        {
            get
            {
                lock (locker)
                {
                    return _isComplete;
                }
            }
            set
            {
                lock (locker)
                {
                    _isComplete = value;
                }
            }
        }
    }
}

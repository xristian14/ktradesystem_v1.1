﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;
using System.Text.Json.Serialization;

namespace ktradesystem.Models
{
    [Serializable]
    public class DataSourceCandles //класс содержит источник данных, и массив массивов свечек которые содержат данные файлов
    {
        public DataSource DataSource { get; set; }
        public Candle[][] Candles { get; set; } //массив со свечками на каждый файл источника данных
        public List<int>[] GapIndexes { get; set; } //индексы свечек, которые считаются гэпами, для каждого файла источника данных. Массив на каждый файл, список с индексами свечек в файле
        [NonSerialized]
        public AlgorithmIndicatorValues[] AlgorithmIndicatorsValues; //массив со значениями индикаторов для отображения на графике
        public AlgorithmIndicatorCatalog[] AlgorithmIndicatorCatalogs { get; set; } //массив с каталогами индикаторов алгоритмов. Каталог содержит индикатор алгоритма и список с: комбинацией значений параметров индикатора алгоритма и название файла со значениями данного индикатора
        public double PerfectProfit { get; set; } //идеальная прибыль. Сумма разности цен закрытия всех последовательных по датам свечек (при переходе на следующий файл доходит до даты которая позже текущей, а разница между свечками разных файлов не высчитывается), взятая по модулю, поделенная на шаг цены и умноженная на стоимость пункта цены
    }
}

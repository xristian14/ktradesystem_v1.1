using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;
using System.Diagnostics;
using System.Reflection;
using ktradesystem.CommunicationChannel;
using System.IO;
using System.Globalization;
using System.Text.Json.Serialization;

namespace ktradesystem.Models
{
    [Serializable]
    public class Testing
    {
        public Algorithm Algorithm { get; set; }
        public List<DataSourceGroup> DataSourceGroups { get; set; }
        public TopModelCriteria TopModelCriteria { get; set; } //критерии оценки топ-модели
        public bool IsConsiderNeighbours { get; set; } //оценивать топ-модель с учетом соседей
        public double SizeNeighboursGroupPercent { get; set; } //размер группы соседних тестов
        public bool IsForwardTesting { get; set; } //проводить ли форвардное тестирование
        public bool IsForwardDepositTrading { get; set; } //добавить ли для форвардного тестирования торговлю депозитом
        public double ForwardDeposit { get; set; } //размер депозита форвардного тестирования
        public Currency DefaultCurrency { get; set; } //валюта по умолчанию
        public DateTimeDuration DurationOptimizationTests { get; set; } //длительность оптимизационных тестов
        public DateTimeDuration OptimizationTestSpacing { get; set; } //временной промежуток между оптимизационными тестами
        public DateTimeDuration DurationForwardTest { get; set; } //длительность форвардного тестирования
        public List<TestBatch> TestBatches { get; set; } //тестовые связки (серия оптимизационных тестов за период + форвардный тест)
        [NonSerialized]
        public dynamic[] CompiledIndicators; //объекты, содержащие метод, выполняющий расчет индикатора
        [NonSerialized]
        public dynamic CompiledAlgorithm; //объект, содержащий метод, вычисляющий работу алгоритма
        [NonSerialized]
        public dynamic[] CompiledEvaluationCriterias; //объекты, содержащие метод, выполняющий расчет критерия оценки тестового прогона
        [NonSerialized]
        public List<DataSourceCandles> DataSourcesCandles; //список с массивами свечек (для файлов) для источников данных (от сюда же будут браться данные для отображения графиков)
        public int TopModelEvaluationCriteriaIndex { get; set; } //индекс критерия оценки топ-модели
        public List<int>[] AlgorithmParametersAllIntValues { get; set; }
        public List<double>[] AlgorithmParametersAllDoubleValues { get; set; }
        public DateTime DateTimeSimulationEnding { get; set; } //дата и время завершения выполнения тестирования
        public TimeSpan TestingDuration { get; set; } //длительность тестирования
        public string TestingName { get; set; } //название результата тестирования
    }
}

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

namespace ktradesystem.Models
{
    //класс проводит компиляцию индикаторов, алгоритма и критериев оценки, так же проводит тестирование и вычисляет значение индикаторов для отображения их на графике
    class ModelSimulation : ModelBase
    {
        private static ModelSimulation _instance;

        public static ModelSimulation getInstance()
        {
            if (_instance == null)
            {
                _instance = new ModelSimulation();
            }
            return _instance;
        }

        private ModelSimulation()
        {
            _mainCommunicationChannel = MainCommunicationChannel.getInstance();
            _modelData = ModelData.getInstance();
        }

        private MainCommunicationChannel _mainCommunicationChannel;
        private ModelData _modelData;
        private ModelTesting _modelTesting;
        public ModelTesting ModelTesting
        {
            get
            {
                if(_modelTesting == null)
                {
                    _modelTesting = ModelTesting.getInstance(); //реализовано таким образом, т.к. объекты ссылаюстя друг на друга и идет бесконечный цикл инициализации
                }
                return _modelTesting;
            }
        }

        public dynamic AlgorithmIndicatorCompile(AlgorithmIndicator algorithmIndicator) //возвращает скомпилированный объект индикатора алгоритма, и null в случае ошибки компиляции
        {
            dynamic compiledIndicator = null;
            string variablesParameters = ""; //инициализация и присвоение значений переменным, в которых хранятся значения параметров индикатора
            int currentIndicatorParameterIndex = -1; //номер параметра для текущего индикатора (это число используется как индекс в массиве параметров который принимает скомпилированный индикатор)
            foreach (IndicatorParameterRange indicatorParameterRange in algorithmIndicator.IndicatorParameterRanges)
            {
                currentIndicatorParameterIndex++;
                variablesParameters += indicatorParameterRange.IndicatorParameterTemplate.ParameterValueType.Id == 1 ? "int " : "double ";
                variablesParameters += "Parameter_" + indicatorParameterRange.IndicatorParameterTemplate.Name;
                variablesParameters += indicatorParameterRange.IndicatorParameterTemplate.ParameterValueType.Id == 1 ? " = indicatorParametersIntValues[" + currentIndicatorParameterIndex + "]; " : " = indicatorParametersDoubleValues[" + currentIndicatorParameterIndex + "]; ";
            }

            //добавляем в текст скрипта приведение к double переменных и чисел в операциях деления и умножения (т.к. при делении типов int на int получится тип int и дробная часть потеряется), а так же возвращаемого значения
            //удаляем дублирующиеся пробелы
            StringBuilder script = ModelFunctions.RemoveDuplicateSpaces(algorithmIndicator.Indicator.Script);
            //добавляем приведение к double правой части операции деления, чтобы результат int/int был с дробной частью
            script.Replace("/", "/(double)");
            //удаляем пробел между Candles и [
            script.Replace("Candles [", "Candles[");
            //определяем для всех обращений к Candles[]: индекс начала ключевого слова Candles[ и индекс закрывающей квардратной скобки, если внутри были еще квадратные скобки, их нужно пропустить и дойти до закрывающей
            string scriptIndicatorString = script.ToString();
            List<int> indexesCandles = new List<int>(); //индексы всех вхождений подстроки "Candles["
            if (scriptIndicatorString.IndexOf("Candles[") != -1)
            {
                indexesCandles.Add(scriptIndicatorString.IndexOf("Candles["));
                while (scriptIndicatorString.IndexOf("Candles[", indexesCandles.Last() + 1) != -1)
                {
                    indexesCandles.Add(scriptIndicatorString.IndexOf("Candles[", indexesCandles.Last() + 1));
                }
            }
            //проходим по всем indexesCandles с конца к началу, и заменяем все закрывающие квадратные скобки которые закрывают Candles[ на круглые, при этом внутренние квадратные скобки будет игнорироваться, и заменена будет только закрывающая Candles[
            for (int k = indexesCandles.Count - 1; k >= 0; k--)
            {
                int countOpen = 1; //количество найденных открывающих скобок на текущий момент
                int countClose = 0; //количество найденных закрывающих скобок на текущий момент
                int currentIndex = indexesCandles[k] + 8; //индекс текущего символа
                                                          //пока количество открывающи не будет равно количеству закрывающих, или пока не превысим длину строки
                while (countOpen != countClose && currentIndex < scriptIndicatorString.Length)
                {
                    if (scriptIndicatorString[currentIndex] == '[')
                    {
                        countOpen++;
                    }
                    if (scriptIndicatorString[currentIndex] == ']')
                    {
                        countClose++;
                    }
                    currentIndex++;
                }
                if (countOpen != countClose) //не найдена закрывающия скобка, выводим сообщение
                {
                    DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Ошибка в скрипте индикатора " + algorithmIndicator.Indicator.Name + ": отсутствует закрывающая скобка \"]\" при обращении к массиву Candles."); }));
                }
                //заменяем закрывающую квадратную скобку на круглую
                scriptIndicatorString = scriptIndicatorString.Remove(currentIndex - 1, 1);
                scriptIndicatorString = scriptIndicatorString.Insert(currentIndex - 1, ")");
            }

            script = new StringBuilder(scriptIndicatorString);
            //заменяем все Canldes[ на GetCandle(
            script.Replace("Candles[", "GetCandle(");


            Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
            System.CodeDom.Compiler.CompilerParameters param = new System.CodeDom.Compiler.CompilerParameters();
            param.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            param.GenerateExecutable = false;
            param.GenerateInMemory = true;

            var compiled = provider.CompileAssemblyFromSource(param, new string[]
            {
                    @"
                    using System;
                    using ktradesystem.Models;
                    public class CompiledIndicator_" + algorithmIndicator.Indicator.Name + "_" + algorithmIndicator.Ending +
                    @"{
                        public int MaxOverIndex;
                        public Candle[] Candles;
                        public int CurrentCandleIndex;
                        public IndicatorCalculateResult Calculate(Candle[] inputCandles, int currentCandleIndex, int[] indicatorParametersIntValues, double[] indicatorParametersDoubleValues)
                        {
                            " + variablesParameters + @"
                            MaxOverIndex = 0;
                            Candles = inputCandles;
                            CurrentCandleIndex = currentCandleIndex;
                            double Indicator = 0;
                            " + script +
                            @"return new IndicatorCalculateResult { Value = Indicator, OverIndex = MaxOverIndex };
                        }
                        public Candle GetCandle(int userIndex)
                        {
                            int realIndex = CurrentCandleIndex - userIndex;
                            Candle result;
                            if(realIndex < 0)
                            {
                                MaxOverIndex = - realIndex > MaxOverIndex? - realIndex: MaxOverIndex;
                                result = new Candle { DateTime = Candles[0].DateTime, O = Candles[0].O, H = Candles[0].H, L = Candles[0].L, C = Candles[0].C, V = Candles[0].V };
                            }
                            else
                            {
                                result = new Candle { DateTime = Candles[realIndex].DateTime, O = Candles[realIndex].O, H = Candles[realIndex].H, L = Candles[realIndex].L, C = Candles[realIndex].C, V = Candles[realIndex].V };
                            }
                            return result;
                        }
                        public CompiledIndicator_" + algorithmIndicator.Indicator.Name + "_" + algorithmIndicator.Ending + @" Clone()
                        {
                            return new CompiledIndicator_" + algorithmIndicator.Indicator.Name + "_" + algorithmIndicator.Ending + @"();
                        }
                    }" //MaxOverIndex - максимальное превышение индекса массива со свечками; GetCandle() создает и возвращает новый объект Candle для того чтобы в скрипте нельзя было переопределить значения свечки
            });
            if (compiled.Errors.Count == 0)
            {
                compiledIndicator = compiled.CompiledAssembly.CreateInstance("CompiledIndicator_" + algorithmIndicator.Indicator.Name + "_" + algorithmIndicator.Ending);
            }
            else
            {
                //отправляем пользователю сообщения об ошибке
                for (int r = 0; r < compiled.Errors.Count; r++)
                {
                    DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Ошибка при компиляции индикатора " + algorithmIndicator.Indicator.Name + ": " + compiled.Errors[0].ErrorText); }));
                }
            }

            return compiledIndicator;
        }

        public dynamic AlgorithmCompile(Algorithm algorithm) //возвращает скомпилированный объект алгоритма, и null в случае ошибки компиляци
        {
            dynamic resultCompiledAlgorithm = null;
            string dataSourcesForCalculateVariables = ""; //объявление переменных для класса, в которых будут храниться DataSourcesForCalculate
            List<string> dsVariablesNames = new List<string>(); //список с названиями переменных dataSourceForCalculate
            for (int k = 0; k < algorithm.DataSourceTemplates.Count; k++)
            {
                dsVariablesNames.Add("Datasource_" + algorithm.DataSourceTemplates[k].Name);
                dataSourcesForCalculateVariables += "public DataSourceForCalculate " + dsVariablesNames[k] + "; ";
            }

            string algorithmVariables = ""; //инициализация и присвоение значений переменным, с которыми будет работать пользователь
            //присваиваем переменным источников данных значения
            for (int k = 0; k < algorithm.DataSourceTemplates.Count; k++)
            {
                algorithmVariables += dsVariablesNames[k] + " = dataSourcesForCalculate[" + k + "]; ";
            }
            //формируем параметры индикаторов для источников данных
            for (int i = 0; i < algorithm.DataSourceTemplates.Count; i++)
            {
                for (int k = 0; k < algorithm.AlgorithmIndicators.Count; k++)
                {
                    algorithmVariables += "double " + dsVariablesNames[i] + "_Indicator_" + algorithm.AlgorithmIndicators[k].Indicator.Name + "_" + algorithm.AlgorithmIndicators[k].Ending + " = dataSourcesForCalculate[" + i + "].IndicatorsValues[" + k + "]; ";
                }
            }
            //формируем параметры алгоритма
            for (int k = 0; k < algorithm.AlgorithmParameters.Count; k++)
            {
                algorithmVariables += algorithm.AlgorithmParameters[k].ParameterValueType.Id == 1 ? "int " : "double ";
                algorithmVariables += "Parameter_" + algorithm.AlgorithmParameters[k].Name + " = ";
                algorithmVariables += algorithm.AlgorithmParameters[k].ParameterValueType.Id == 1 ? "algorithmParametersIntValues[" + k + "]; " : "algorithmParametersDoubleValues[" + k + "]; ";
            }
            //удаляем дублирующиеся пробелы
            StringBuilder scriptAlgorithm = ModelFunctions.RemoveDuplicateSpaces(algorithm.Script);
            //добавляем приведение к double правой части операции деления, чтобы результат int/int был с дробной частью
            scriptAlgorithm.Replace("/", "/(double)");
            //заменяем все обращения к конкретному индикатору типа: Datasource_maket.Indicator_sma на Datasource_maket_Indicator_sma
            for (int i = 0; i < algorithm.DataSourceTemplates.Count; i++)
            {
                for (int k = 0; k < algorithm.AlgorithmIndicators.Count; k++)
                {
                    scriptAlgorithm.Replace(dsVariablesNames[i] + ".Indicator_" + algorithm.AlgorithmIndicators[k].Indicator.Name + "_" + algorithm.AlgorithmIndicators[k].Ending, dsVariablesNames[i] + "_Indicator_" + algorithm.AlgorithmIndicators[k].Indicator.Name + "_" + algorithm.AlgorithmIndicators[k].Ending);
                }
            }
            //удаляем пробелы до и после открывающейся скобки после ключевого слова на создание заявки
            string[] orderLetters = new string[] { "Order_LimitSell", "Order_LimitBuy", "Order_StopSell", "Order_StopBuy", "Order_MarketSell", "Order_MarketBuy", "Order_StopTakeBuy", "Order_StopTakeSell" }; //слова создания заявок
            foreach (string str in orderLetters)
            {
                scriptAlgorithm.Replace(str + " (", str + "("); //удаляем пробел перед открывающейся скобкой
                scriptAlgorithm.Replace(str + "( ", str + "("); //удаляем пробел после открывающейся скобки
            }
            //заменяем ключевые слова на создание заявок, на функцию по созданию заявки и добавление её в коллекцию заявок orders
            string[] orderCorrectLetters = new string[] { "orders.Add(GetOrder(1, false,", "orders.Add(GetOrder(1, true,", "orders.Add(GetOrder(3, false,", "orders.Add(GetOrder(3, true,", "orders.Add(GetOrder(2, false,", "orders.Add(GetOrder(2, true,", "orders.AddRange(GetStopTake(true,", "orders.AddRange(GetStopTake(false," };
            for (int k = 0; k < orderLetters.Length; k++)
            {
                scriptAlgorithm.Replace(orderLetters[k] + "(", orderCorrectLetters[k]);
            }
            //добавляем закрывающую скобку для добавлений заявок
            List<int> indexesSemicolon = new List<int>(); //список с индексами точек с запятой, следующих за словом добавления заявки
            //находим индексы слов добавления заявок
            bool isSemicolonFind = true; //найдена ли точка с запятой после слова на создание заявки
            string scriptAlgorithmString = scriptAlgorithm.ToString(); //текст скрипта в формате string
            //ищем вхождения всех ключевых слов
            for (int k = 0; k < orderCorrectLetters.Length; k++)
            {
                int indexFindLetter = scriptAlgorithmString.IndexOf(orderCorrectLetters[k]); //индекс найденного слова
                while (indexFindLetter != -1)
                {
                    int indexSemicolon = scriptAlgorithmString.IndexOf(";", indexFindLetter); //индекс первой найденной точки запятой, от индекса слова добавления заявки
                    if (indexSemicolon != -1)
                    {
                        indexesSemicolon.Add(indexSemicolon);
                    }
                    else
                    {
                        isSemicolonFind = false; //указываем что после описания добавления заявки не найден символ точки с запятой
                    }

                    indexFindLetter = scriptAlgorithmString.IndexOf(orderCorrectLetters[k], indexFindLetter + 1); //ищем следующее вхождение данного слова
                }
            }
            if (isSemicolonFind == false)
            {
                //отправляем пользователю сообщения об ошибке
                DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Синтаксическая ошибка в скрипте алгоритма: не найдена \";\" после добавления заявки."); }));
            }
            //сортируем индексы точки с запятой, закрывающей оператор добавления заявки
            indexesSemicolon.Sort();
            //вставляем закрывающую скобку
            for (int k = indexesSemicolon.Count - 1; k >= 0; k--)
            {
                scriptAlgorithm.Insert(indexesSemicolon[k], ")");
            }
            //заменяем для всех источников данных обращение типа: Datasource_maket.Candles[5] на GetCandle(Datasource_maket, 5)
            scriptAlgorithm.Replace("Candles [", "Candles["); //удаляем пробел
            scriptAlgorithmString = scriptAlgorithm.ToString(); //текст скрипта в формате string
            //заменяем все закрывающие скобки "]" при обращении к Candles на круглые ")"
            List<int> algorithmIndexesCandles = new List<int>(); //индексы всех вхождений подстроки "Candles["
            if (scriptAlgorithmString.IndexOf("Candles[") != -1)
            {
                algorithmIndexesCandles.Add(scriptAlgorithmString.IndexOf("Candles["));
                while (scriptAlgorithmString.IndexOf("Candles[", algorithmIndexesCandles.Last() + 1) != -1)
                {
                    algorithmIndexesCandles.Add(scriptAlgorithmString.IndexOf("Candles[", algorithmIndexesCandles.Last() + 1));
                }
            }
            //проходим по всем indexesCandles с конца к началу, и заменяем все закрывающие квадратные скобки которые закрывают Candles[ на круглые, при этом внутренние квадратные скобки будет игнорироваться, и заменена будет только закрывающая Candles[
            for (int k = algorithmIndexesCandles.Count - 1; k >= 0; k--)
            {
                int countOpen = 1; //количество найденных открывающих скобок на текущий момент
                int countClose = 0; //количество найденных закрывающих скобок на текущий момент
                int currentIndex = algorithmIndexesCandles[k] + 8; //индекс текущего символа
                //пока количество открывающи не будет равно количеству закрывающих, или пока не превысим длину строки
                while (countOpen != countClose && currentIndex < scriptAlgorithmString.Length)
                {
                    if (scriptAlgorithmString[currentIndex] == '[')
                    {
                        countOpen++;
                    }
                    if (scriptAlgorithmString[currentIndex] == ']')
                    {
                        countClose++;
                    }
                    currentIndex++;
                }
                if (countOpen != countClose) //не найдена закрывающия скобка, выводим сообщение
                {
                    DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Ошибка в скрипте алгоритма: отсутствует закрывающая скобка \"]\" при обращении к массиву Candles."); }));
                }
                //заменяем закрывающую квадратную скобку на круглую
                scriptAlgorithmString = scriptAlgorithmString.Remove(currentIndex - 1, 1);
                scriptAlgorithmString = scriptAlgorithmString.Insert(currentIndex - 1, ")");
            }
            scriptAlgorithm = new StringBuilder(scriptAlgorithmString);
            //заменяем все обращения типа: Datasource_maket.Candles[ на GetCandle(Datasource_maket, 
            for (int k = 0; k < dsVariablesNames.Count; k++)
            {
                //находим индексы начала обращения к Datasource_maket.Candles[
                scriptAlgorithm.Replace(dsVariablesNames[k] + ".Candles[", "GetCandle(" + dsVariablesNames[k] + ", ");
            }

            Microsoft.CSharp.CSharpCodeProvider providerAlgorithm = new Microsoft.CSharp.CSharpCodeProvider();
            System.CodeDom.Compiler.CompilerParameters paramAlgorithm = new System.CodeDom.Compiler.CompilerParameters();
            paramAlgorithm.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            paramAlgorithm.ReferencedAssemblies.Add("System.dll");
            paramAlgorithm.ReferencedAssemblies.Add("System.Core.dll");
            paramAlgorithm.GenerateExecutable = false;
            paramAlgorithm.GenerateInMemory = true;

            var compiledAlgorithm = providerAlgorithm.CompileAssemblyFromSource(paramAlgorithm, new string[]
            {
                @"
                using System;
                using System.Collections.Generic;
                using System.Collections.ObjectModel;
                using System.Linq;
                using ktradesystem.Models;
                public class CompiledAlgorithm
                {
                    ModelData _modelData;
                    " + dataSourcesForCalculateVariables + @"
                    int MaxOverIndex;
                    public AlgorithmCalculateResult Calculate(AccountForCalculate account, DataSourceForCalculate[] dataSourcesForCalculate, int[] algorithmParametersIntValues, double[] algorithmParametersDoubleValues)
                    {
                        _modelData = ModelData.getInstance();
                        " + algorithmVariables + @"
                        MaxOverIndex = 0;
                        List<Order> orders = new List<Order>();
                        " + scriptAlgorithm +
                        @"return new AlgorithmCalculateResult { Orders = orders, OverIndex = MaxOverIndex };
                    }
                    public Candle GetCandle(DataSourceForCalculate dataSourcesForCalculate, int userIndex)
                    {
                        int realIndex = dataSourcesForCalculate.CurrentCandleIndex - userIndex;
                        Candle result;
                        if(realIndex < 0)
                        {
                            MaxOverIndex = - realIndex > MaxOverIndex? - realIndex: MaxOverIndex;
                            result = new Candle { DateTime = dataSourcesForCalculate.Candles[0].DateTime, O = dataSourcesForCalculate.Candles[0].O, H = dataSourcesForCalculate.Candles[0].H, L = dataSourcesForCalculate.Candles[0].L, C = dataSourcesForCalculate.Candles[0].C, V = dataSourcesForCalculate.Candles[0].V };
                        }
                        else
                        {
                            result = new Candle { DateTime = dataSourcesForCalculate.Candles[realIndex].DateTime, O = dataSourcesForCalculate.Candles[realIndex].O, H = dataSourcesForCalculate.Candles[realIndex].H, L = dataSourcesForCalculate.Candles[realIndex].L, C = dataSourcesForCalculate.Candles[realIndex].C, V = dataSourcesForCalculate.Candles[realIndex].V };
                        }
                        return result;
                    }
                    public List<Order> GetStopTake(bool direction, DataSourceForCalculate dataSourceForCalculate, double stopPrice, double takePrice, decimal count)
                    {
                        Order stopOrder = GetOrder(3, direction, dataSourceForCalculate, stopPrice, count);
                        Order takeOrder = GetOrder(1, direction, dataSourceForCalculate, takePrice, count);
                        stopOrder.LinkedOrder = takeOrder;
                        takeOrder.LinkedOrder = stopOrder;
                        return new List<Order> { stopOrder, takeOrder };
                    }
                    public Order GetOrder(int idTypeOrder, bool direction, DataSourceForCalculate dataSourceForCalculate, double price, decimal count)
                    {
                        Order order = new Order();
                        order.TypeOrder = _modelData.TypeOrders.Where(i => i.Id == idTypeOrder).First();
                        order.Direction = direction;
                        order.DataSource = _modelData.DataSources.Where(j => j.Id == dataSourceForCalculate.idDataSource).First();
                        order.IdDataSource = order.DataSource.Id;
                        order.Price = price;

                        decimal orderCount = count;
                        if(order.DataSource.Instrument.Id != 3)
                        {
                            orderCount = Math.Truncate(orderCount);
                        }
                        order.Count = orderCount;
                        order.StartCount = orderCount;

                        return order;
                    }
                    public CompiledAlgorithm Clone()
                    {
                        return new CompiledAlgorithm();
                    }
                }"
            });
            if (compiledAlgorithm.Errors.Count == 0)
            {
                resultCompiledAlgorithm = compiledAlgorithm.CompiledAssembly.CreateInstance("CompiledAlgorithm");
            }
            else
            {
                //отправляем пользователю сообщения об ошибке
                for (int r = 0; r < compiledAlgorithm.Errors.Count; r++)
                {
                    DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Ошибка при компиляции алгоритма: " + compiledAlgorithm.Errors[0].ErrorText); }));
                }
            }

            return resultCompiledAlgorithm;
        }

        public dynamic EvaluationCriteriaCompile(EvaluationCriteria evaluationCriteria) //возвращает скомпилированный объект критерия оценки, и null в случае ошибки компиляци
        {
            dynamic compiledEvaluationCriteria = null;
            string script = evaluationCriteria.Script;

            Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
            System.CodeDom.Compiler.CompilerParameters param = new System.CodeDom.Compiler.CompilerParameters();
            param.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            param.ReferencedAssemblies.Add("System.dll");
            param.ReferencedAssemblies.Add("System.Core.dll");
            param.GenerateExecutable = false;
            param.GenerateInMemory = true;

            var compiled = provider.CompileAssemblyFromSource(param, new string[]
            {
                    @"
                    using System;
                    using System.Collections.Generic;
                    using System.Collections.ObjectModel;
                    using System.Linq;
                    using ktradesystem.Models;
                    using ktradesystem.Models.Datatables;
                    public class CompiledEvaluationCriteria_" + evaluationCriteria.Id.ToString() +
                    @"{
                        public EvaluationCriteriaValue Calculate(DataSourceCandles[] dataSourcesCandles, TestRun testRun, ObservableCollection<Setting> settings)
                        {
                            double ResultDoubleValue = 0;
                            string ResultStringValue = """";
                            " + script +
                            @"return new EvaluationCriteriaValue { DoubleValue = ResultDoubleValue, StringValue = ResultStringValue };
                        }
                        public CompiledEvaluationCriteria_" + evaluationCriteria.Id.ToString() + @" Clone()
                        {
                            return new CompiledEvaluationCriteria_" + evaluationCriteria.Id.ToString() + @"();
                        }
                    }"
            });

            if (compiled.Errors.Count == 0)
            {
                compiledEvaluationCriteria = compiled.CompiledAssembly.CreateInstance("CompiledEvaluationCriteria_" + evaluationCriteria.Id.ToString());
            }
            else
            {
                //отправляем пользователю сообщения об ошибке
                for (int r = 0; r < compiled.Errors.Count; r++)
                {
                    DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Ошибка при компиляции критерия оценки " + evaluationCriteria.Name + ": " + compiled.Errors[0].ErrorText); }));
                }
            }

            return compiledEvaluationCriteria;
        }

        public List<int[]> CreateCombinations(List<int[]> combination, List<int> indexes) //принимает 2 списка, 1-й - содержит массив с комбинации индексов параметров: {[0,0],[0,1],[1,0],[1,1]}, второй только индексы: {0,1}, функция перебирает все комбинации элементов обоих списков и возвращает новый список в котором индексы 2-го списка добавлены в комбинацию 1-го: {[0,0,0],[0,0,1],[0,1,0]..}
        {
            List<int[]> newCombination = new List<int[]>();
            for (int i = 0; i < combination.Count; i++)
            {
                for (int k = 0; k < indexes.Count; k++)
                {
                    int[] arr = new int[combination[i].Length + 1]; //создаем новый массив с комбинацией индексов параметров, превышающий старый на один элемент
                    for (int n = 0; n < combination[i].Length; n++) //заносим в новый массив все элементы старого массива
                    {
                        arr[n] = combination[i][n];
                    }
                    arr[combination[i].Length] = indexes[k]; //помещаем в последний элемент нового массива индекс из списка индексов
                    newCombination.Add(arr); //добавляем новую созданную комбинацию в список новых комбинаций
                }
            }
            return newCombination;
        }

        public void TestingSimulation(Testing testing) //выполнение тестирования
        {
            testing.TestBatches = new List<TestBatch>();

            //находим индекс критерия оценки топ-модели
            testing.TopModelEvaluationCriteriaIndex = _modelData.EvaluationCriterias.IndexOf(testing.TopModelCriteria.EvaluationCriteria);

            //определяем списки со значениями параметров
            testing.AlgorithmParametersAllIntValues = new List<int>[testing.Algorithm.AlgorithmParameters.Count]; //массив со всеми возможными целочисленными значениями параметров алгоритма
            testing.AlgorithmParametersAllDoubleValues = new List<double>[testing.Algorithm.AlgorithmParameters.Count]; //массив со всеми возможными дробными значениями параметров алгоритма

            //параметры будут передаваться в индикаторы и алгоритм в качестве параметров методов, при описании методов индикатора или алгоритма я укажу тип принимаемого параметра int или double в зависимости от типа в шаблоне параметра, и после проверки типа параметра, решу из какого списка передавать, со значениями double, или со значениями int

            //генерируем все значения параметров алгоритма
            for (int i = 0; i < testing.Algorithm.AlgorithmParameters.Count; i++)
            {
                testing.AlgorithmParametersAllIntValues[i] = new List<int>();
                testing.AlgorithmParametersAllDoubleValues[i] = new List<double>();
                //определяем, какой список формировать, целых или дробных чисел
                bool isIntValueType = (testing.Algorithm.AlgorithmParameters[i].ParameterValueType.Id == 1) ? true : false;
                //определяем шаг
                double step = testing.Algorithm.AlgorithmParameters[i].Step;
                if (testing.Algorithm.AlgorithmParameters[i].IsStepPercent)
                {
                    step = (testing.Algorithm.AlgorithmParameters[i].MaxValue - testing.Algorithm.AlgorithmParameters[i].MinValue) * (testing.Algorithm.AlgorithmParameters[i].Step / 100);
                }

                double currentValue = testing.Algorithm.AlgorithmParameters[i].MinValue; //текущее значение

                if (isIntValueType)
                {
                    testing.AlgorithmParametersAllIntValues[i].Add((int)Math.Round(currentValue));
                }
                else
                {
                    testing.AlgorithmParametersAllDoubleValues[i].Add(currentValue);
                }
                currentValue += step;

                while (currentValue <= testing.Algorithm.AlgorithmParameters[i].MaxValue)
                {
                    if (isIntValueType)
                    {
                        int intCurrentValue = (int)Math.Round(currentValue);
                        if (intCurrentValue != testing.AlgorithmParametersAllIntValues[i].Last()) //если текущее значение отличается от предыдущего, добавляем его в целочисленные значения
                        {
                            testing.AlgorithmParametersAllIntValues[i].Add(intCurrentValue);
                        }
                    }
                    else
                    {
                        testing.AlgorithmParametersAllDoubleValues[i].Add(currentValue);
                    }

                    currentValue += step;
                }
            }

            //формируем список со всеми комбинациями параметров
            List<int[]> allCombinations = new List<int[]>();
            for (int alg = 0; alg < testing.Algorithm.AlgorithmParameters.Count; alg++)
            {
                bool isAlgorithmParameterIntValueType = (testing.Algorithm.AlgorithmParameters[alg].ParameterValueType.Id == 1) ? true : false;

                if (allCombinations.Count == 0) //если комбинации пустые (не было параметров индикаторов), создаем комбинации
                {
                    if (isAlgorithmParameterIntValueType)
                    {
                        for (int algValIndex = 0; algValIndex < testing.AlgorithmParametersAllIntValues[alg].Count; algValIndex++)
                        {
                            allCombinations.Add(new int[1] { algValIndex });
                        }
                    }
                    else
                    {
                        for (int algValIndex = 0; algValIndex < testing.AlgorithmParametersAllDoubleValues[alg].Count; algValIndex++)
                        {
                            allCombinations.Add(new int[1] { algValIndex });
                        }
                    }
                }
                else
                {
                    List<int> indexes = new List<int>();
                    if (isAlgorithmParameterIntValueType)
                    {
                        for (int algValIndex = 0; algValIndex < testing.AlgorithmParametersAllIntValues[alg].Count; algValIndex++)
                        {
                            indexes.Add(algValIndex);
                        }
                    }
                    else
                    {
                        for (int algValIndex = 0; algValIndex < testing.AlgorithmParametersAllDoubleValues[alg].Count; algValIndex++)
                        {
                            indexes.Add(algValIndex);
                        }
                    }
                    allCombinations = CreateCombinations(allCombinations, indexes);
                }
            }

            //формируем тестовые связки
            foreach (DataSourceGroup dataSourceGroup in testing.DataSourceGroups)
            {
                //формируем серии оптимизационных тестов для данного источника данных для каждого периода

                //определяем диапазон доступных дат для данной группы источников данных (начальная и конечная даты которые есть во всех источниках данных группы)
                DateTime availableDateStart = new DateTime();
                DateTime availableDateEnd = new DateTime();
                for (int i = 0; i < dataSourceGroup.DataSourceAccordances.Count; i++)
                {
                    if (i == 0)
                    {
                        availableDateStart = dataSourceGroup.DataSourceAccordances[i].DataSource.StartDate.Date;
                        availableDateEnd = dataSourceGroup.DataSourceAccordances[i].DataSource.EndDate.Date;
                    }
                    else
                    {
                        if (DateTime.Compare(availableDateStart, dataSourceGroup.DataSourceAccordances[i].DataSource.StartDate) < 0)
                        {
                            availableDateStart = dataSourceGroup.DataSourceAccordances[i].DataSource.StartDate.Date;
                        }
                        if (DateTime.Compare(availableDateEnd, dataSourceGroup.DataSourceAccordances[i].DataSource.EndDate) > 0)
                        {
                            availableDateEnd = dataSourceGroup.DataSourceAccordances[i].DataSource.EndDate.Date;
                        }
                    }
                }
                availableDateEnd = availableDateEnd.AddDays(1); //прибавляем 1 день, т.к. в расчетах последний день является днем окончания и не торговым днем, а здесь последний день вычисляется как торговый

                //определяем дату окончания тестирования
                DateTime endDate = DateTime.Compare(availableDateEnd, testing.EndPeriod) > 0 ? testing.EndPeriod : availableDateEnd;

                DateTime currentDate = testing.StartPeriod; //текущая дата

                //определяем минимально допустимую длительность оптимизационного теста ((текущая дата + оптимизация  -  текущая) * % из настроек)
                TimeSpan minimumAllowedOptimizationDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days) - currentDate).TotalDays * ((double)_modelData.Settings.Where(i => i.Id == 2).First().IntValue / 100)));
                minimumAllowedOptimizationDuration = minimumAllowedOptimizationDuration.TotalDays < 1 ? TimeSpan.FromDays(1) : minimumAllowedOptimizationDuration; //если менее одного дня, устанавливаем в один день
                //определяем минимально допустимую длительность форвардного теста ((текущая дата + оптимизация + форвардный  -  текущая + оптимизация) * % из настроек)
                TimeSpan minimumAllowedForwardDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days).AddYears(testing.DurationForwardTest.Years).AddMonths(testing.DurationForwardTest.Months).AddDays(testing.DurationForwardTest.Days) - currentDate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days)).TotalDays * ((double)_modelData.Settings.Where(i => i.Id == 2).First().IntValue / 100)));
                minimumAllowedForwardDuration = minimumAllowedForwardDuration.TotalDays < 1 && testing.IsForwardTesting ? TimeSpan.FromDays(1) : minimumAllowedForwardDuration; //если менее одного дня и это форвардное тестирование, устанавливаем в один день (при не форвардном будет 0)

                //в цикле определяется минимально допустимая длительность для следующей проверки, исходя из разности дат текущей и текущей + требуемой
                //цикл проверяет, помещается ли минимум в оставшийся период, так же в нем идет проверка на то, текущая раньше доступной или нет. Если да, то проверяется, помещается ли в период с доступной по текущая + промежуток, минимальная длительность. Если да, то текущая для расчетов устанавливается в начало доступной. Все даты определяются из текущей для расчетов, а не из текущей. Поэтому после установки текущей для расчетов в доступную, можно дальше расчитывать даты тем же алгоритмом что и для варианта когда текущая позже или равна доступной. Если же с доступной до текущей + промежуток минимальная длительность не помещается, цикл переходит на следующую итерацию.
                while (DateTime.Compare(currentDate.Add(minimumAllowedOptimizationDuration).Add(minimumAllowedForwardDuration).Date, endDate) <= 0)
                {
                    DateTime currentDateForCalculate = currentDate; //текущая дата для расчетов, в неё будет попадать доступная дата начала, если текущая раньше доступной

                    bool isSkipIteration = false; //пропустить итерацию или нет
                    //проверяем, текущая дата раньше доступной даты начала или нет
                    if (DateTime.Compare(currentDate, availableDateStart) < 0)
                    {
                        //проверяем, помещается ли минимальная длительность оптимизационного и форвардного тестов в промежуток с доступной даты начала по текущая + промежуток
                        if (DateTime.Compare(availableDateStart.Add(minimumAllowedOptimizationDuration).Add(minimumAllowedForwardDuration).Date, currentDate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days).AddYears(testing.DurationForwardTest.Years).AddMonths(testing.DurationForwardTest.Months).AddDays(testing.DurationForwardTest.Days)) < 0)
                        {
                            currentDateForCalculate = availableDateStart;
                        }
                        else
                        {
                            isSkipIteration = true; //т.к. минимально допустимая длительность не помещается в текущий промежуток, переходим на следующую итерацию цикла
                        }
                    }

                    if (isSkipIteration == false) //если минимальная длительность помещается в доступную, создаем тесты
                    {
                        //определяем начальные и конечные даты оптимизационного и форвардного тестов
                        DateTime optimizationStartDate = new DateTime();
                        DateTime optimizationEndDate = new DateTime(); //дата, на которой заканчивается тест, этот день не торговый
                        DateTime forwardStartDate = new DateTime();
                        DateTime forwardEndDate = new DateTime(); //дата, на которой заканчивается тест, этот день не торговый

                        //проверяем, помещается ли полная оптимизационная и форвардная длительность в доступный промежуток
                        if (DateTime.Compare(currentDateForCalculate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days).AddYears(testing.DurationForwardTest.Years).AddMonths(testing.DurationForwardTest.Months).AddDays(testing.DurationForwardTest.Days), currentDate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days).AddYears(testing.DurationForwardTest.Years).AddMonths(testing.DurationForwardTest.Months).AddDays(testing.DurationForwardTest.Days)) > 0) //если текущая дата для расчетов + полная длительность позже текущей даты + полная длительность, значит не помещается
                        {
                            //определяем максимальную длительность, которая помещается в доступный промежуток
                            double currentDurationPercent = 99.75;
                            TimeSpan currentOptimizationDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days) - currentDate).TotalDays * (currentDurationPercent / 100)));
                            currentOptimizationDuration = currentOptimizationDuration.TotalDays < 1 ? TimeSpan.FromDays(1) : currentOptimizationDuration; //если менее одного дня, устанавливаем в один день
                            TimeSpan currentForwardDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days).AddYears(testing.DurationForwardTest.Years).AddMonths(testing.DurationForwardTest.Months).AddDays(testing.DurationForwardTest.Days) - currentDate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days)).TotalDays * (currentDurationPercent / 100)));
                            currentForwardDuration = currentForwardDuration.TotalDays < 1 && testing.IsForwardTesting ? TimeSpan.FromDays(1) : currentForwardDuration; //если менее одного дня и это форвардное тестирование, устанавливаем в один день (при не форвардном будет 0)
                            //пока период с уменьшенной длительностью не поместится, уменьшаем длительность (пока текущая дата для расчетов + уменьшенная длительность больше текущей даты + полная длительность)
                            while (DateTime.Compare(currentDateForCalculate.Add(currentOptimizationDuration).Add(currentForwardDuration).Date, currentDate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days).AddYears(testing.DurationForwardTest.Years).AddMonths(testing.DurationForwardTest.Months).AddDays(testing.DurationForwardTest.Days)) > 0)
                            {
                                currentDurationPercent -= 0.25;
                                currentOptimizationDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days) - currentDate).TotalDays * (currentDurationPercent / 100)));
                                currentOptimizationDuration = currentOptimizationDuration.TotalDays < 1 ? TimeSpan.FromDays(1) : currentOptimizationDuration; //если менее одного дня, устанавливаем в один день
                                currentForwardDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days).AddYears(testing.DurationForwardTest.Years).AddMonths(testing.DurationForwardTest.Months).AddDays(testing.DurationForwardTest.Days) - currentDate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days)).TotalDays * (currentDurationPercent / 100)));
                                currentForwardDuration = currentForwardDuration.TotalDays < 1 && testing.IsForwardTesting ? TimeSpan.FromDays(1) : currentForwardDuration; //если менее одного дня и это форвардное тестирование, устанавливаем в один день (при не форвардном будет 0)
                            }

                            //устанавливаем начальные и конечные даты оптимизационного и форвардного тестов
                            optimizationStartDate = currentDateForCalculate;
                            optimizationEndDate = currentDateForCalculate.Add(currentOptimizationDuration).Date;
                            forwardStartDate = optimizationEndDate;
                            forwardEndDate = currentDateForCalculate.Add(currentOptimizationDuration).Add(currentForwardDuration).Date;
                        }
                        else
                        {
                            //устанавливаем начальные и конечные даты оптимизационного и форвардного тестов
                            optimizationStartDate = currentDateForCalculate;
                            optimizationEndDate = currentDateForCalculate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days).Date;
                            forwardStartDate = optimizationEndDate;
                            forwardEndDate = currentDateForCalculate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days).AddYears(testing.DurationForwardTest.Years).AddMonths(testing.DurationForwardTest.Months).AddDays(testing.DurationForwardTest.Days).Date;
                        }

                        //создаем testBatch
                        TestBatch testBatch = new TestBatch { DataSourceGroup = dataSourceGroup, DataSourceGroupIndex = testing.DataSourceGroups.IndexOf(dataSourceGroup), StatisticalSignificance = new List<string[]>(), IsTopModelDetermining = false, IsTopModelWasFind = false };

                        int testRunNumber = 1; //номер тестового прогона

                        //формируем оптимизационные тесты
                        List<TestRun> optimizationTestRuns = new List<TestRun>();
                        for (int i = 0; i < allCombinations.Count; i++)
                        {
                            List<DepositCurrency> freeForwardDepositCurrencies = new List<DepositCurrency>(); //свободные средства в открытых позициях
                            List<DepositCurrency> takenForwardDepositCurrencies = new List<DepositCurrency>(); //занятые средства(на которые куплены лоты) в открытых позициях
                            foreach (Currency currency in _modelData.Currencies)
                            {
                                freeForwardDepositCurrencies.Add(new DepositCurrency { Currency = currency, Deposit = 0 });
                                takenForwardDepositCurrencies.Add(new DepositCurrency { Currency = currency, Deposit = 0 });
                            }

                            List<DepositCurrency> firstDepositCurrenciesChanges = new List<DepositCurrency>(); //начальное состояние депозита
                            foreach (DepositCurrency depositCurrency in freeForwardDepositCurrencies)
                            {
                                firstDepositCurrenciesChanges.Add(new DepositCurrency { Currency = depositCurrency.Currency, Deposit = 0 });
                            }
                            List<List<DepositCurrency>> depositCurrenciesChanges = new List<List<DepositCurrency>>();
                            depositCurrenciesChanges.Add(firstDepositCurrenciesChanges);

                            Account account = new Account { Orders = new List<Order>(), AllOrders = new List<Order>(), CurrentPosition = new List<Deal>(), AllDeals = new List<Deal>(), DefaultCurrency = testing.DefaultCurrency, FreeForwardDepositCurrencies = freeForwardDepositCurrencies, TakenForwardDepositCurrencies = takenForwardDepositCurrencies, DepositCurrenciesChanges = depositCurrenciesChanges };
                            //формируем список со значениями параметров алгоритма
                            List<AlgorithmParameterValue> algorithmParameterValues = new List<AlgorithmParameterValue>();
                            int alg = 0;
                            while (alg < testing.Algorithm.AlgorithmParameters.Count)
                            {
                                AlgorithmParameterValue algorithmParameterValue = new AlgorithmParameterValue { AlgorithmParameter = testing.Algorithm.AlgorithmParameters[alg] };
                                if (testing.Algorithm.AlgorithmParameters[alg].ParameterValueType.Id == 1)
                                {
                                    algorithmParameterValue.IntValue = testing.AlgorithmParametersAllIntValues[alg][allCombinations[i][alg]];
                                }
                                else
                                {
                                    algorithmParameterValue.DoubleValue = testing.AlgorithmParametersAllDoubleValues[alg][allCombinations[i][alg]];
                                }
                                algorithmParameterValues.Add(algorithmParameterValue);
                                alg++;
                            }
                            TestRun testRun = new TestRun { Number = testRunNumber, TestBatch = testBatch, Account = account, StartPeriod = optimizationStartDate, EndPeriod = optimizationEndDate, AlgorithmParameterValues = algorithmParameterValues, EvaluationCriteriaValues = new List<EvaluationCriteriaValue>(), DealsDeviation = new List<string>(), LoseDeviation = new List<string>(), ProfitDeviation = new List<string>(), LoseSeriesDeviation = new List<string>(), ProfitSeriesDeviation = new List<string>(), IsComplete = false };
                            testRunNumber++;
                            optimizationTestRuns.Add(testRun);
                        }
                        testBatch.OptimizationTestRuns = optimizationTestRuns;

                        //формируем форвардный тест
                        if (testing.IsForwardTesting)
                        {
                            List<DepositCurrency> freeForwardDepositCurrencies = new List<DepositCurrency>(); //свободные средства в открытых позициях
                            List<DepositCurrency> takenForwardDepositCurrencies = new List<DepositCurrency>(); //занятые средства(на которые куплены лоты) в открытых позициях
                            foreach (Currency currency in _modelData.Currencies)
                            {
                                freeForwardDepositCurrencies.Add(new DepositCurrency { Currency = currency, Deposit = 0 });
                                takenForwardDepositCurrencies.Add(new DepositCurrency { Currency = currency, Deposit = 0 });
                            }

                            List<DepositCurrency> firstDepositCurrenciesChanges = new List<DepositCurrency>(); //начальное состояние депозита
                            foreach (DepositCurrency depositCurrency in freeForwardDepositCurrencies)
                            {
                                firstDepositCurrenciesChanges.Add(new DepositCurrency { Currency = depositCurrency.Currency, Deposit = 0 });
                            }
                            List<List<DepositCurrency>> depositCurrenciesChanges = new List<List<DepositCurrency>>();
                            depositCurrenciesChanges.Add(firstDepositCurrenciesChanges);

                            Account account = new Account { Orders = new List<Order>(), AllOrders = new List<Order>(), CurrentPosition = new List<Deal>(), AllDeals = new List<Deal>(), DefaultCurrency = testing.DefaultCurrency, IsForwardDepositTrading = false, FreeForwardDepositCurrencies = freeForwardDepositCurrencies, TakenForwardDepositCurrencies = takenForwardDepositCurrencies, DepositCurrenciesChanges = depositCurrenciesChanges };
                            TestRun testRun = new TestRun { Number = testRunNumber, TestBatch = testBatch, Account = account, StartPeriod = forwardStartDate, EndPeriod = forwardEndDate, EvaluationCriteriaValues = new List<EvaluationCriteriaValue>(), DealsDeviation = new List<string>(), LoseDeviation = new List<string>(), ProfitDeviation = new List<string>(), LoseSeriesDeviation = new List<string>(), ProfitSeriesDeviation = new List<string>(), IsComplete = false };
                            testRunNumber++;
                            //добавляем форвардный тест в testBatch
                            testBatch.ForwardTestRun = testRun;
                        }
                        //формируем форвардный тест с торговлей депозитом
                        if (testing.IsForwardTesting && testing.IsForwardDepositTrading)
                        {
                            List<DepositCurrency> freeForwardDepositCurrencies = new List<DepositCurrency>(); //свободные средства в открытых позициях
                            List<DepositCurrency> takenForwardDepositCurrencies = new List<DepositCurrency>(); //занятые средства(на которые куплены лоты) в открытых позициях
                            foreach (DepositCurrency depositCurrency in testing.ForwardDepositCurrencies)
                            {
                                freeForwardDepositCurrencies.Add(new DepositCurrency { Currency = depositCurrency.Currency, Deposit = depositCurrency.Deposit });
                                takenForwardDepositCurrencies.Add(new DepositCurrency { Currency = depositCurrency.Currency, Deposit = 0 });
                            }

                            List<DepositCurrency> firstDepositCurrenciesChanges = new List<DepositCurrency>(); //начальное состояние депозита
                            foreach (DepositCurrency depositCurrency in freeForwardDepositCurrencies)
                            {
                                firstDepositCurrenciesChanges.Add(new DepositCurrency { Currency = depositCurrency.Currency, Deposit = depositCurrency.Deposit });
                            }
                            List<List<DepositCurrency>> depositCurrenciesChanges = new List<List<DepositCurrency>>();
                            depositCurrenciesChanges.Add(firstDepositCurrenciesChanges);

                            Account account = new Account { Orders = new List<Order>(), AllOrders = new List<Order>(), CurrentPosition = new List<Deal>(), AllDeals = new List<Deal>(), DefaultCurrency = testing.DefaultCurrency, IsForwardDepositTrading = true, FreeForwardDepositCurrencies = freeForwardDepositCurrencies, TakenForwardDepositCurrencies = takenForwardDepositCurrencies, DepositCurrenciesChanges = depositCurrenciesChanges };
                            TestRun testRun = new TestRun { Number = testRunNumber, TestBatch = testBatch, Account = account, StartPeriod = forwardStartDate, EndPeriod = forwardEndDate, EvaluationCriteriaValues = new List<EvaluationCriteriaValue>(), DealsDeviation = new List<string>(), LoseDeviation = new List<string>(), ProfitDeviation = new List<string>(), LoseSeriesDeviation = new List<string>(), ProfitSeriesDeviation = new List<string>(), IsComplete = false };
                            testRunNumber++;
                            //добавляем форвардный тест с торговлей депозитом в testBatch
                            testBatch.ForwardTestRunDepositTrading = testRun;
                        }

                        testing.TestBatches.Add(testBatch);
                    }

                    //прибавляем к текущей дате временной промежуток между оптимизационными тестами
                    currentDate = currentDate.AddYears(testing.OptimizationTestSpacing.Years).AddMonths(testing.OptimizationTestSpacing.Months).AddDays(testing.OptimizationTestSpacing.Days);

                    //определяем минимально допустимую длительность оптимизационного теста ((текущая дата + оптимизация  -  текущая) * % из настроек)
                    minimumAllowedOptimizationDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days) - currentDate).TotalDays * ((double)_modelData.Settings.Where(i => i.Id == 2).First().IntValue / 100)));
                    minimumAllowedOptimizationDuration = minimumAllowedOptimizationDuration.TotalDays < 1 ? TimeSpan.FromDays(1) : minimumAllowedOptimizationDuration; //если менее одного дня, устанавливаем в один день
                    //определяем минимально допустимую длительность форвардного теста ((текущая дата + оптимизация + форвардный  -  текущая + оптимизация) * % из настроек)
                    minimumAllowedForwardDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days).AddYears(testing.DurationForwardTest.Years).AddMonths(testing.DurationForwardTest.Months).AddDays(testing.DurationForwardTest.Days) - currentDate.AddYears(testing.DurationOptimizationTests.Years).AddMonths(testing.DurationOptimizationTests.Months).AddDays(testing.DurationOptimizationTests.Days)).TotalDays * ((double)_modelData.Settings.Where(i => i.Id == 2).First().IntValue / 100)));
                    minimumAllowedForwardDuration = minimumAllowedForwardDuration.TotalDays < 1 && testing.IsForwardTesting ? TimeSpan.FromDays(1) : minimumAllowedForwardDuration; //если менее одного дня и это форвардное тестирование, устанавливаем в один день (при не форвардном будет 0)
                }
            }

            //выполняем компиляцию индикаторов алгоритма, алгоритма и критериев оценки
            bool isErrorCompile = false; //были ли ошибки при компиляции

            //создаем объекты индикаторов алгоритма
            testing.CompiledIndicators = new dynamic[testing.Algorithm.AlgorithmIndicators.Count];
            for(int i = 0; i < testing.Algorithm.AlgorithmIndicators.Count; i++)
            {
                testing.CompiledIndicators[i] = AlgorithmIndicatorCompile(testing.Algorithm.AlgorithmIndicators[i]); //возвращает скомпилированный объект или null в случае ошибки компиляции
                if (testing.CompiledIndicators[i] == null)
                {
                    isErrorCompile = true;
                }
            }

            //создаем объекты алгоритма
            testing.CompiledAlgorithm = AlgorithmCompile(testing.Algorithm); //возвращает скомпилированный объект или null в случае ошибки компиляции
            if (testing.CompiledAlgorithm == null)
            {
                isErrorCompile = true;
            }

            //создаем объекты критериев оценки
            testing.CompiledEvaluationCriterias = new dynamic[_modelData.EvaluationCriterias.Count];
            for (int i = 0; i < _modelData.EvaluationCriterias.Count; i++)
            {
                testing.CompiledEvaluationCriterias[i] = EvaluationCriteriaCompile(_modelData.EvaluationCriterias[i]);
                if (testing.CompiledEvaluationCriterias[i] == null)
                {
                    isErrorCompile = true;
                }
            }


            if (isErrorCompile == false)
            {
                //определяем количество testRun без учета форвардных
                int countTestRuns = 0;
                foreach (TestBatch testBatch1 in testing.TestBatches)
                {
                    foreach (TestRun testRun in testBatch1.OptimizationTestRuns)
                    {
                        countTestRuns++;
                    }
                }

                int countTestRunWithForward = countTestRuns;
                countTestRunWithForward += testing.IsForwardTesting ? testing.TestBatches.Count : 0;
                countTestRunWithForward += testing.IsForwardTesting && testing.IsForwardDepositTrading ? testing.TestBatches.Count : 0;

                if (countTestRuns > 0) //если количество тестов больше нуля, переходим на создание задач и выполнение тестов
                {
                    CancellationToken cancellationToken = ModelTesting.CancellationTokenTesting.Token;

                    NumberFormatInfo nfiComma = CultureInfo.GetCultureInfo("ru-RU").NumberFormat;
                    NumberFormatInfo nfiDot = (NumberFormatInfo)nfiComma.Clone();
                    nfiDot.NumberDecimalSeparator = nfiDot.CurrencyDecimalSeparator = nfiDot.PercentDecimalSeparator = "."; //эту переменнную нужно указать в методе double.Parse(string, nfiDot), чтобы преобразовался формат строки с разделителем дробной части в виде точки а не запятой

                    //определяем количество уникальных источников данных во всех группах источников данных
                    List<DataSource> dataSources = new List<DataSource>();
                    foreach (DataSourceGroup dataSourceGroup in testing.DataSourceGroups)
                    {
                        foreach (DataSourceAccordance dataSourceAccordance in dataSourceGroup.DataSourceAccordances)
                        {
                            if(dataSources.Contains(dataSourceAccordance.DataSource) == false)
                            {
                                dataSources.Add(dataSourceAccordance.DataSource);
                            }
                        }
                    }
                    //считываем свечки всех источников данных
                    int filesCount = 0; //всего файлов с свечками
                    foreach(DataSource dataSource in dataSources)
                    {
                        filesCount += dataSource.DataSourceFiles.Count;
                    }
                    int readFilesCount = 0; //считано файлов с свечками
                    Stopwatch stopwatchReadDataSources = new Stopwatch();
                    stopwatchReadDataSources.Start();
                    DispatcherInvoke((Action)(() => {
                        _mainCommunicationChannel.TestingProgress.Clear();
                        _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 1/4:  Считывание файлов источников данных", StepTasksCount = filesCount, CompletedStepTasksCount = readFilesCount, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatchReadDataSources.Elapsed, CancelPossibility = true, IsFinishSimulation = false, IsSuccessSimulation = false, IsFinish = false });
                    }));
                    testing.DataSourcesCandles = new List<DataSourceCandles>(); //инициализируем список со всеми свечками источников данных
                    for(int i = 0; i < dataSources.Count; i++)
                    {
                        testing.DataSourcesCandles.Add(new DataSourceCandles { DataSource = dataSources[i], Candles = new Candle[dataSources[i].DataSourceFiles.Count][], AlgorithmIndicatorsValues = new AlgorithmIndicatorValues[testing.Algorithm.AlgorithmIndicators.Count] });
                        //проходим по всем файлам источника данных
                        for (int k = 0; k < dataSources[i].DataSourceFiles.Count; k++)
                        {
                            string fileName = dataSources[i].DataSourceFiles[k].Path;
                            //определяем размер массива (исходя из количества строк в файле)
                            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                            StreamReader streamReader = new StreamReader(fileStream);
                            string line = streamReader.ReadLine(); //пропускаем шапку файла
                            line = streamReader.ReadLine();
                            int count = 0;
                            while (line != null)
                            {
                                count++;
                                line = streamReader.ReadLine();
                            }
                            streamReader.Close();
                            fileStream.Close();

                            //создаем массив
                            Candle[] candles = new Candle[count];
                            //заполняем массив
                            fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                            streamReader = new StreamReader(fileStream);
                            line = streamReader.ReadLine(); //пропускаем шапку файла
                            line = streamReader.ReadLine(); //счиытваем 1-ю строку с данными
                            int r = 0;
                            while (line != null)
                            {
                                string[] lineArr = line.Split(',');
                                string dateTimeFormated = lineArr[2].Insert(6, "-").Insert(4, "-") + " " + lineArr[3].Insert(4, ":").Insert(2, ":");
                                candles[r] = new Candle { DateTime = DateTime.Parse(dateTimeFormated), O = double.Parse(lineArr[4], nfiDot), H = double.Parse(lineArr[5], nfiDot), L = double.Parse(lineArr[6], nfiDot), C = double.Parse(lineArr[7], nfiDot), V = double.Parse(lineArr[8], nfiDot) };
                                line = streamReader.ReadLine();
                                r++;
                            }
                            streamReader.Close();
                            fileStream.Close();

                            testing.DataSourcesCandles[i].Candles[k] = candles;

                            readFilesCount++;
                            DispatcherInvoke((Action)(() => {
                                _mainCommunicationChannel.TestingProgress.Clear();
                                _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 1/4:  Считывание файлов источников данных", StepTasksCount = filesCount, CompletedStepTasksCount = readFilesCount, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatchReadDataSources.Elapsed, CancelPossibility = true, IsFinishSimulation = false, IsSuccessSimulation = false, IsFinish = false });
                            }));
                        }
                    }
                    stopwatchReadDataSources.Stop();

                    Stopwatch stopwatchCalculateIndicators = new Stopwatch();
                    stopwatchCalculateIndicators.Start();
                    //формируем AlgorithmIndicatorCatalogElements для DataSourcesCandles
                    for (int i = 0; i < testing.DataSourcesCandles.Count; i++)
                    {
                        //определяем каталоги индикаторов алгоритмов
                        testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs = new AlgorithmIndicatorCatalog[testing.Algorithm.AlgorithmIndicators.Count];
                        //проходим по всем индикаторам алгоритма
                        int algorithmIndicatorIndex = 0;
                        foreach (AlgorithmIndicator algorithmIndicator in testing.Algorithm.AlgorithmIndicators)
                        {
                            AlgorithmIndicatorCatalog algorithmIndicatorCatalog = new AlgorithmIndicatorCatalog { AlgorithmIndicator = algorithmIndicator, AlgorithmIndicatorFolderName = algorithmIndicator.Indicator.Name + "_" + algorithmIndicator.Ending + "_values", AlgorithmIndicatorCatalogElements = new List<AlgorithmIndicatorCatalogElement>() };

                            //получаем список параметров алгоритмов, используемых в индикаторе алгоритма
                            List<AlgorithmParameter> algorithmParameters = new List<AlgorithmParameter>();
                            foreach (IndicatorParameterRange indicatorParameterRange in algorithmIndicator.IndicatorParameterRanges)
                            {
                                if (algorithmParameters.Contains(indicatorParameterRange.AlgorithmParameter) == false)
                                {
                                    algorithmParameters.Add(indicatorParameterRange.AlgorithmParameter);
                                }
                            }

                            if (algorithmParameters.Count == 0) //если нет параметров, значит только 1 вариант значений индикатора для источника данных
                            {
                                algorithmIndicatorCatalog.AlgorithmIndicatorCatalogElements.Add(new AlgorithmIndicatorCatalogElement { AlgorithmParameterValues = new List<AlgorithmParameterValue>(), FileName = "withoutParameters.dat" });
                            }
                            else
                            {
                                //формируем список со всеми комбинациями параметров алгоритма данного индикатора алгоритма
                                List<int[][]> algorithmParameterCombinations = new List<int[][]>(); //список с комбинациями (массивами с комбинацией параметров алгоритма и значения): 0-й элемент - индекс параметра алгоритма во всех параметрах, 1-й - индекс значения параметра во всех значениях параметров алгоритма
                                                                                                    //algorithmParameterCombinations[0] - первая комбинация параметров алгоритма
                                                                                                    //algorithmParameterCombinations[0][0] - первый параметр комбинации
                                                                                                    //algorithmParameterCombinations[0][0][0] - индекс параметра алгоритма первого элемента первой комбинации
                                                                                                    //algorithmParameterCombinations[0][0][1] - индекс значения параметра алгоритма первого элемента первой комбинации

                                //заполняем комбинации всеми вариантами первого параметра
                                int indexFirstParameter = testing.Algorithm.AlgorithmParameters.IndexOf(algorithmParameters[0]); //индекс первого параметра алгоритма индикатора алгоритма
                                int countParameterValues1 = testing.Algorithm.AlgorithmParameters[indexFirstParameter].ParameterValueType.Id == 1 ? testing.AlgorithmParametersAllIntValues[indexFirstParameter].Count : testing.AlgorithmParametersAllDoubleValues[indexFirstParameter].Count; //количество значений текущего параметра алгоритма
                                for (int k = 0; k < countParameterValues1; k++)
                                {
                                    int[][] arr = new int[1][];
                                    arr[0] = new int[2] { indexFirstParameter, k }; //записываем индекс параметра алгоритма и индекс значения параметра
                                    algorithmParameterCombinations.Add(arr);
                                }

                                //формируем комбинации со всеми параметрами кроме первого
                                for (int k = 1; k < algorithmParameters.Count; k++)
                                {
                                    int indexAlgorithmParameter = testing.Algorithm.AlgorithmParameters.IndexOf(algorithmParameters[k]); //индекс текущего параметра алгоритма
                                    List<int[][]> newAlgorithmParameterCombinations = new List<int[][]>(); //новые комбинации. Для всех элементов старых комбинаций будут созданы комбинации с текущим параметром и старые комбинации обновятся на новые
                                    for (int u = 0; u < algorithmParameterCombinations.Count; u++)
                                    {
                                        int countParameterValues2 = testing.Algorithm.AlgorithmParameters[indexAlgorithmParameter].ParameterValueType.Id == 1 ? testing.AlgorithmParametersAllIntValues[indexAlgorithmParameter].Count : testing.AlgorithmParametersAllDoubleValues[indexAlgorithmParameter].Count; //количество значений текущего параметра алгоритма
                                        for (int y = 0; y < countParameterValues2; y++)
                                        {
                                            int[][] arr = new int[algorithmParameterCombinations[u].Length + 1][]; //увеличиваем количество элементов в комбинации на 1
                                                                                                                   //заполняем комбинацию старыми элементами
                                            for (int x = 0; x < algorithmParameterCombinations[u].Length; x++)
                                            {
                                                arr[x] = algorithmParameterCombinations[u][x];
                                            }
                                            //записываем новый параметр
                                            arr[arr.Length - 1] = new int[2] { indexAlgorithmParameter, y }; //записываем индекс параметра алгоритма и индекс значения параметра
                                            newAlgorithmParameterCombinations.Add(arr);
                                        }
                                    }
                                    algorithmParameterCombinations = newAlgorithmParameterCombinations;
                                }

                                //для каждой комбинации параметров формируем элемент каталога индикатора алгоритма
                                for (int k = 0; k < algorithmParameterCombinations.Count; k++)
                                {
                                    string fileName = "";
                                    foreach (int[] value in algorithmParameterCombinations[k])
                                    {
                                        string parameterValue = testing.Algorithm.AlgorithmParameters[value[0]].ParameterValueType.Id == 1 ? testing.AlgorithmParametersAllIntValues[value[0]][value[1]].ToString() : testing.AlgorithmParametersAllDoubleValues[value[0]][value[1]].ToString();
                                        fileName += fileName.Length == 0 ? "" : " "; //если это не первые символы названия, отделяем их пробелом от предыдущих
                                        fileName += testing.Algorithm.AlgorithmParameters[value[0]].Name + "=" + parameterValue;
                                    }
                                    fileName += ".dat";
                                    AlgorithmIndicatorCatalogElement algorithmIndicatorCatalogElement = new AlgorithmIndicatorCatalogElement { AlgorithmParameterValues = new List<AlgorithmParameterValue>(), FileName = fileName };
                                    for (int u = 0; u < algorithmParameterCombinations[k].Length; u++)
                                    {
                                        int intValue = testing.Algorithm.AlgorithmParameters[algorithmParameterCombinations[k][u][0]].ParameterValueType.Id == 1 ? testing.AlgorithmParametersAllIntValues[algorithmParameterCombinations[k][u][0]][algorithmParameterCombinations[k][u][1]] : 0;
                                        double doubleValue = testing.Algorithm.AlgorithmParameters[algorithmParameterCombinations[k][u][0]].ParameterValueType.Id == 1 ? 0 : testing.AlgorithmParametersAllDoubleValues[algorithmParameterCombinations[k][u][0]][algorithmParameterCombinations[k][u][1]];
                                        algorithmIndicatorCatalogElement.AlgorithmParameterValues.Add(new AlgorithmParameterValue { AlgorithmParameter = testing.Algorithm.AlgorithmParameters[algorithmParameterCombinations[k][u][0]], IntValue = intValue, DoubleValue = doubleValue });
                                    }
                                    algorithmIndicatorCatalog.AlgorithmIndicatorCatalogElements.Add(algorithmIndicatorCatalogElement);
                                }
                            }
                            testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs[algorithmIndicatorIndex] = algorithmIndicatorCatalog; //записываем каталог для индикатора алгоритма
                            algorithmIndicatorIndex++;
                        }
                    }

                    //определяем количество значений всех индикаторов
                    int AlgorithmIndicatorsValuesCount = 0; //количество значений всех индикаторов
                    int CalculatedAlgorithmIndicatorsCount = 0; //количество вычисленных индикаторов
                    for (int i = 0; i < testing.DataSourcesCandles.Count; i++)
                    {
                        foreach (AlgorithmIndicatorCatalog algorithmIndicatorCatalog in testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs)
                        {
                            AlgorithmIndicatorsValuesCount += algorithmIndicatorCatalog.AlgorithmIndicatorCatalogElements.Count;
                        }
                    }
                    DispatcherInvoke((Action)(() => {
                        _mainCommunicationChannel.TestingProgress.Clear();
                        _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 2/4:  Вычисление индикаторов", StepTasksCount = AlgorithmIndicatorsValuesCount, CompletedStepTasksCount = CalculatedAlgorithmIndicatorsCount, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatchCalculateIndicators.Elapsed, CancelPossibility = true, IsFinishSimulation = false, IsSuccessSimulation = false, IsFinish = false });
                    }));

                    //вычисляем значения индикаторов для всех источников данных со всеми комбинациями оптимизационных параметров
                    for (int i = 0; i < testing.DataSourcesCandles.Count; i++)
                    {
                        foreach (AlgorithmIndicatorCatalog algorithmIndicatorCatalog in testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs)
                        {
                            foreach (AlgorithmIndicatorCatalogElement algorithmIndicatorCatalogElement in algorithmIndicatorCatalog.AlgorithmIndicatorCatalogElements)
                            {
                                //вычисляем значения индикатора алгоритма
                                algorithmIndicatorCatalogElement.AlgorithmIndicatorValues = AlgorithmIndicatorCalculate(testing, testing.DataSourcesCandles[i], algorithmIndicatorCatalog.AlgorithmIndicator, algorithmIndicatorCatalogElement.AlgorithmParameterValues);
                                DispatcherInvoke((Action)(() => {
                                    _mainCommunicationChannel.TestingProgress.Clear();
                                    _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 2/4:  Вычисление индикаторов", StepTasksCount = AlgorithmIndicatorsValuesCount, CompletedStepTasksCount = CalculatedAlgorithmIndicatorsCount, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatchCalculateIndicators.Elapsed, CancelPossibility = true, IsFinishSimulation = false, IsSuccessSimulation = false, IsFinish = false });
                                }));
                                CalculatedAlgorithmIndicatorsCount++;
                            }
                        }
                    }
                    stopwatchCalculateIndicators.Stop();

                    //формируем сегменты для всех групп источников данных
                    testing.DataSourceGroupsSegments = new List<DataSourceGroupSegments>();
                    foreach(DataSourceGroup dataSourceGroup in testing.DataSourceGroups)
                    {
                        int[] fileIndexes = Enumerable.Repeat(0, dataSourceGroup.DataSourceAccordances.Count).ToArray(); //индексы файлов для всех источников данных группы
                        int[] candleIndexes = Enumerable.Repeat(0, dataSourceGroup.DataSourceAccordances.Count).ToArray(); //индексы свечек для всех источников данных группы
                        DataSourceGroupSegments dataSourceGroupSegments = new DataSourceGroupSegments();
                        dataSourceGroupSegments.DataSourceGroup = dataSourceGroup;
                        dataSourceGroupSegments.Sections = new List<Section>(); //секции
                        dataSourceGroupSegments.Segments = new List<Segment>();
                        dataSourceGroupSegments.LastTradeSegmentIndex = -1; //устанавливаем в -1, чтобы установить этот индекс на первый индекс, на котором закончатся файлы одного из источников данных
                        List<DataSource> endedDataSources = new List<DataSource>(); //источники данных, которые закончились (индекс файла вышел за границы массива)
                        DateTime currentDateTime = new DateTime(); //текущая дата
                        //определяем самую раннюю дату среди всех источников данных группы
                        for(int i = 0; i < dataSourceGroup.DataSourceAccordances.Count; i++)
                        {
                            if (i == 0)
                            {
                                currentDateTime = testing.DataSourcesCandles.Where(j => j.DataSource.Id == dataSourceGroup.DataSourceAccordances[i].DataSource.Id).First().Candles[fileIndexes[i]][candleIndexes[i]].DateTime;
                            }
                            else
                            {
                                DateTime dateTime = testing.DataSourcesCandles.Where(j => j.DataSource.Id == dataSourceGroup.DataSourceAccordances[i].DataSource.Id).First().Candles[fileIndexes[i]][candleIndexes[i]].DateTime;
                                if(DateTime.Compare(dateTime, currentDateTime) < 0) //если дата свечки у текущего источника данных раньше текущей даты, обновляем текущую дату
                                {
                                    currentDateTime = dateTime;
                                }
                            }
                        }
                        DateTime laterDateTime = currentDateTime; //самая поздняя дата и время, используется для определения дат которые уже были

                        bool isAllDataSourcesEnd = false; //закончились ли все источники данных
                        Section section = new Section(); //первая секция
                        section.IsPresent = true;
                        section.DataSources = new List<DataSource>();
                        section.DataSourceCandlesIndexes = new List<int>();
                        foreach (DataSourceAccordance dataSourceAccordance in dataSourceGroup.DataSourceAccordances)
                        {
                            section.DataSources.Add(dataSourceAccordance.DataSource);
                        }
                        dataSourceGroupSegments.Sections.Add(section);
                        while (isAllDataSourcesEnd == false)
                        {
                            int[] sectionDataSourceCountSegments = Enumerable.Repeat(0, dataSourceGroupSegments.Sections.Last().DataSources.Count).ToArray(); //количество сегментов для источников данных в секции. Значение в sectionDataSourceCountSegments[i] соответствует количеству сегментов с источником данных: sections.Last().DataSources[i]
                            bool isNewSection = false; //перешли ли на новую секцию
                            while(isNewSection == false)
                            {
                                //определяем текущую дату (самую раннюю дату среди всех источников данных секции)
                                for (int i = 0; i < dataSourceGroup.DataSourceAccordances.Count; i++)
                                {
                                    if (dataSourceGroupSegments.Sections.Last().DataSources.Where(a => a.Id == dataSourceGroup.DataSourceAccordances[i].DataSource.Id).Any()) //если в текущей секции имеется текущий источник данных
                                    {
                                        if (i == 0)
                                        {
                                            currentDateTime = testing.DataSourcesCandles.Where(j => j.DataSource.Id == dataSourceGroup.DataSourceAccordances[i].DataSource.Id).First().Candles[fileIndexes[i]][candleIndexes[i]].DateTime;
                                        }
                                        else
                                        {
                                            DateTime dateTime = testing.DataSourcesCandles.Where(j => j.DataSource.Id == dataSourceGroup.DataSourceAccordances[i].DataSource.Id).First().Candles[fileIndexes[i]][candleIndexes[i]].DateTime;
                                            if (DateTime.Compare(dateTime, currentDateTime) < 0) //если дата свечки у текущего источника данных раньше текущей даты, обновляем текущую дату
                                            {
                                                currentDateTime = dateTime;
                                            }
                                        }
                                    }
                                }
                                if(DateTime.Compare(currentDateTime, laterDateTime) > 0) //если текущая дата позже самой поздней, обновляем самую позднюю дату
                                {
                                    laterDateTime = currentDateTime;
                                }
                                //формируем сегмент
                                Segment segment = new Segment();
                                segment.Section = dataSourceGroupSegments.Sections.Last();
                                segment.SectionIndex = dataSourceGroupSegments.Sections.Count - 1; //индекс секции
                                segment.CandleIndexes = new List<CandleIndex>();
                                //проходим по источникам данных секции, и добавляем в сегмент свечки тех которые имеют текущую дату
                                for (int i = 0; i < dataSourceGroup.DataSourceAccordances.Count; i++)
                                {
                                    if(dataSourceGroupSegments.Sections.Last().DataSources.Where(a => a.Id == dataSourceGroup.DataSourceAccordances[i].DataSource.Id).Any()) //если в текущей секции имеется текущий источник данных
                                    {
                                        int dataSourceCandlesIndex = testing.DataSourcesCandles.FindIndex(a => a.DataSource.Id == dataSourceGroup.DataSourceAccordances[i].DataSource.Id);
                                        if (DateTime.Compare(currentDateTime, testing.DataSourcesCandles[dataSourceCandlesIndex].Candles[fileIndexes[i]][candleIndexes[i]].DateTime) == 0) //если текущая дата и дата текущей свечки у текущего источника данных равны
                                        {
                                            segment.CandleIndexes.Add(new CandleIndex { DataSourceCandlesIndex = dataSourceCandlesIndex, FileIndex = fileIndexes[i], IndexCandle = candleIndexes[i] });
                                            int sectionDataSourceIndex = dataSourceGroupSegments.Sections.Last().DataSources.FindIndex(a => a.Id == dataSourceGroup.DataSourceAccordances[i].DataSource.Id);
                                            sectionDataSourceCountSegments[sectionDataSourceIndex]++; //увеличиваем количество свечек с данным источником данных
                                            //переходим на следующую свечку у данного источника данных
                                            candleIndexes[i]++;
                                            if(candleIndexes[i] >= testing.DataSourcesCandles[dataSourceCandlesIndex].Candles[fileIndexes[i]].Length)
                                            {
                                                candleIndexes[i] = 0;
                                                fileIndexes[i]++;
                                                if(fileIndexes[i] >= testing.DataSourcesCandles[dataSourceCandlesIndex].Candles.Length) //если вышли за предел файла и это был первый закончившийся источник данных, запоминаем индекс сегмента на котором торговля заканчивается
                                                {
                                                    if(dataSourceGroupSegments.LastTradeSegmentIndex == -1)
                                                    {
                                                        dataSourceGroupSegments.LastTradeSegmentIndex = dataSourceGroupSegments.Segments.Count; //не вычитаем 1, т.к. еще не добавили текущий сегмент, и индекс будет на 1 больше чем текущий последний
                                                    }
                                                    endedDataSources.Add(dataSourceGroup.DataSourceAccordances[i].DataSource); //запоминаем источник данных, для которого закончились файлы, в последствии при создании новой секции, этот источник данных не будет включен в секцию
                                                    isNewSection = true; //отмечаем, что нужно создать новую секцию, т.к. при текущей секции будут обращения к несуществующему файлу
                                                }
                                            }
                                        }
                                    }
                                }
                                dataSourceGroupSegments.Segments.Add(segment);
                                if (dataSourceGroupSegments.Segments.Count == 28990)
                                {
                                    int t = 0;
                                }
                                if(isNewSection == false) //если не было добавлено новой секции по причине окончания одного из источников данных, проверяем, не закончилась ли секция по причине выхода на дату которая не позже самой поздней или которая позже самой поздней
                                {
                                    if (dataSourceGroupSegments.Sections.Last().IsPresent) //если секция в настоящем, условием для создания новой секции является переход одной из свечек на дату которая равна или раньше самой поздней
                                    {
                                        bool isAllCandlesLater = true; //все ли свечки позднее самой поздней даты
                                        for (int i = 0; i < dataSourceGroup.DataSourceAccordances.Count; i++)
                                        {
                                            if (dataSourceGroupSegments.Sections.Last().DataSources.Where(a => a.Id == dataSourceGroup.DataSourceAccordances[i].DataSource.Id).Any()) //если в текущей секции имеется текущий источник данных
                                            {
                                                int dataSourceCandlesIndex = testing.DataSourcesCandles.FindIndex(a => a.DataSource.Id == dataSourceGroup.DataSourceAccordances[i].DataSource.Id);
                                                if (DateTime.Compare(testing.DataSourcesCandles[dataSourceCandlesIndex].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, laterDateTime) <= 0) //если дата свечки раньше или равняется самой последней дате
                                                {
                                                    isAllCandlesLater = false;
                                                }
                                            }
                                        }
                                        if(isAllCandlesLater == false) //если хоть одна из свечек не была позднее
                                        {
                                            isNewSection = true;
                                        }
                                    }
                                    else //если даты секции уже были, значит условием перехода на следующую секцию является переход на дату, которая позже самой поздней
                                    {
                                        bool isAllCandlesLater = true; //все ли свечки позднее самой поздней даты
                                        for (int i = 0; i < dataSourceGroup.DataSourceAccordances.Count; i++)
                                        {
                                            if (dataSourceGroupSegments.Sections.Last().DataSources.Where(a => a.Id == dataSourceGroup.DataSourceAccordances[i].DataSource.Id).Any()) //если в текущей секции имеется текущий источник данных
                                            {
                                                int dataSourceCandlesIndex = testing.DataSourcesCandles.FindIndex(a => a.DataSource.Id == dataSourceGroup.DataSourceAccordances[i].DataSource.Id);
                                                if (DateTime.Compare(testing.DataSourcesCandles[dataSourceCandlesIndex].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, laterDateTime) <= 0) //если дата свечки раньше или равняется самой последней дате
                                                {
                                                    isAllCandlesLater = false;
                                                }
                                            }
                                        }
                                        if (isAllCandlesLater) //если все свечки были позднее
                                        {
                                            isNewSection = true;
                                        }
                                    }
                                }

                                if (isNewSection) //если нужно добавить новую секцию, добавляем её
                                {
                                    //удаляем из текущей секции источники данных, свечки которых не были добавлены в сегменты секции
                                    for(int i = sectionDataSourceCountSegments.Length - 1; i >= 0 ; i--)
                                    {
                                        if(sectionDataSourceCountSegments[i] == 0) //если не было добавлено свечек с данным источником данных, удаляем его из секции
                                        {
                                            dataSourceGroupSegments.Sections.Last().DataSources.RemoveAt(i);
                                        }
                                    }
                                    //сохраняем индексы объектов со свечками источников данных которые соответствуют источникам данных в DataSources
                                    foreach (DataSource dataSource in dataSourceGroupSegments.Sections.Last().DataSources)
                                    {
                                        dataSourceGroupSegments.Sections.Last().DataSourceCandlesIndexes.Add(testing.DataSourcesCandles.FindIndex(a => a.DataSource.Id == dataSource.Id)); //сохраняем индекс DataSourceCandles в котором свечки данного источника данных
                                    }

                                    //добавляем новую секцию
                                    Section newSection = new Section(); //новая секция
                                    newSection.DataSources = new List<DataSource>();
                                    newSection.DataSourceCandlesIndexes = new List<int>();
                                    foreach (DataSourceAccordance dataSourceAccordance in dataSourceGroup.DataSourceAccordances)
                                    {
                                        if(endedDataSources.Where(a => a.Id == dataSourceAccordance.DataSource.Id).Any() == false) //если данного источника данных нет в списке закончившихся источников данных
                                        {
                                            newSection.DataSources.Add(dataSourceAccordance.DataSource);
                                        }
                                    }
                                    //определяем, секция в настоящем или прошлом
                                    bool isAllCandlesLater = true; //все ли свечки позднее самой поздней даты
                                    for (int i = 0; i < dataSourceGroup.DataSourceAccordances.Count; i++)
                                    {
                                        if (newSection.DataSources.Where(a => a.Id == dataSourceGroup.DataSourceAccordances[i].DataSource.Id).Any()) //если в текущей секции имеется текущий источник данных
                                        {
                                            int dataSourceCandlesIndex = testing.DataSourcesCandles.FindIndex(a => a.DataSource.Id == dataSourceGroup.DataSourceAccordances[i].DataSource.Id);
                                            if (DateTime.Compare(testing.DataSourcesCandles[dataSourceCandlesIndex].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, laterDateTime) <= 0) //если дата свечки раньше или равняется самой последней дате
                                            {
                                                isAllCandlesLater = false;
                                            }
                                        }
                                    }
                                    newSection.IsPresent = isAllCandlesLater ? true : false;
                                    dataSourceGroupSegments.Sections.Add(newSection);
                                }
                            }
                            //проверяем, закончились ли все источники данных
                            if(endedDataSources.Count == dataSourceGroup.DataSourceAccordances.Count)
                            {
                                isAllDataSourcesEnd = true;
                                //при переходе на новый файл создается новая секция, поэтому когда все файлы заканчиваются создается пустая секция, удаляем её сейчас
                                if (dataSourceGroupSegments.Sections.Any())
                                {
                                    dataSourceGroupSegments.Sections.RemoveAt(dataSourceGroupSegments.Sections.Count - 1);
                                }
                            }
                        }
                        testing.DataSourceGroupsSegments.Add(dataSourceGroupSegments);
                    }
                    
                    //вычисляем идеальную прибыль для каждого DataSourceCandles
                    foreach (DataSourceCandles dataSourceCandles in testing.DataSourcesCandles)
                    {
                        double pricesAmount = 0; //сумма разности цен закрытия, взятой по модулю
                        int fileIndex = 0;
                        int candleIndex = 0;
                        bool isOverFileIndex = false; //вышел ли какой-либо из индексов файлов за границы массива файлов источника данных
                        while (isOverFileIndex == false)
                        {
                            DateTime currentDateTime = dataSourceCandles.Candles[fileIndex][candleIndex].DateTime;
                            if (candleIndex > 0) //чтобы не обращаться к прошлой свечке при смене файла
                            {
                                currentDateTime = dataSourceCandles.Candles[fileIndex][candleIndex].DateTime;
                                pricesAmount += Math.Abs(dataSourceCandles.Candles[fileIndex][candleIndex].C - dataSourceCandles.Candles[fileIndex][candleIndex - 1].C); //прибавляем разность цен закрытия, взятую по модулю
                            }
                            //переходим на следующую свечку, пока не дойдем до даты которая позже текущей или пока не выйдем за пределы файлов
                            bool isOverDate = DateTime.Compare(dataSourceCandles.Candles[fileIndex][candleIndex].DateTime, currentDateTime) > 0; //дошли ли до даты которая позже текущей
                            while (isOverDate == false && isOverFileIndex == false)
                            {
                                candleIndex++;
                                //если массив со свечками файла подошел к концу, переходим на следующий файл
                                if (candleIndex >= dataSourceCandles.Candles[fileIndex].Length)
                                {
                                    fileIndex++;
                                    candleIndex = 0;
                                }
                                //если индекс файла не вышел за пределы массива, проверяем, дошли ли до даты которая позже текущей
                                if (fileIndex < dataSourceCandles.Candles.Length)
                                {
                                    isOverDate = DateTime.Compare(dataSourceCandles.Candles[fileIndex][candleIndex].DateTime, currentDateTime) > 0;
                                }
                                else
                                {
                                    isOverFileIndex = true;
                                }
                            }
                        }
                        dataSourceCandles.PerfectProfit = pricesAmount / dataSourceCandles.DataSource.PriceStep * dataSourceCandles.DataSource.CostPriceStep; //записываем идеальную прибыль
                    }


                    //выполняем тестирование для всех TestBatches
                    //определяем количество используемых потоков
                    int processorCount = Environment.ProcessorCount;
                    processorCount -= _modelData.Settings.Where(i => i.Id == 1).First().BoolValue ? 1 : 0; //если в настройках выбрано оставлять один поток, вычитаем из количества потоков
                    if (countTestRuns < processorCount) //если тестов меньше чем число доступных потоков, устанавливаем количество потоков на количество тестов, т.к. WaitAll ругается если задача в tasks null
                    {
                        processorCount = countTestRuns;
                    }
                    if (processorCount < 1)
                    {
                        processorCount = 1;
                    }
                    //processorCount = 1; //эту строку я использую если нужно проследить выполнение тестового прогона, чтобы не переключаться между другими потоками
                    Task[] tasks = new Task[processorCount]; //задачи
                    Stopwatch stopwatchTestRunsExecution = new Stopwatch();
                    stopwatchTestRunsExecution.Start();
                    int[][] tasksExecutingTestRuns = new int[processorCount][]; //массив, в котором хранится индекс testBatch-а (в 0-м индексе) и testRuna (из OptimizationTestRuns) (в 1-м индексе), который выполняется в задаче с таким же индексом в массиве задач (если это форвардный тест, в 1-м элементе будет OptimizationTestRuns.Count, если это форвардный тест с торговлей депозитом в 1-м элементе будет OptimizationTestRuns.Count + 1)
                    int[][] testRunsStatus = new int[testing.TestBatches.Count][]; //статусы выполненности testRun-ов в testBatch-ах. Первый индекс - индекс testBatch-а, второй - индекс testRun-a. У невыполненного значение 0, у запущенного 1, а у выполненного 2. При форвардном тестировании, в список со статусами выполненности оптимизационных тестов в конец добавляется еще один элемент - статус форвардного теста, при форвардном тестировании с торговлей депозитом - 2 элемента, статус форвардного теста и статус форвардного теста с торговлей депозитом
                    int[][] testRunsStatus2 = new int[testing.TestBatches.Count][]; //индексы потока в tasks в котором выполнялся testRun
                    //создаем для каждого testBatch массив равный количеству testRun
                    for (int k = 0; k < testing.TestBatches.Count; k++)
                    {
                        int forwardTestRunsCount = testing.IsForwardTesting ? 1 : 0; //количество форвардных тестов в данном TestBatch (при форвардном - 1, при форвардном и форвардном с торговлей депозитом - 2)
                        forwardTestRunsCount += testing.IsForwardTesting && testing.IsForwardDepositTrading ? 1 : 0;
                        testRunsStatus[k] = new int[testing.TestBatches[k].OptimizationTestRuns.Count + forwardTestRunsCount];
                        for (int y = 0; y < testRunsStatus[k].Length; y++) { testRunsStatus[k][y] = 0; } //заполняем статусы testRun нулями
                        testRunsStatus2[k] = new int[testing.TestBatches[k].OptimizationTestRuns.Count + forwardTestRunsCount];
                        for (int y = 0; y < testRunsStatus2[k].Length; y++) { testRunsStatus2[k][y] = -1; } //заполняем индексы задач в tasks -1
                    }
                    bool isAllTestRunsComplete = false; //выполнены ли все testRun-ы
                    int completedCount = 0; //количество выполненных testRun-ов
                    int n = 0; //номер задачи, нужен для начального заполнения массива tasks
                    DispatcherInvoke((Action)(() => {
                        _mainCommunicationChannel.TestingProgress.Clear();
                        _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 3/4:  Симуляция тестов", StepTasksCount = countTestRunWithForward, CompletedStepTasksCount = completedCount, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatchTestRunsExecution.Elapsed, CancelPossibility = true, IsFinishSimulation = false, IsSuccessSimulation = false, IsFinish = false });
                    }));
                    while (isAllTestRunsComplete == false)
                    {
                        if (tasks[tasks.Length - 1] == null) //если пока еще не заполнен массив с задачами, заполняем его
                        {
                            //находим первый testRun, который еще не запущен (имеет статус 0)
                            int testBatchIndx = 0;
                            int testRunIndx = 0;
                            bool isFindTestRun = false;
                            while (isFindTestRun == false)
                            {
                                if (testRunsStatus[testBatchIndx][testRunIndx] == 0)
                                {
                                    isFindTestRun = true;
                                }
                                else
                                {
                                    testRunIndx++;
                                    if (testRunIndx >= testing.TestBatches[testBatchIndx].OptimizationTestRuns.Count) //если индекс testRun >= количеству OptimizationTestRuns, переходим на следующий testBatch
                                    {
                                        testRunIndx = 0;
                                        testBatchIndx++;
                                    }
                                }
                            }

                            TestRun testRun = testing.TestBatches[testBatchIndx].OptimizationTestRuns[testRunIndx];
                            Task task = Task.Run(() => TestRunExecute(testRun, testing, testing.Algorithm.AlgorithmIndicators, cancellationToken));
                            tasks[n] = task;
                            tasksExecutingTestRuns[n] = new int[2] { testBatchIndx, testRunIndx }; //запоминаем индексы testBatch и testRun, который выполняется в текущей задачи (в элементе массива tasks с индексом n)
                            testRunsStatus[testBatchIndx][testRunIndx] = 1; //отмечаем что testRun имеет статус запущен
                            testRunsStatus2[testBatchIndx][testRunIndx] = n;
                            n++; //увеличиваем индекс задачи
                        }
                        else //иначе ждем и обрабатываем выполненные задачи
                        {
                            //int completedTaskIndex = Task.WaitAny(tasks); //ждем чтобы не все время в цикле со sleep ждать
                            bool isAnyComplete = false;
                            //ждем пока один из выполняющихся testRun-ов не будет выполнен
                            while (isAnyComplete == false)
                            {
                                Thread.Sleep(10);
                                int taskIndex = 0;
                                while (taskIndex < tasksExecutingTestRuns.Length && isAnyComplete == false) //проходим по всем задачам и смотрим на статусы выполненности testRun-ов
                                {
                                    //если в задаче находится testRun со статусом Запущен (если нет, то он уже выполнен, и не был заменен новым testRun-ом т.к. они закончились)
                                    if (testRunsStatus[tasksExecutingTestRuns[taskIndex][0]][tasksExecutingTestRuns[taskIndex][1]] == 1)
                                    {
                                        if (tasksExecutingTestRuns[taskIndex][1] < testing.TestBatches[0].OptimizationTestRuns.Count) //оптимизационный тест
                                        {
                                            if (testing.TestBatches[tasksExecutingTestRuns[taskIndex][0]].OptimizationTestRuns[tasksExecutingTestRuns[taskIndex][1]].IsComplete)
                                            {
                                                isAnyComplete = true;
                                            }
                                        }
                                        else if (tasksExecutingTestRuns[taskIndex][1] == testing.TestBatches[0].OptimizationTestRuns.Count) //форвардный тест
                                        {
                                            if (testing.TestBatches[tasksExecutingTestRuns[taskIndex][0]].ForwardTestRun.IsComplete)
                                            {
                                                isAnyComplete = true;
                                            }
                                        }
                                        else if (tasksExecutingTestRuns[taskIndex][1] == testing.TestBatches[0].OptimizationTestRuns.Count + 1) //форвардный тест с торговлей депозитом
                                        {
                                            if (testing.TestBatches[tasksExecutingTestRuns[taskIndex][0]].ForwardTestRunDepositTrading.IsComplete)
                                            {
                                                isAnyComplete = true;
                                            }
                                        }
                                    }
                                    taskIndex++;
                                }
                            }

                            if (cancellationToken.IsCancellationRequested) //если был запрос на отмену операции, прекращем функцию
                            {
                                Task.WaitAll(tasks);
                                TestingEnding(false, testing);
                                return;
                            }
                            DispatcherInvoke((Action)(() => {
                                _mainCommunicationChannel.TestingProgress.Clear();
                                _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 3/4:  Симуляция тестов", StepTasksCount = countTestRunWithForward, CompletedStepTasksCount = completedCount, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatchTestRunsExecution.Elapsed, CancelPossibility = true, IsFinishSimulation = false, IsSuccessSimulation = false, IsFinish = false });
                            }));

                            //обрабатываем выполненные testRun-ы
                            int taskIndex1 = 0;
                            while (taskIndex1 < tasksExecutingTestRuns.Length) //проходим по всем задачам и смотрим на статусы выполненности testRun-ов, у выполненных, которые имееют статус Запущен, отмечаем в статусе как выполнена
                            {
                                if (testRunsStatus[tasksExecutingTestRuns[taskIndex1][0]][tasksExecutingTestRuns[taskIndex1][1]] == 1) //если testRun в текущей задаче имеет статус Запущен
                                {
                                    if (tasksExecutingTestRuns[taskIndex1][1] < testing.TestBatches[0].OptimizationTestRuns.Count) //оптимизационный тест
                                    {
                                        if (testing.TestBatches[tasksExecutingTestRuns[taskIndex1][0]].OptimizationTestRuns[tasksExecutingTestRuns[taskIndex1][1]].IsComplete)
                                        {
                                            testRunsStatus[tasksExecutingTestRuns[taskIndex1][0]][tasksExecutingTestRuns[taskIndex1][1]] = 2; //отмечаем в статусе testRun-а что он выполнен
                                            completedCount++; //увеличиваем количество выполненных тестов на 1
                                                              //определяем, выполнены ли все оптимизационные тесты данного testBatch
                                            bool isOptimizationTestsComplete = true;
                                            int a = 0;
                                            while (isOptimizationTestsComplete && a < testing.TestBatches[tasksExecutingTestRuns[taskIndex1][0]].OptimizationTestRuns.Count)
                                            {
                                                if (testRunsStatus[tasksExecutingTestRuns[taskIndex1][0]][a] != 2)
                                                {
                                                    isOptimizationTestsComplete = false;
                                                }
                                                a++;
                                            }
                                            if (isOptimizationTestsComplete) //если все оптимизационные тесты данного testBatch выполнены и топ-модель еще не была определена, запускаем определение топ-модели и статистической значимости
                                            {
                                                //определяем топ-модель и статистичекую значимость
                                                TestBatch testBatch = testing.TestBatches[tasksExecutingTestRuns[taskIndex1][0]]; //tasksExecutingTestRuns[taskIndex1][0] - testBatchIndex
                                                TestBatchTopModelDetermining(testBatch, testing); //определяем топ-модель
                                                //если это форвардное тестирование, и топ-модель не найдена, отмечаем что форвардный тест имеет статус выполнен
                                                if (testing.IsForwardTesting && testBatch.IsTopModelWasFind == false)
                                                {
                                                    testRunsStatus[tasksExecutingTestRuns[taskIndex1][0]][testBatch.OptimizationTestRuns.Count] = 2; //отмечаем в статусе форвардного testRun-а что он выполнен, т.к. мы не можем провести форвардный тест без топ-модели
                                                    if (testing.IsForwardDepositTrading)
                                                    {
                                                        testRunsStatus[tasksExecutingTestRuns[taskIndex1][0]][testBatch.OptimizationTestRuns.Count + 1] = 2; //отмечаем в статусе форвардного testRun-а с торговлей депозитом что он выполнен, т.к. мы не можем провести форвардный тест без топ-модели
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (tasksExecutingTestRuns[taskIndex1][1] == testing.TestBatches[0].OptimizationTestRuns.Count) //форвардный тест
                                    {
                                        if (testing.TestBatches[tasksExecutingTestRuns[taskIndex1][0]].ForwardTestRun.IsComplete)
                                        {
                                            testRunsStatus[tasksExecutingTestRuns[taskIndex1][0]][tasksExecutingTestRuns[taskIndex1][1]] = 2; //отмечаем в статусе testRun-а что он выполнен
                                            completedCount++; //увеличиваем количество выполненных тестов на 1
                                        }
                                    }
                                    else if (tasksExecutingTestRuns[taskIndex1][1] == testing.TestBatches[0].OptimizationTestRuns.Count + 1) //форвардный тест с торговлей депозитом
                                    {
                                        if (testing.TestBatches[tasksExecutingTestRuns[taskIndex1][0]].ForwardTestRunDepositTrading.IsComplete)
                                        {
                                            testRunsStatus[tasksExecutingTestRuns[taskIndex1][0]][tasksExecutingTestRuns[taskIndex1][1]] = 2; //отмечаем в статусе testRun-а что он выполнен
                                            completedCount++; //увеличиваем количество выполненных тестов на 1
                                        }
                                    }
                                }
                                taskIndex1++;
                            }

                            //проходим по всем задачам, и для каждой завершенной, ищем невыполненный и незапущенный тест, и если нашли, то запускаем его в задаче
                            int indexTask = 0;
                            while (indexTask < tasksExecutingTestRuns.Length)
                            {
                                //определяем, имеет ли testRun в данной задаче статус Выполнен
                                if (testRunsStatus[tasksExecutingTestRuns[indexTask][0]][tasksExecutingTestRuns[indexTask][1]] == 2)
                                {
                                    //ищем невыполненный и незапущенный тест среди всех тестов
                                    TestRun testRun = new TestRun();
                                    bool isTestFind = false;
                                    int tBatchIndex = 0;
                                    int tRunIndex = 0;
                                    while (isTestFind == false && tBatchIndex < testing.TestBatches.Count)
                                    {
                                        //смотрим, определена ли топ-модель данного testBatch
                                        if (testing.TestBatches[tBatchIndex].IsTopModelDetermining)
                                        {
                                            if (testing.IsForwardTesting)
                                            {
                                                //смотрим, имеет ли форвардный тест статус Не выполнен
                                                if (testRunsStatus[tBatchIndex][testing.TestBatches[tBatchIndex].OptimizationTestRuns.Count] == 0) //если у форвардного теста статус Не выполнен, запускаем его в текущей задаче
                                                {
                                                    testRun = testing.TestBatches[tBatchIndex].ForwardTestRun;
                                                    isTestFind = true;
                                                    tRunIndex = testing.TestBatches[tBatchIndex].OptimizationTestRuns.Count; //запоминаем индекс, в который записать статус testRun
                                                }
                                            }
                                            //если форвардный тест не был выбран, смотрим форвардный тест с торговлей депозитом
                                            if (isTestFind == false)
                                            {
                                                if (testing.IsForwardTesting && testing.IsForwardDepositTrading)
                                                {
                                                    //смотрим, имеет ли форвардный тест с торговлей депозитом статус Не выполнен
                                                    if (testRunsStatus[tBatchIndex][testing.TestBatches[tBatchIndex].OptimizationTestRuns.Count + 1] == 0) //если у форвардного теста с торговлей депозитом статус Не выполнен, запускаем его в текущей задаче
                                                    {
                                                        testRun = testing.TestBatches[tBatchIndex].ForwardTestRunDepositTrading;
                                                        isTestFind = true;
                                                        tRunIndex = testing.TestBatches[tBatchIndex].OptimizationTestRuns.Count + 1; //запоминаем индекс, в который записать статус testRun
                                                    }
                                                }
                                            }
                                            //если не запущенный тест не был найден, переходим на следующий testBatch, т.к. все оптимизационные тесты выполненны, и форвардные запускать не нужно
                                            if (isTestFind == false)
                                            {
                                                tRunIndex = 0;
                                                tBatchIndex++;
                                            }
                                        }
                                        else //если не определена топ-модель, ищем не запущенный testRun среди оптимизационных тестов
                                        {
                                            while (isTestFind == false && tRunIndex < testing.TestBatches[tBatchIndex].OptimizationTestRuns.Count)
                                            {
                                                if (testRunsStatus[tBatchIndex][tRunIndex] == 0)
                                                {
                                                    testRun = testing.TestBatches[tBatchIndex].OptimizationTestRuns[tRunIndex];
                                                    isTestFind = true;
                                                }
                                                else
                                                {
                                                    tRunIndex++;
                                                }
                                            }
                                        }

                                        //если не нашли не запущенный тест, и tRunIndex превысил размер массива, переходим на следующий testBatch
                                        if (isTestFind == false && tRunIndex >= testing.TestBatches[0].OptimizationTestRuns.Count)
                                        {
                                            tRunIndex = 0;
                                            tBatchIndex++;
                                        }
                                    }
                                    //если нашли не запущенный тест, запускаем его в текущей задаче
                                    if (isTestFind)
                                    {
                                        Task task = Task.Run(() => TestRunExecute(testRun, testing, testing.Algorithm.AlgorithmIndicators, cancellationToken));
                                        tasks[indexTask] = task;
                                        tasksExecutingTestRuns[indexTask] = new int[2] { tBatchIndex, tRunIndex }; //запоминаем индексы testBatch и testRun, который выполняется в текущей задачи (в элементе массива tasks с индексом indexTask)
                                        testRunsStatus[tBatchIndex][tRunIndex] = 1; //отмечаем что testRun имеет статус запущен
                                        testRunsStatus2[tBatchIndex][tRunIndex] = indexTask; //запоминаем индекс task в котором выполняется данный testRun
                                    }
                                }
                                indexTask++;
                            }
                            //смотрим на статусы testRun-ов в задачах, и если нет ни одного со статусом Запущен, значит все testRun-ы выполнены, тестирование окончено
                            bool isAnyLaunched = false;
                            for (int i = 0; i < tasksExecutingTestRuns.Length; i++)
                            {
                                if (testRunsStatus[tasksExecutingTestRuns[i][0]][tasksExecutingTestRuns[i][1]] == 1)
                                {
                                    isAnyLaunched = true;
                                }
                            }
                            if (isAnyLaunched == false)
                            {
                                isAllTestRunsComplete = true; //если ни один из testRun-ов в задачах не имеет статус Запущен, отмечаем что тестирование окончено
                            }
                        }
                    }
                    stopwatchTestRunsExecution.Stop();
                    TestingEnding(true, testing);
                }
                else //если количество testRun-ов == 0, оповещаем пользователя и завершаем тестирование
                {
                    DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Тестирование не было выполнено, т.к. количество тестов равно нулю."); }));
                    TestingEnding(false, testing);
                }
            }
            else //если были ошибки при компиляции, завершаем тестирование
            {
                TestingEnding(false, testing);
            }
        }

        /*private void TestRunExecute(TestRun testRun, Testing testing, List<AlgorithmIndicator> algorithmIndicators, CancellationToken cancellationToken)
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            //формируем массивы с int и double значениями параметров для алгоритма
            int[] algorithmParametersIntValues = new int[testRun.AlgorithmParameterValues.Count];
            double[] algorithmParametersDoubleValues = new double[testRun.AlgorithmParameterValues.Count];
            for (int i = 0; i < testRun.AlgorithmParameterValues.Count; i++)
            {
                algorithmParametersIntValues[i] = testRun.AlgorithmParameterValues[i].IntValue;
                algorithmParametersDoubleValues[i] = testRun.AlgorithmParameterValues[i].DoubleValue;
            }
            
            
            //определяем индексы элемента каталога в AlgorithmIndicatorCatalog со значениями индикатора для всех индикаторов во всех источниках данных
            int[][] algorithmIndicatorCatalogElementIndexes = new int[testing.DataSourcesCandles.Count][];  //индексы элемента каталога в AlgorithmIndicatorCatalog со значениями индикатора для всех индикаторов во всех источниках данных
            //algorithmIndicatorCatalogElementIndexes[0] - соответствует источнику данных testing.DataSourcesCandles[0]
            //algorithmIndicatorCatalogElementIndexes[0][0] - соответствует индикатору testing.DataSourcesCandles[0].AlgorithmIndicatorCatalogs[0]. И содержит индекс элемента каталога со значениями для даннного индикатора
            for (int i = 0; i < testing.DataSourcesCandles.Count; i++)
            {
                if(testRun.TestBatch.DataSourceGroup.DataSourceAccordances.Where(a => a.DataSource.Id == testing.DataSourcesCandles[i].DataSource.Id).Any()) //если в источниках данных тестового прогона имеется источник данных текущего testing.DataSourcesCandles[i]
                {
                    algorithmIndicatorCatalogElementIndexes[i] = new int[testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs.Length]; //индексы для всех индикаторов в данном источнике данных
                    for (int k = 0; k < algorithmIndicatorCatalogElementIndexes[i].Length; k++)
                    {
                        //определяем индекс элемента каталога с текущей комбинацией значений параметров алгоритма
                        bool isFind = false;
                        int catalogElementIndex = 0;
                        while (isFind == false && catalogElementIndex < testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs[k].AlgorithmIndicatorCatalogElements.Count)
                        {
                            bool isAllParameterValuesEqual = true; //совпадают ли все значения параметров алгоритма со значениями в элементе каталога
                                                                   //проходим по всем занчениями параметров алгоритма в элементе каталога
                            foreach (AlgorithmParameterValue algorithmParameterValue in testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs[k].AlgorithmIndicatorCatalogElements[catalogElementIndex].AlgorithmParameterValues)
                            {
                                AlgorithmParameterValue algorithmParameterValueTestRun = testRun.AlgorithmParameterValues.Where(j => j.AlgorithmParameter.Id == algorithmParameterValue.AlgorithmParameter.Id).First(); //значение параметра алгоритма с таким же параметром алгоритма как и текущий параметр из элемента каталога
                                if (algorithmParameterValue.AlgorithmParameter.ParameterValueType.Id == 1) //параметр типа int
                                {
                                    if (algorithmParameterValue.IntValue != algorithmParameterValueTestRun.IntValue)
                                    {
                                        isAllParameterValuesEqual = false;
                                    }
                                }
                                else //параметр типа double
                                {
                                    if (algorithmParameterValue.DoubleValue != algorithmParameterValueTestRun.DoubleValue)
                                    {
                                        isAllParameterValuesEqual = false;
                                    }
                                }
                            }
                            if (isAllParameterValuesEqual)
                            {
                                algorithmIndicatorCatalogElementIndexes[i][k] = catalogElementIndex; //запоминаем индекс элемента каталога со значенями индикатора
                                isFind = true;
                            }
                            catalogElementIndex++;
                        }
                    }
                }
            }

            //получаем сегменты для группы источников данных как у текущего testRun
            DataSourceGroupSegments dataSourceGroupSegments = new DataSourceGroupSegments();
            bool isDataSourceGroupSegmentsFind = false;
            int dataSourceGroupsSegmentsIndex = 0;
            while (isDataSourceGroupSegmentsFind == false)
            {
                bool IsAllDataSourcesFind = true;
                foreach(DataSourceAccordance dataSourceAccordance in testRun.TestBatch.DataSourceGroup.DataSourceAccordances)
                {
                    if(testing.DataSourceGroupsSegments[dataSourceGroupsSegmentsIndex].DataSourceGroup.DataSourceAccordances.Where(a => a.DataSource.Id == dataSourceAccordance.DataSource.Id).Any() == false) //если текущий источник данных в testRun не имеется в DataSourceGroupsSegments
                    {
                        IsAllDataSourcesFind = false;
                    }
                }
                if (IsAllDataSourcesFind)
                {
                    dataSourceGroupSegments = testing.DataSourceGroupsSegments[dataSourceGroupsSegmentsIndex];
                    isDataSourceGroupSegmentsFind = true;
                }
                else
                {
                    dataSourceGroupsSegmentsIndex++;
                }
            }
            List<Segment> segments = dataSourceGroupSegments.Segments;
            int segmentIndex = 0;

            //доходим до индекса сегмента с датой, которая равна или позже даты начала тестирования
            while (DateTime.Compare( testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[0].DataSourceCandlesIndex].Candles[segments[segmentIndex].CandleIndexes[0].FileIndex][segments[segmentIndex].CandleIndexes[0].IndexCandle].DateTime, testRun.StartPeriod) < 0)
            {
                segmentIndex++;
            }

            //проходим по всем свечкам источников данных, пока не достигнем времени окончания теста, не выйдем за границы имеющихся файлов, или не получим запрос на отмену тестирования
            //while (DateTime.Compare(currentDateTime, testRun.EndPeriod) < 0 && isOverFileIndex == false && cancellationToken.IsCancellationRequested == false)
            //проходим по всем свечкам, пока не достигнем времени окончания теста, не выйдем за границы последнего торгового сегмента, или не получим запрос на отмену тестирования
            bool isDateTimeEnd = false; //дошла ли дата до даты окончания теста
            while (isDateTimeEnd == false && segmentIndex <= dataSourceGroupSegments.LastTradeSegmentIndex && cancellationToken.IsCancellationRequested == false)
            {
                //обрабатываем текущие заявки (только тех источников данных, текущие свечки которых равняются текущей дате)
                //формируем список источников данных для которых будут проверяться заявки на исполнение (те, даты которых равняются текущей дате)
                List<DataSource> approvedDataSources = new List<DataSource>();
                foreach(CandleIndex candleIndex in segments[segmentIndex].CandleIndexes)
                {
                    approvedDataSources.Add(testing.DataSourcesCandles[candleIndex.DataSourceCandlesIndex].DataSource);
                }
                if(testRun.Number == 101 && testRun.Account.Orders.Where(a => a.Number == 21).Any() && testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[0].DataSourceCandlesIndex].Candles[segments[segmentIndex].CandleIndexes[0].FileIndex][segments[segmentIndex].CandleIndexes[0].IndexCandle].DateTime.Hour == 17 && testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[0].DataSourceCandlesIndex].Candles[segments[segmentIndex].CandleIndexes[0].FileIndex][segments[segmentIndex].CandleIndexes[0].IndexCandle].DateTime.Minute == 43)
                {
                    int y = 0;
                }
                //проверяем заявки на исполнение
                bool isWereDeals = CheckOrdersExecution(testing, testRun.Account, approvedDataSources, segments[segmentIndex], true, true, true); //были ли совершены сделки при проверке исполнения заявок
                //проверяем, для всех источников данных имеются свечки в текущем сегменте
                bool isAllDataSourcesInSegment = true; //все ли источники данных имеют свечки в текущем сегменте
                if(segments[segmentIndex].CandleIndexes.Count != testRun.TestBatch.DataSourceGroup.DataSourceAccordances.Count)
                {
                    isAllDataSourcesInSegment = false;
                }
                //если свечки всех источников данных равняются текущей дате, вычисляем индикаторы и алгоритм
                if (isAllDataSourcesInSegment)
                {
                    //если были совершены сделки на текущей свечке, дважды выполняем алгоритм: первый раз обновляем заявки и проверяем на исполнение стоп-заявки (если была открыта позиция на текущей свечке, нужно выставить стоп и проверить мог ли он на этой же свечке исполнится), и если были сделки то выполняем алгоритм еще раз и обновляем заявки, после чего переходим на следующую свечку

                    bool IsOverIndex = false; //было ли превышение индекса в индикаторах и алгоритме
                    double[][] indicatorsValues = new double[testRun.TestBatch.DataSourceGroup.DataSourceAccordances.Count][];
                    //проходим по всем источникам данных и формируем значения всех индикаторов для каждого источника данных
                    for (int i = 0; i < segments[segmentIndex].CandleIndexes.Count; i++)
                    {
                        indicatorsValues[i] = new double[algorithmIndicators.Count];
                        for (int k = 0; k < indicatorsValues[i].Length; k++)
                        {
                            indicatorsValues[i][k] = testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[i].DataSourceCandlesIndex].AlgorithmIndicatorCatalogs[k].AlgorithmIndicatorCatalogElements[algorithmIndicatorCatalogElementIndexes[i][k]].AlgorithmIndicatorValues.Values[segments[segmentIndex].CandleIndexes[i].FileIndex][segments[segmentIndex].CandleIndexes[i].IndexCandle].Value;
                            if(testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[i].DataSourceCandlesIndex].AlgorithmIndicatorCatalogs[k].AlgorithmIndicatorCatalogElements[algorithmIndicatorCatalogElementIndexes[i][k]].AlgorithmIndicatorValues.Values[segments[segmentIndex].CandleIndexes[i].FileIndex][segments[segmentIndex].CandleIndexes[i].IndexCandle].IsNotOverIndex == false) //если при вычислении данного значения индикатора было превышение индекса свечки, отмечаем что было превышение индекса
                            {
                                IsOverIndex = true;
                            }
                        }
                    }

                    //если были сделки на этой свечке, то для того чтобы проверить мог ли исполниться стоп-лосс на текущей свечке, выполняем алгоритм (после чего для открытой позиции будет выставлен стоп-лосс) и проверяем исполнение стоп-заявок. Если в процессе выполнения стоп-заявок были совершены сделки, еще раз выполняем алгоритм, обновляем заявки и переходим на следующую свечку
                    int iteration = 0; //номер итерации
                    bool isWereDealsStopLoss = false; //были ли совешены сделки при проверки стоп-заявок на исполнение
                    do
                    {
                        iteration++;
                        //вычисляем алгоритм
                        //формируем dataSourcesForCalculate
                        DataSourceForCalculate[] dataSourcesForCalculate = new DataSourceForCalculate[segments[segmentIndex].CandleIndexes.Count];
                        for (int i = 0; i < segments[segmentIndex].CandleIndexes.Count; i++)
                        {
                            //определяем среднюю цену и объем позиции
                            double averagePricePosition = 0; //средняя цена позиции
                            decimal volumePosition = 0; //объем позиции
                            bool isBuyDirection = false;
                            foreach (Deal deal in testRun.Account.CurrentPosition)
                            {
                                if (deal.DataSource == testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[i].DataSourceCandlesIndex].DataSource) //если сделка относится к текущему источнику данных
                                {
                                    if (volumePosition == 0) //если это первая сделка по данному источнику данных, запоминаем цену и объем
                                    {
                                        averagePricePosition = deal.Price;
                                        volumePosition = deal.Count;
                                    }
                                    else //если это не первая сделка по данному источнику данных, определяем среднюю цену и обновляем объем
                                    {
                                        averagePricePosition = (double)(((decimal)averagePricePosition * volumePosition + (decimal)deal.Price * deal.Count) / (volumePosition + deal.Count)); //(средняя цена * объем средней цены + текущая цена * текущий объем)/(объем средней цены + текущий объем)
                                        averagePricePosition = ModelFunctions.RoundToIncrement(averagePricePosition, deal.DataSource.PriceStep); //округляем среднюю цену позиции до шага 1 пункта цены данного инструмента
                                        volumePosition += deal.Count;
                                    }
                                    if (deal.Order.Direction)
                                    {
                                        isBuyDirection = true;
                                    }
                                }
                            }
                            DataSourceFileWorkingPeriod dataSourceFileWorkingPeriod = testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[i].DataSourceCandlesIndex].DataSource.DataSourceFiles[segments[segmentIndex].CandleIndexes[i].FileIndex].DataSourceFileWorkingPeriods.Where(j => DateTime.Compare(testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[0].DataSourceCandlesIndex].Candles[segments[segmentIndex].CandleIndexes[0].FileIndex][segments[segmentIndex].CandleIndexes[0].IndexCandle].DateTime, j.StartPeriod) >= 0).Last(); //последний период, дата начала которого раньше или равняется текущей дате

                            dataSourcesForCalculate[i] = new DataSourceForCalculate();
                            dataSourcesForCalculate[i].idDataSource = testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[i].DataSourceCandlesIndex].DataSource.Id;
                            dataSourcesForCalculate[i].IndicatorsValues = indicatorsValues[i];
                            dataSourcesForCalculate[i].PriceStep = testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[i].DataSourceCandlesIndex].DataSource.PriceStep;
                            dataSourcesForCalculate[i].CostPriceStep = testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[i].DataSourceCandlesIndex].DataSource.CostPriceStep;
                            dataSourcesForCalculate[i].OneLotCost = testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[i].DataSourceCandlesIndex].DataSource.Instrument.Id == 2 ? testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[i].DataSourceCandlesIndex].DataSource.Cost : testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[i].DataSourceCandlesIndex].Candles[segments[segmentIndex].CandleIndexes[0].FileIndex][segments[segmentIndex].CandleIndexes[0].IndexCandle].C;
                            dataSourcesForCalculate[i].Price = averagePricePosition;
                            dataSourcesForCalculate[i].CountBuy = isBuyDirection ? volumePosition : 0;
                            dataSourcesForCalculate[i].CountSell = isBuyDirection ? 0 : volumePosition;
                            dataSourcesForCalculate[i].TimeInCandle = testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[i].DataSourceCandlesIndex].DataSource.Interval.Duration;
                            dataSourcesForCalculate[i].TradingStartTimeOfDay = dataSourceFileWorkingPeriod.TradingStartTime;
                            dataSourcesForCalculate[i].TradingEndTimeOfDay = dataSourceFileWorkingPeriod.TradingEndTime;
                            dataSourcesForCalculate[i].Candles = testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[i].DataSourceCandlesIndex].Candles[segments[segmentIndex].CandleIndexes[0].FileIndex];
                            dataSourcesForCalculate[i].CurrentCandleIndex = segments[segmentIndex].CandleIndexes[0].IndexCandle;
                        }

                        AccountForCalculate accountForCalculate = new AccountForCalculate { FreeRubleMoney = testRun.Account.FreeForwardDepositCurrencies.Where(j => j.Currency.Id == 1).First().Deposit, FreeDollarMoney = testRun.Account.FreeForwardDepositCurrencies.Where(j => j.Currency.Id == 2).First().Deposit, TakenRubleMoney = testRun.Account.TakenForwardDepositCurrencies.Where(j => j.Currency.Id == 1).First().Deposit, TakenDollarMoney = testRun.Account.TakenForwardDepositCurrencies.Where(j => j.Currency.Id == 2).First().Deposit, IsForwardDepositTrading = testRun.Account.IsForwardDepositTrading };
                        //копируем объект скомпилированного алгоритма, чтобы из разных потоков не обращаться к одному объекту и к одним свойствам объекта
                        dynamic CompiledAlgorithmCopy = testing.CompiledAlgorithm.Clone();
                        AlgorithmCalculateResult algorithmCalculateResult = CompiledAlgorithmCopy.Calculate(accountForCalculate, dataSourcesForCalculate, algorithmParametersIntValues, algorithmParametersDoubleValues);

                        if (IsOverIndex == false) //если не был превышен допустимый индекс при вычислении индикаторов и алгоритма, обрабатываем заявки
                        {
                            //удаляем заявки, количество лотов в которых равно 0
                            for(int i = algorithmCalculateResult.Orders.Count - 1; i >= 0; i--)
                            {
                                if(algorithmCalculateResult.Orders[i].Count == 0)
                                {
                                    algorithmCalculateResult.Orders.RemoveAt(i);
                                }
                            }
                            //если это не форвардное тестирование с торговлей депозитом, устанавливаем размер заявок в 1 лот, а так же устанавливаем DateTimeSubmit для заявок
                            foreach (Order order in algorithmCalculateResult.Orders)
                            {
                                if (testRun.Account.IsForwardDepositTrading == false)
                                {
                                    order.Count = 1;
                                    order.StartCount = 1;
                                }
                                order.DateTimeSubmit = testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[0].DataSourceCandlesIndex].Candles[segments[segmentIndex].CandleIndexes[0].FileIndex][segments[segmentIndex].CandleIndexes[0].IndexCandle].DateTime;
                            }
                            //приводим заявки к виду который прислал пользователь в алгоритме
                            //самое самое новое
                            List<Order> accountOrders = new List<Order>(); //список с текущими выставленными заявками
                            accountOrders.AddRange(testRun.Account.Orders);

                            List<Order> userOrders = new List<Order>(); //список с заявками пользователя
                            userOrders.AddRange(algorithmCalculateResult.Orders);

                            List<Order> newOrders = new List<Order>(); //список с новыми выствленными заявками
                            newOrders.AddRange(algorithmCalculateResult.Orders);

                            //обрабатываем все заявки в accountOrders
                            int countAccountOrders = accountOrders.Count;
                            while (countAccountOrders > 0)
                            {
                                Order accountOrder = accountOrders[0]; //текущая заявка из accountOrders
                                Order userOrder = null; //совпадающая с accountOrder заявка из userOrders
                                //ищем в userOrders совпадающую с accountOrder заявку
                                int userOrderIndex = 0;
                                while (userOrderIndex < userOrders.Count && userOrder == null)
                                {
                                    bool isEqual = accountOrder.DataSource == userOrders[userOrderIndex].DataSource && accountOrder.TypeOrder == userOrders[userOrderIndex].TypeOrder && accountOrder.Direction == userOrders[userOrderIndex].Direction && accountOrder.Price == userOrders[userOrderIndex].Price && accountOrder.Count == userOrders[userOrderIndex].Count; //проверка на соответстве источника данных, типа заявки, направления, цены, количества
                                    isEqual = isEqual && ((accountOrder.LinkedOrder != null && userOrders[userOrderIndex].LinkedOrder != null) || (accountOrder.LinkedOrder == null && userOrders[userOrderIndex].LinkedOrder == null)); //проверка на соответствие наличия/отсутствия связаной заявки
                                    if (isEqual)
                                    {
                                        userOrder = userOrders[userOrderIndex]; //запоминаем совпадающую с accountOrder заявку
                                    }
                                    else
                                    {
                                        userOrderIndex++; //увеличиваем индекс заявок пользователя
                                    }
                                }
                                //если в userOrders есть совпадающая, удаляем совпадающую из userOrders и newOrders, и вставляем в newOrders из accountOrders
                                if(userOrder != null)
                                {
                                    userOrders.Remove(userOrder);
                                    newOrders.Remove(userOrder);
                                    newOrders.Add(accountOrder);
                                    //если у accountOrder есть связанная заявка, сравниваем accountOrder.LinkedOrder и userOrder.LinkedOrder
                                    if (accountOrder.LinkedOrder != null)
                                    {
                                        bool isEqual = accountOrder.LinkedOrder.DataSource == userOrder.LinkedOrder.DataSource && accountOrder.LinkedOrder.TypeOrder == userOrder.LinkedOrder.TypeOrder && accountOrder.LinkedOrder.Direction == userOrder.LinkedOrder.Direction && accountOrder.LinkedOrder.Price == userOrder.LinkedOrder.Price && accountOrder.LinkedOrder.Count == userOrder.LinkedOrder.Count; //проверка на соответстве источника данных, типа заявки, направления, цены, количества
                                        if (isEqual)
                                        {
                                            //если совпадают удаляем из userOrders и newOrders userOrder.LinkedOrder, вставляем в newOrders accountOrder.LinkedOrder, удаляем из accountOrders accountOrder.LinkedOrder
                                            userOrders.Remove(userOrder.LinkedOrder);
                                            newOrders.Remove(userOrder.LinkedOrder);
                                            newOrders.Add(accountOrder.LinkedOrder);
                                            accountOrders.Remove(accountOrder.LinkedOrder);
                                        }
                                        else
                                        {
                                            //если не совпадают, значит свзяанная с accountOrder будет взята из userOrder, и нужно проставить связи между ними (т.к. userOrder.LinkedOrder уже имеется, а accountOrder только что добавлена)
                                            accountOrder.LinkedOrder.DateTimeRemove = testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[0].DataSourceCandlesIndex].Candles[segments[segmentIndex].CandleIndexes[0].FileIndex][segments[segmentIndex].CandleIndexes[0].IndexCandle].DateTime; //т.к. accountOrder.LinkedOrder не совпадает с userOrder.LinkedOrder, accountOrder.LinkedOrder снята, и нужно установить дату снятия
                                            accountOrder.LinkedOrder = userOrder.LinkedOrder;
                                            accountOrder.LinkedOrder.LinkedOrder = accountOrder;
                                        }
                                    }
                                }
                                else
                                {
                                    //т.к. в userOrders не была найдена такая заявка, она снята, и нужно установить дату снятия
                                    accountOrder.DateTimeRemove = testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[0].DataSourceCandlesIndex].Candles[segments[segmentIndex].CandleIndexes[0].FileIndex][segments[segmentIndex].CandleIndexes[0].IndexCandle].DateTime;
                                }
                                //удаляем из accountOrders accountOrder, т.к. мы её обработали
                                accountOrders.Remove(accountOrder);
                                countAccountOrders = accountOrders.Count;
                            }

                            //проставляем номера новым заявкам и добавляем их в testRun.Account.AllOrders
                            int lastNumber = testRun.Account.AllOrders.Count == 0 ? 0 : testRun.Account.AllOrders.Last().Number; //номер последней заявки
                            foreach (Order order in newOrders)
                            {
                                if(order.Number == 0) //если номер заявки равен 0, значит она новая
                                {
                                    lastNumber++;
                                    order.Number = lastNumber;
                                    testRun.Account.AllOrders.Add(order);
                                }
                            }
                            //устанавливаем текущие выставленные заявки в newOrders
                            testRun.Account.Orders = newOrders;

                            //если на текущей свечке были совершены сделки, проверяем стоп-заявки на исполнение (чтобы если на текущей свечке была открыта позиция, после выставления стоп-заявки проверить её на исполнение на текущей свечке)
                            if (isWereDeals && iteration == 1)
                            {
                                isWereDealsStopLoss = CheckOrdersExecution(testing, testRun.Account, approvedDataSources, segments[segmentIndex], false, true, false); //были ли совершены сделки при проверке исполнения стоп-заявок
                            }
                        }
                    }
                    while (isWereDealsStopLoss && iteration == 1); //если этой первое исполнение алгоритма, и при проверке стоп-заявок были сделки, еще раз прогоняем алгоритм чтобы обновить заявки
                }

                //переходим на следующий сегмент, у которого дата находится в настоящем
                bool isSegmentInPresent = true; //находится ли сегмент в настоящем
                do
                {
                    segmentIndex++;
                    if (segmentIndex <= dataSourceGroupSegments.LastTradeSegmentIndex) //если индекс сегмента не вышел за пределы
                    {
                        isSegmentInPresent = segments[segmentIndex].Section.IsPresent;
                        isDateTimeEnd = DateTime.Compare(testing.DataSourcesCandles[segments[segmentIndex].CandleIndexes[0].DataSourceCandlesIndex].Candles[segments[segmentIndex].CandleIndexes[0].FileIndex][segments[segmentIndex].CandleIndexes[0].IndexCandle].DateTime, testRun.EndPeriod) >= 0; //если дата сегмента больше или равна дате окончания теста, отмечаем это
                    }
                }
                while (isSegmentInPresent == false && isDateTimeEnd == false && segmentIndex <= dataSourceGroupSegments.LastTradeSegmentIndex);
            }

            //устанавливаем значение маржи
            testRun.Account.Margin = ModelFunctions.MarginCalculate(testRun);

            DataSourceCandles[] dataSourceCandles = new DataSourceCandles[testRun.TestBatch.DataSourceGroup.DataSourceAccordances.Count]; //массив с ссылками на DataSourceCandles, соответствующими источникам данных группы источников данных
            for (int i = 0; i < dataSourceCandles.Length; i++)
            {
                dataSourceCandles[i] = testing.DataSourcesCandles.Where(j => j.DataSource == testRun.TestBatch.DataSourceGroup.DataSourceAccordances[i].DataSource).First(); //DataSourceCandles для источника данных из DataSourceAccordances с индексом i в dataSourceCandles
            }

            //рассчитываем критерии оценки для данного testRun
            for (int i = 0; i < _modelData.EvaluationCriterias.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break; //если был запрос на отмену тестирования, завершаем цикл
                }
                //определяем индекс источника данных, с наибольшей идеальной прибылью
                /*int index = 0;
                for(int k = 1; k < dataSourceCandles.Length; k++)
                {
                    if(dataSourceCandles[index].PerfectProfit < dataSourceCandles[k].PerfectProfit)
                    {
                        index = k;
                    }
                }*/
        /*public EvaluationCriteriaValue Calculate(List<DataSourceCandles> dataSourcesCandles, TestRun testRun, ObservableCollection<Setting> settings)
        {
            double ResultDoubleValue = 0;
            string ResultStringValue = """";
            " + script + @"
            return new EvaluationCriteriaValue { DoubleValue = ResultDoubleValue, StringValue = ResultStringValue };
        }*/
        /*

        //ModelFunctions.TestEvaluationCriteria(testRun); //так я отлаживаю критерии оценки

        //копируем объект скомпилированного критерия оценки, чтобы из разных потоков не обращаться к одному объекту и к одним свойствам объекта
        dynamic CompiledEvaluationCriteriaCopy = testing.CompiledEvaluationCriterias[i].Clone();
        EvaluationCriteriaValue evaluationCriteriaValue = CompiledEvaluationCriteriaCopy.Calculate(dataSourceCandles, testRun, _modelData.Settings);
        evaluationCriteriaValue.EvaluationCriteria = _modelData.EvaluationCriterias[i];
        testRun.EvaluationCriteriaValues.Add(evaluationCriteriaValue);
    }
    testRun.IsComplete = true;
}*/

        private void TestRunExecute(TestRun testRun, Testing testing, List<AlgorithmIndicator> algorithmIndicators, CancellationToken cancellationToken)
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            //формируем массивы с int и double значениями параметров для каждого индикатора
            int[][] indicatorParametersIntValues = new int[algorithmIndicators.Count][];
            double[][] indicatorParametersDoubleValues = new double[algorithmIndicators.Count][];
            //новая версия
            for (int i = 0; i < algorithmIndicators.Count; i++)
            {
                indicatorParametersIntValues[i] = new int[algorithmIndicators[i].IndicatorParameterRanges.Count];
                indicatorParametersDoubleValues[i] = new double[algorithmIndicators[i].IndicatorParameterRanges.Count];
                for (int k = 0; k < algorithmIndicators[i].IndicatorParameterRanges.Count; k++)
                {
                    indicatorParametersIntValues[i][k] = testRun.AlgorithmParameterValues.Where(j => j.AlgorithmParameter.Id == algorithmIndicators[i].IndicatorParameterRanges[k].AlgorithmParameter.Id).First().IntValue;
                    indicatorParametersDoubleValues[i][k] = testRun.AlgorithmParameterValues.Where(j => j.AlgorithmParameter.Id == algorithmIndicators[i].IndicatorParameterRanges[k].AlgorithmParameter.Id).First().DoubleValue;
                }
            }

            //формируем массивы с int и double значениями параметров для алгоритма
            int[] algorithmParametersIntValues = new int[testRun.AlgorithmParameterValues.Count];
            double[] algorithmParametersDoubleValues = new double[testRun.AlgorithmParameterValues.Count];
            for (int i = 0; i < testRun.AlgorithmParameterValues.Count; i++)
            {
                algorithmParametersIntValues[i] = testRun.AlgorithmParameterValues[i].IntValue;
                algorithmParametersDoubleValues[i] = testRun.AlgorithmParameterValues[i].DoubleValue;
            }

            TimeSpan intervalDuration = testRun.TestBatch.DataSourceGroup.DataSourceAccordances[0].DataSource.Interval.Duration; //длительность интервала
            DataSourceCandles[] dataSourceCandles = new DataSourceCandles[testRun.TestBatch.DataSourceGroup.DataSourceAccordances.Count]; //массив с ссылками на DataSourceCandles, соответствующими источникам данных группы источников данных
            for (int i = 0; i < dataSourceCandles.Length; i++)
            {
                dataSourceCandles[i] = testing.DataSourcesCandles.Where(j => j.DataSource == testRun.TestBatch.DataSourceGroup.DataSourceAccordances[i].DataSource).First(); //DataSourceCandles для источника данных из DataSourceAccordances с индексом i в dataSourceCandles
            }
            int[] fileIndexes = new int[testRun.TestBatch.DataSourceGroup.DataSourceAccordances.Count]; //индексы (для всех источников данных группы) элемента массива Candle[][] Candles в DataSourcesCandles, соответствующий файлу источника данных
            int[] candleIndexes = new int[testRun.TestBatch.DataSourceGroup.DataSourceAccordances.Count]; //индексы (для всех источников данных группы) элемента массива Candles[], сответствующий свечке

            //определяем индексы элемента каталога в AlgorithmIndicatorCatalog со значениями индикатора для всех индикаторов во всех источниках данных
            int[][] algorithmIndicatorCatalogElementIndexes = new int[dataSourceCandles.Length][]; //индексы элемента каталога в AlgorithmIndicatorCatalog со значениями индикатора для всех индикаторов во всех источниках данных
            //algorithmIndicatorCatalogElementIndexes[0] - соответствует источнику данных dataSourceCandles[0]
            //algorithmIndicatorCatalogElementIndexes[0][0] - соответствует индикатору dataSourceCandles[0].AlgorithmIndicatorCatalogs[0]. И содержит индекс элемента каталога со значениями для даннного индикатора
            for (int i = 0; i < dataSourceCandles.Length; i++)
            {
                algorithmIndicatorCatalogElementIndexes[i] = new int[dataSourceCandles[i].AlgorithmIndicatorCatalogs.Length]; //индексы для всех индикаторов в данном источнике данных
                for (int k = 0; k < algorithmIndicatorCatalogElementIndexes[i].Length; k++)
                {
                    //определяем индекс элемента каталога с текущей комбинацией значений параметров алгоритма
                    bool isFind = false;
                    int catalogElementIndex = 0;
                    while (isFind == false && catalogElementIndex < dataSourceCandles[i].AlgorithmIndicatorCatalogs[k].AlgorithmIndicatorCatalogElements.Count)
                    {
                        bool isAllParameterValuesEqual = true; //совпадают ли все значения параметров алгоритма со значениями в элементе каталога
                        //проходим по всем занчениями параметров алгоритма в элементе каталога
                        foreach (AlgorithmParameterValue algorithmParameterValue in dataSourceCandles[i].AlgorithmIndicatorCatalogs[k].AlgorithmIndicatorCatalogElements[catalogElementIndex].AlgorithmParameterValues)
                        {
                            AlgorithmParameterValue algorithmParameterValueTestRun = testRun.AlgorithmParameterValues.Where(j => j.AlgorithmParameter.Id == algorithmParameterValue.AlgorithmParameter.Id).First(); //значение параметра алгоритма с таким же параметром алгоритма как и текущий параметр из элемента каталога
                            if (algorithmParameterValue.AlgorithmParameter.ParameterValueType.Id == 1) //параметр типа int
                            {
                                if (algorithmParameterValue.IntValue != algorithmParameterValueTestRun.IntValue)
                                {
                                    isAllParameterValuesEqual = false;
                                }
                            }
                            else //параметр типа double
                            {
                                if (algorithmParameterValue.DoubleValue != algorithmParameterValueTestRun.DoubleValue)
                                {
                                    isAllParameterValuesEqual = false;
                                }
                            }
                        }
                        if (isAllParameterValuesEqual)
                        {
                            algorithmIndicatorCatalogElementIndexes[i][k] = catalogElementIndex; //запоминаем индекс элемента каталога со значенями индикатора
                            isFind = true;
                        }
                        catalogElementIndex++;
                    }
                }
            }

            //устанавливаем начальные индексы файла и свечки для источников данных
            for (int i = 0; i < fileIndexes.Length; i++)
            {
                fileIndexes[i] = 0;
                candleIndexes[i] = 0;
            }

            //находим индексы файлов и свечек, дата и время которых позже или равняется дате и времени начала тестирования
            for (int i = 0; i < dataSourceCandles.Length; i++)
            {
                //находим индекс файла в текущем dataSourceCandles, дата последней свечки которого позже даты начала тестирования
                int fileIndex = 0;
                bool isFindFile = false;
                //пока не вышли за пределы массива файлов, и пока не нашли файл, дата последней свечки которого позже даты начала тестирования
                while (fileIndex < dataSourceCandles[i].Candles.Length && isFindFile == false)
                {
                    if (DateTime.Compare(dataSourceCandles[i].Candles[fileIndex].Last().DateTime, testRun.StartPeriod) > 0)
                    {
                        isFindFile = true;
                    }
                    else
                    {
                        fileIndex++;
                    }
                }
                fileIndexes[i] = fileIndex;
                //если нашли файл, последняя свечка которого позже даты начала тестирования, находим индекс свечки, дата которой позже или равняется дате начала тестирования
                int candleIndex = 0;
                if (isFindFile)
                {
                    bool isFindCandle = false;
                    while (isFindCandle == false)
                    {
                        if (DateTime.Compare(dataSourceCandles[i].Candles[fileIndexes[i]][candleIndex].DateTime, testRun.StartPeriod) >= 0)
                        {
                            isFindCandle = true;
                        }
                        else
                        {
                            candleIndex++;
                        }
                    }
                }
                candleIndexes[i] = candleIndex;
            }

            bool isOverFileIndex = false; //вышел ли какой-либо из индексов файлов за границы массива файлов источника данных
            for (int i = 0; i < fileIndexes.Length; i++)
            {
                if (fileIndexes[i] >= dataSourceCandles[i].Candles.Length)
                {
                    isOverFileIndex = true; //отмечаем что индекс файла вышел за границы массива
                }
            }

            //устанавливаем текущую дату, взяв самую позднюю дату текущей свечки среди источников данных
            DateTime currentDateTime = new DateTime(); //текущие дата и время
            if (isOverFileIndex == false) //если не было превышений индекса файла, ищем текущую дату
            {
                currentDateTime = dataSourceCandles[0].Candles[fileIndexes[0]][candleIndexes[0]].DateTime; //текущие дата и время
                for (int i = 1; i < dataSourceCandles.Length; i++)
                {
                    if (DateTime.Compare(currentDateTime, dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime) < 0)
                    {
                        currentDateTime = dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime;
                    }
                }
            }

            //проходим по всем свечкам источников данных, пока не достигнем времени окончания теста, не выйдем за границы имеющихся файлов, или не получим запрос на отмену тестирования
            while (DateTime.Compare(currentDateTime, testRun.EndPeriod) < 0 && isOverFileIndex == false && cancellationToken.IsCancellationRequested == false)
            {
                //обрабатываем текущие заявки (только тех источников данных, текущие свечки которых равняются текущей дате)
                //формируем список источников данных для которых будут проверяться заявки на исполнение (те, даты которых равняются текущей дате)
                List<DataSource> approvedDataSources = new List<DataSource>();
                for (int i = 0; i < dataSourceCandles.Length; i++)
                {
                    if (DateTime.Compare(dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, currentDateTime) == 0)
                    {
                        approvedDataSources.Add(dataSourceCandles[i].DataSource);
                    }
                }
                if(testRun.Number == 101 && testRun.Account.Orders.Where(a => a.Number == 21).Any() && currentDateTime.Hour == 17 && currentDateTime.Minute == 43)
                {
                    int y = 0;
                }
                //проверяем заявки на исполнение
                bool isWereDeals = CheckOrdersExecution(dataSourceCandles, testRun.Account, approvedDataSources, fileIndexes, candleIndexes, true, true, true); //были ли совершены сделки при проверке исполнения заявок

                //проверяем, равняются ли все свечки источников данных текущей дате
                bool isCandlesDateTimeEqual = true;
                for (int i = 0; i < dataSourceCandles.Length; i++)
                {
                    if (DateTime.Compare(dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, currentDateTime) != 0)
                    {
                        isCandlesDateTimeEqual = false;
                    }
                }
                //если свечки всех источников данных равняются текущей дате, вычисляем индикаторы и алгоритм
                if (isCandlesDateTimeEqual)
                {
                    //если были совершены сделки на текущей свечке, дважды выполняем алгоритм: первый раз обновляем заявки и проверяем на исполнение стоп-заявки (если была открыта позиция на текущей свечке, нужно выставить стоп и проверить мог ли он на этой же свечке исполнится), и если были сделки то выполняем алгоритм еще раз и обновляем заявки, после чего переходим на следующую свечку

                    bool IsOverIndex = false; //было ли превышение индекса в индикаторах и алгоритме
                    double[][] indicatorsValues = new double[dataSourceCandles.Length][];
                    //проходим по всем источникам данных и формируем значения всех индикаторов для каждого источника данных
                    for (int i = 0; i < dataSourceCandles.Length; i++)
                    {
                        indicatorsValues[i] = new double[algorithmIndicators.Count];
                        for (int k = 0; k < indicatorsValues[i].Length; k++)
                        {
                            indicatorsValues[i][k] = dataSourceCandles[i].AlgorithmIndicatorCatalogs[k].AlgorithmIndicatorCatalogElements[algorithmIndicatorCatalogElementIndexes[i][k]].AlgorithmIndicatorValues.Values[fileIndexes[i]][candleIndexes[i]].Value;
                            if (dataSourceCandles[i].AlgorithmIndicatorCatalogs[k].AlgorithmIndicatorCatalogElements[algorithmIndicatorCatalogElementIndexes[i][k]].AlgorithmIndicatorValues.Values[fileIndexes[i]][candleIndexes[i]].IsNotOverIndex == false) //если при вычислении данного значения индикатора было превышение индекса свечки, отмечаем что было превышение индекса
                            {
                                IsOverIndex = true;
                            }
                        }
                    }

                    //если были сделки на этой свечке, то для того чтобы проверить мог ли исполниться стоп-лосс на текущей свечке, выполняем алгоритм (после чего для открытой позиции будет выставлен стоп-лосс) и проверяем исполнение стоп-заявок. Если в процессе выполнения стоп-заявок были совершены сделки, еще раз выполняем алгоритм, обновляем заявки и переходим на следующую свечку
                    int iteration = 0; //номер итерации
                    bool isWereDealsStopLoss = false; //были ли совешены сделки при проверки стоп-заявок на исполнение
                    do
                    {
                        iteration++;
                        //вычисляем алгоритм
                        //формируем dataSourcesForCalculate
                        DataSourceForCalculate[] dataSourcesForCalculate = new DataSourceForCalculate[dataSourceCandles.Length];
                        for (int i = 0; i < dataSourceCandles.Length; i++)
                        {
                            //определяем среднюю цену и объем позиции
                            double averagePricePosition = 0; //средняя цена позиции
                            decimal volumePosition = 0; //объем позиции
                            bool isBuyDirection = false;
                            foreach (Deal deal in testRun.Account.CurrentPosition)
                            {
                                if (deal.DataSource == dataSourceCandles[i].DataSource) //если сделка относится к текущему источнику данных
                                {
                                    if (volumePosition == 0) //если это первая сделка по данному источнику данных, запоминаем цену и объем
                                    {
                                        averagePricePosition = deal.Price;
                                        volumePosition = deal.Count;
                                    }
                                    else //если это не первая сделка по данному источнику данных, определяем среднюю цену и обновляем объем
                                    {
                                        averagePricePosition = (double)(((decimal)averagePricePosition * volumePosition + (decimal)deal.Price * deal.Count) / (volumePosition + deal.Count)); //(средняя цена * объем средней цены + текущая цена * текущий объем)/(объем средней цены + текущий объем)
                                        averagePricePosition = ModelFunctions.RoundToIncrement(averagePricePosition, deal.DataSource.PriceStep); //округляем среднюю цену позиции до шага 1 пункта цены данного инструмента
                                        volumePosition += deal.Count;
                                    }
                                    if (deal.Order.Direction)
                                    {
                                        isBuyDirection = true;
                                    }
                                }
                            }
                            DataSourceFileWorkingPeriod dataSourceFileWorkingPeriod = dataSourceCandles[i].DataSource.DataSourceFiles[fileIndexes[i]].DataSourceFileWorkingPeriods.Where(j => DateTime.Compare(currentDateTime, j.StartPeriod) >= 0).Last(); //последний период, дата начала которого раньше или равняется текущей дате

                            dataSourcesForCalculate[i] = new DataSourceForCalculate();
                            dataSourcesForCalculate[i].idDataSource = dataSourceCandles[i].DataSource.Id;
                            dataSourcesForCalculate[i].IndicatorsValues = indicatorsValues[i];
                            dataSourcesForCalculate[i].PriceStep = dataSourceCandles[i].DataSource.PriceStep;
                            dataSourcesForCalculate[i].CostPriceStep = dataSourceCandles[i].DataSource.CostPriceStep;
                            dataSourcesForCalculate[i].OneLotCost = dataSourceCandles[i].DataSource.Instrument.Id == 2 ? dataSourceCandles[i].DataSource.Cost : dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].C;
                            dataSourcesForCalculate[i].Price = averagePricePosition;
                            dataSourcesForCalculate[i].CountBuy = isBuyDirection ? volumePosition : 0;
                            dataSourcesForCalculate[i].CountSell = isBuyDirection ? 0 : volumePosition;
                            dataSourcesForCalculate[i].TimeInCandle = dataSourceCandles[i].DataSource.Interval.Duration;
                            dataSourcesForCalculate[i].TradingStartTimeOfDay = dataSourceFileWorkingPeriod.TradingStartTime;
                            dataSourcesForCalculate[i].TradingEndTimeOfDay = dataSourceFileWorkingPeriod.TradingEndTime;
                            dataSourcesForCalculate[i].Candles = dataSourceCandles[i].Candles[fileIndexes[i]];
                            dataSourcesForCalculate[i].CurrentCandleIndex = candleIndexes[i];
                        }

                        AccountForCalculate accountForCalculate = new AccountForCalculate { FreeRubleMoney = testRun.Account.FreeForwardDepositCurrencies.Where(j => j.Currency.Id == 1).First().Deposit, FreeDollarMoney = testRun.Account.FreeForwardDepositCurrencies.Where(j => j.Currency.Id == 2).First().Deposit, TakenRubleMoney = testRun.Account.TakenForwardDepositCurrencies.Where(j => j.Currency.Id == 1).First().Deposit, TakenDollarMoney = testRun.Account.TakenForwardDepositCurrencies.Where(j => j.Currency.Id == 2).First().Deposit, IsForwardDepositTrading = testRun.Account.IsForwardDepositTrading };
                        //копируем объект скомпилированного алгоритма, чтобы из разных потоков не обращаться к одному объекту и к одним свойствам объекта
                        dynamic CompiledAlgorithmCopy = testing.CompiledAlgorithm.Clone();
                        AlgorithmCalculateResult algorithmCalculateResult = CompiledAlgorithmCopy.Calculate(accountForCalculate, dataSourcesForCalculate, algorithmParametersIntValues, algorithmParametersDoubleValues);

                        if (IsOverIndex == false) //если не был превышен допустимый индекс при вычислении индикаторов и алгоритма, обрабатываем заявки
                        {
                            //удаляем заявки, количество лотов в которых равно 0
                            for (int i = algorithmCalculateResult.Orders.Count - 1; i >= 0; i--)
                            {
                                if (algorithmCalculateResult.Orders[i].Count == 0)
                                {
                                    algorithmCalculateResult.Orders.RemoveAt(i);
                                }
                            }
                            //если это не форвардное тестирование с торговлей депозитом, устанавливаем размер заявок в 1 лот, а так же устанавливаем DateTimeSubmit для заявок
                            foreach (Order order in algorithmCalculateResult.Orders)
                            {
                                if (testRun.Account.IsForwardDepositTrading == false)
                                {
                                    order.Count = 1;
                                    order.StartCount = 1;
                                }
                                order.DateTimeSubmit = currentDateTime;
                            }
                            //приводим заявки к виду который прислал пользователь в алгоритме
                            //самое самое новое
                            List<Order> accountOrders = new List<Order>(); //список с текущими выставленными заявками
                            accountOrders.AddRange(testRun.Account.Orders);

                            List<Order> userOrders = new List<Order>(); //список с заявками пользователя
                            userOrders.AddRange(algorithmCalculateResult.Orders);

                            List<Order> newOrders = new List<Order>(); //список с новыми выствленными заявками
                            newOrders.AddRange(algorithmCalculateResult.Orders);

                            //обрабатываем все заявки в accountOrders
                            int countAccountOrders = accountOrders.Count;
                            while (countAccountOrders > 0)
                            {
                                Order accountOrder = accountOrders[0]; //текущая заявка из accountOrders
                                Order userOrder = null; //совпадающая с accountOrder заявка из userOrders
                                //ищем в userOrders совпадающую с accountOrder заявку
                                int userOrderIndex = 0;
                                while (userOrderIndex < userOrders.Count && userOrder == null)
                                {
                                    bool isEqual = accountOrder.DataSource == userOrders[userOrderIndex].DataSource && accountOrder.TypeOrder == userOrders[userOrderIndex].TypeOrder && accountOrder.Direction == userOrders[userOrderIndex].Direction && accountOrder.Price == userOrders[userOrderIndex].Price && accountOrder.Count == userOrders[userOrderIndex].Count; //проверка на соответстве источника данных, типа заявки, направления, цены, количества
                                    isEqual = isEqual && ((accountOrder.LinkedOrder != null && userOrders[userOrderIndex].LinkedOrder != null) || (accountOrder.LinkedOrder == null && userOrders[userOrderIndex].LinkedOrder == null)); //проверка на соответствие наличия/отсутствия связаной заявки
                                    if (isEqual)
                                    {
                                        userOrder = userOrders[userOrderIndex]; //запоминаем совпадающую с accountOrder заявку
                                    }
                                    else
                                    {
                                        userOrderIndex++; //увеличиваем индекс заявок пользователя
                                    }
                                }
                                //если в userOrders есть совпадающая, удаляем совпадающую из userOrders и newOrders, и вставляем в newOrders из accountOrders
                                if (userOrder != null)
                                {
                                    userOrders.Remove(userOrder);
                                    newOrders.Remove(userOrder);
                                    newOrders.Add(accountOrder);
                                    //если у accountOrder есть связанная заявка, сравниваем accountOrder.LinkedOrder и userOrder.LinkedOrder
                                    if (accountOrder.LinkedOrder != null)
                                    {
                                        bool isEqual = accountOrder.LinkedOrder.DataSource == userOrder.LinkedOrder.DataSource && accountOrder.LinkedOrder.TypeOrder == userOrder.LinkedOrder.TypeOrder && accountOrder.LinkedOrder.Direction == userOrder.LinkedOrder.Direction && accountOrder.LinkedOrder.Price == userOrder.LinkedOrder.Price && accountOrder.LinkedOrder.Count == userOrder.LinkedOrder.Count; //проверка на соответстве источника данных, типа заявки, направления, цены, количества
                                        if (isEqual)
                                        {
                                            //если совпадают удаляем из userOrders и newOrders userOrder.LinkedOrder, вставляем в newOrders accountOrder.LinkedOrder, удаляем из accountOrders accountOrder.LinkedOrder
                                            userOrders.Remove(userOrder.LinkedOrder);
                                            newOrders.Remove(userOrder.LinkedOrder);
                                            newOrders.Add(accountOrder.LinkedOrder);
                                            accountOrders.Remove(accountOrder.LinkedOrder);
                                        }
                                        else
                                        {
                                            //если не совпадают, значит свзяанная с accountOrder будет взята из userOrder, и нужно проставить связи между ними (т.к. userOrder.LinkedOrder уже имеется, а accountOrder только что добавлена)
                                            accountOrder.LinkedOrder.DateTimeRemove = currentDateTime; //т.к. accountOrder.LinkedOrder не совпадает с userOrder.LinkedOrder, accountOrder.LinkedOrder снята, и нужно установить дату снятия
                                            accountOrder.LinkedOrder = userOrder.LinkedOrder;
                                            accountOrder.LinkedOrder.LinkedOrder = accountOrder;
                                        }
                                    }
                                }
                                else
                                {
                                    //т.к. в userOrders не была найдена такая заявка, она снята, и нужно установить дату снятия
                                    accountOrder.DateTimeRemove = currentDateTime;
                                }
                                //удаляем из accountOrders accountOrder, т.к. мы её обработали
                                accountOrders.Remove(accountOrder);
                                countAccountOrders = accountOrders.Count;
                            }

                            //проставляем номера новым заявкам и добавляем их в testRun.Account.AllOrders
                            int lastNumber = testRun.Account.AllOrders.Count == 0 ? 0 : testRun.Account.AllOrders.Last().Number; //номер последней заявки
                            foreach (Order order in newOrders)
                            {
                                if (order.Number == 0) //если номер заявки равен 0, значит она новая
                                {
                                    lastNumber++;
                                    order.Number = lastNumber;
                                    testRun.Account.AllOrders.Add(order);
                                }
                            }
                            //устанавливаем текущие выставленные заявки в newOrders
                            testRun.Account.Orders = newOrders;

                            //если на текущей свечке были совершены сделки, проверяем стоп-заявки на исполнение (чтобы если на текущей свечке была открыта позиция, после выставления стоп-заявки проверить её на исполнение на текущей свечке)
                            if (isWereDeals && iteration == 1)
                            {
                                isWereDealsStopLoss = CheckOrdersExecution(dataSourceCandles, testRun.Account, approvedDataSources, fileIndexes, candleIndexes, false, true, false); //были ли совершены сделки при проверке исполнения стоп-заявок
                            }
                        }
                    }
                    while (isWereDealsStopLoss && iteration == 1); //если этой первое исполнение алгоритма, и при проверке стоп-заявок были сделки, еще раз прогоняем алгоритм чтобы обновить заявки
                }

                //для каждого источника данных доходим до даты, которая позже текущей
                for (int i = 0; i < dataSourceCandles.Length; i++)
                {
                    //переходим на следующую свечку, пока не дойдем до даты которая позже текущей
                    bool isOverDate = DateTime.Compare(dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, currentDateTime) > 0; //дошли ли до даты которая позже текущей

                    //переходим на следующую свечку, пока не дойдем до даты которая позже текущей или пока не выйдем за пределы файлов
                    while (isOverDate == false && isOverFileIndex == false)
                    {
                        candleIndexes[i]++;
                        //если массив со свечками файла подошел к концу, переходим на следующий файл
                        if (candleIndexes[i] >= dataSourceCandles[i].Candles[fileIndexes[i]].Length)
                        {
                            fileIndexes[i]++;
                            candleIndexes[i] = 0;
                        }
                        //если индекс файла не вышел за пределы массива, проверяем, дошли ли до даты которая позже текущей
                        if (fileIndexes[i] < dataSourceCandles[i].Candles.Length)
                        {
                            isOverDate = DateTime.Compare(dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, currentDateTime) > 0;
                        }
                        else
                        {
                            isOverFileIndex = true;
                        }
                    }
                }

                //обновляем текущую дату (берем самую раннюю дату из источников данных)
                if (isOverFileIndex == false)
                {
                    currentDateTime = dataSourceCandles[0].Candles[fileIndexes[0]][candleIndexes[0]].DateTime;
                    for (int i = 1; i < dataSourceCandles.Length; i++)
                    {
                        if (DateTime.Compare(dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, currentDateTime) < 0)
                        {
                            currentDateTime = dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime;
                        }
                    }
                }
            }
            //устанавливаем значение маржи
            testRun.Account.Margin = ModelFunctions.MarginCalculate(testRun);
            //рассчитываем критерии оценки для данного testRun
            for (int i = 0; i < _modelData.EvaluationCriterias.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break; //если был запрос на отмену тестирования, завершаем цикл
                }
                //определяем индекс источника данных, с наибольшей идеальной прибылью
                /*int index = 0;
                for(int k = 1; k < dataSourceCandles.Length; k++)
                {
                    if(dataSourceCandles[index].PerfectProfit < dataSourceCandles[k].PerfectProfit)
                    {
                        index = k;
                    }
                }*/
                /*public EvaluationCriteriaValue Calculate(List<DataSourceCandles> dataSourcesCandles, TestRun testRun, ObservableCollection<Setting> settings)
                {
                    double ResultDoubleValue = 0;
                    string ResultStringValue = """";
                    " + script + @"
                    return new EvaluationCriteriaValue { DoubleValue = ResultDoubleValue, StringValue = ResultStringValue };
                }*/
        

                //ModelFunctions.TestEvaluationCriteria(testRun); //так я отлаживаю критерии оценки

                //копируем объект скомпилированного критерия оценки, чтобы из разных потоков не обращаться к одному объекту и к одним свойствам объекта
                dynamic CompiledEvaluationCriteriaCopy = testing.CompiledEvaluationCriterias[i].Clone();
                EvaluationCriteriaValue evaluationCriteriaValue = CompiledEvaluationCriteriaCopy.Calculate(dataSourceCandles, testRun, _modelData.Settings);
                evaluationCriteriaValue.EvaluationCriteria = _modelData.EvaluationCriterias[i];
                testRun.EvaluationCriteriaValues.Add(evaluationCriteriaValue);
            }
            testRun.IsComplete = true;
        }

        /*public bool CheckOrdersExecution(Testing testing, Account account, List<DataSource> approvedDataSources, Segment segment, bool isMarket, bool isStop, bool isLimit) //функция проверяет заявки на их исполнение в текущей свечке, возвращает false если не было сделок, и true если были совершены сделки. approvedDataSources - список с источниками данных, заявки которых будут проверяться на исполнение. isMarket, isStop, isLimit - если true, будут проверяться на исполнение эти заявки
        {
            bool isMakeADeals = false; //были ли совершены сделки
            //исполняем рыночные заявки
            if (isMarket)
            {
                List<Order> ordersToRemove = new List<Order>(); //заявки которые нужно удалить из заявок
                List<DateTime> ordersToRemoveDateTime = new List<DateTime>(); //дата снятия заявок
                foreach (Order order in account.Orders)
                {
                    if (order.TypeOrder.Id == 2 && approvedDataSources.Contains(order.DataSource)) //рыночная заявка
                    {
                        //определяем индекс CandleIndexes у сегмента с источником данных заявки
                        int candleIndex = 0;
                        for(int i = 0; i < segment.CandleIndexes.Count; i++)
                        {
                            if(testing.DataSourcesCandles[segment.CandleIndexes[i].DataSourceCandlesIndex].DataSource.Id == order.DataSource.Id)
                            {
                                candleIndex = i;
                            }
                        }
                        int slippage = _modelData.Settings.Where(i => i.Id == 3).First().IntValue; //количество пунктов на которое цена исполнения рыночной заявки будет хуже
                        slippage += Slippage(testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex], segment.CandleIndexes[candleIndex].FileIndex, segment.CandleIndexes[candleIndex].IndexCandle, order.Count); //добавляем проскальзывание
                        slippage = order.Direction == true ? slippage : -slippage; //для покупки проскальзывание идет вверх, для продажи вниз
                        isMakeADeals = MakeADeal(account, order, order.Count, testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].Candles[segment.CandleIndexes[candleIndex].FileIndex][segment.CandleIndexes[candleIndex].IndexCandle].C + slippage * testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].DataSource.PriceStep, testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].Candles[segment.CandleIndexes[candleIndex].FileIndex][segment.CandleIndexes[candleIndex].IndexCandle].DateTime) ? true : isMakeADeals;
                        if (order.Count == 0)
                        {
                            ordersToRemove.Add(order);
                            ordersToRemoveDateTime.Add(testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].Candles[segment.CandleIndexes[candleIndex].FileIndex][segment.CandleIndexes[candleIndex].IndexCandle].DateTime);
                        }
                    }
                }
                //снимаем полностью исполненные заявки
                for (int i = 0; i < ordersToRemove.Count; i++)
                {
                    ordersToRemove[i].DateTimeRemove = ordersToRemoveDateTime[i];
                    account.Orders.Remove(ordersToRemove[i]);
                    if (ordersToRemove[i].LinkedOrder != null)
                    {
                        ordersToRemove[i].LinkedOrder.DateTimeRemove = ordersToRemoveDateTime[i];
                        account.Orders.Remove(ordersToRemove[i].LinkedOrder);
                    }
                }
            }

            //проверяем стоп-заявки на исполнение
            if (isStop)
            {
                List<Order> ordersToRemove = new List<Order>(); //заявки которые нужно удалить из заявок
                List<DateTime> ordersToRemoveDateTime = new List<DateTime>(); //дата снятия заявок
                foreach (Order order in account.Orders)
                {
                    if (order.TypeOrder.Id == 3 && approvedDataSources.Contains(order.DataSource)) //стоп-заявка
                    {
                        //определяем индекс CandleIndexes у сегмента с источником данных заявки
                        int candleIndex = 0;
                        for (int i = 0; i < segment.CandleIndexes.Count; i++)
                        {
                            if (testing.DataSourcesCandles[segment.CandleIndexes[i].DataSourceCandlesIndex].DataSource.Id == order.DataSource.Id)
                            {
                                candleIndex = i;
                            }
                        }
                        //проверяем, зашла ли цена в текущей свечке за цену заявки
                        bool isStopExecute = (order.Direction == true && testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].Candles[segment.CandleIndexes[candleIndex].FileIndex][segment.CandleIndexes[candleIndex].IndexCandle].H >= order.Price) || (order.Direction == false && testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].Candles[segment.CandleIndexes[candleIndex].FileIndex][segment.CandleIndexes[candleIndex].IndexCandle].L <= order.Price); //заявка на покупку, и верхняя цена выше цены заявки или заявка на продажу, и нижняя цена ниже цены заявки
                        if (isStopExecute)
                        {
                            int slippage = _modelData.Settings.Where(i => i.Id == 3).First().IntValue; //количество пунктов на которое цена исполнения рыночной заявки будет хуже
                            slippage += Slippage(testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex], segment.CandleIndexes[candleIndex].FileIndex, segment.CandleIndexes[candleIndex].IndexCandle, order.Count); //добавляем проскальзывание
                            slippage = order.Direction == true ? slippage : -slippage; //для покупки проскальзывание идет вверх, для продажи вниз
                            isMakeADeals = MakeADeal(account, order, order.Count, order.Price + slippage * testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].DataSource.PriceStep, testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].Candles[segment.CandleIndexes[candleIndex].FileIndex][segment.CandleIndexes[candleIndex].IndexCandle].DateTime) ? true : isMakeADeals;
                            if (order.Count == 0)
                            {
                                ordersToRemove.Add(order);
                                ordersToRemoveDateTime.Add(testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].Candles[segment.CandleIndexes[candleIndex].FileIndex][segment.CandleIndexes[candleIndex].IndexCandle].DateTime);
                            }
                        }
                    }
                }
                //снимаем полностью исполненные заявки
                for (int i = 0; i < ordersToRemove.Count; i++)
                {
                    ordersToRemove[i].DateTimeRemove = ordersToRemoveDateTime[i];
                    account.Orders.Remove(ordersToRemove[i]);
                    if (ordersToRemove[i].LinkedOrder != null)
                    {
                        ordersToRemove[i].LinkedOrder.DateTimeRemove = ordersToRemoveDateTime[i];
                        account.Orders.Remove(ordersToRemove[i].LinkedOrder);
                    }
                }
            }

            //проверяем лимитные заявки на исполнение
            if (isLimit)
            {
                List<Order> ordersToRemove = new List<Order>(); //заявки которые нужно удалить из заявок
                List<DateTime> ordersToRemoveDateTime = new List<DateTime>(); //дата снятия заявок
                foreach (Order order in account.Orders)
                {
                    if (order.TypeOrder.Id == 1 && approvedDataSources.Contains(order.DataSource)) //лимитная заявка
                    {
                        //определяем индекс CandleIndexes у сегмента с источником данных заявки
                        int candleIndex = 0;
                        for (int i = 0; i < segment.CandleIndexes.Count; i++)
                        {
                            if (testing.DataSourcesCandles[segment.CandleIndexes[i].DataSourceCandlesIndex].DataSource.Id == order.DataSource.Id)
                            {
                                candleIndex = i;
                            }
                        }
                        //проверяем, зашла ли цена в текущей свечке за цену заявки
                        bool isLimitExecute = (order.Direction == true && testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].Candles[segment.CandleIndexes[candleIndex].FileIndex][segment.CandleIndexes[candleIndex].IndexCandle].L < order.Price) || (order.Direction == false && testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].Candles[segment.CandleIndexes[candleIndex].FileIndex][segment.CandleIndexes[candleIndex].IndexCandle].H > order.Price); //заявка на покупку, и нижняя цена ниже цены покупки или заявка на продажу, и верхняя цена выше цены продажи
                        if (isLimitExecute)
                        {
                            //определяем количество лотов, которое находится за ценой заявки, и которое могло быть куплено/продано на текущей свечке
                            int stepCount = (int)Math.Round((testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].Candles[segment.CandleIndexes[candleIndex].FileIndex][segment.CandleIndexes[candleIndex].IndexCandle].H - testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].Candles[segment.CandleIndexes[candleIndex].FileIndex][segment.CandleIndexes[candleIndex].IndexCandle].L + order.DataSource.PriceStep) / order.DataSource.PriceStep); //количество пунктов цены
                            decimal stepLots = (decimal)testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].Candles[segment.CandleIndexes[candleIndex].FileIndex][segment.CandleIndexes[candleIndex].IndexCandle].V / stepCount; //среднее количество лотов на 1 пункт цены
                            int stepsOver = order.Direction ? (int)Math.Round((order.Price - testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].Candles[segment.CandleIndexes[candleIndex].FileIndex][segment.CandleIndexes[candleIndex].IndexCandle].L) / order.DataSource.PriceStep) : (int)Math.Round((testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].Candles[segment.CandleIndexes[candleIndex].FileIndex][segment.CandleIndexes[candleIndex].IndexCandle].H - order.Price) / order.DataSource.PriceStep); //количество пунктов за ценой заявки
                            decimal overLots = stepLots * stepsOver / 2; //количество лотов которое могло быть куплено/продано на текущей свечке (делить на 2 т.к. лишь половина от лотов - это сделки в нужной нам операции (купить или продать))
                            if (order.DataSource.Instrument.Id != 3) //если это не криптовалюта, округляем количество лотов до целого
                            {
                                overLots = Math.Round(overLots);
                            }
                            if (overLots > 0) //если есть лоты которые могли быть исполнены на текущей свечке, совершаем сделку
                            {
                                decimal dealCount = order.Count <= overLots ? order.Count : overLots;
                                isMakeADeals = MakeADeal(account, order, dealCount, order.Price, testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].Candles[segment.CandleIndexes[candleIndex].FileIndex][segment.CandleIndexes[candleIndex].IndexCandle].DateTime) ? true : isMakeADeals;
                                if (order.Count == 0)
                                {
                                    ordersToRemove.Add(order);
                                    ordersToRemoveDateTime.Add(testing.DataSourcesCandles[segment.CandleIndexes[candleIndex].DataSourceCandlesIndex].Candles[segment.CandleIndexes[candleIndex].FileIndex][segment.CandleIndexes[candleIndex].IndexCandle].DateTime);
                                }
                            }
                        }
                    }
                }
                //снимаем полностью исполненные заявки
                for (int i = 0; i < ordersToRemove.Count; i++)
                {
                    ordersToRemove[i].DateTimeRemove = ordersToRemoveDateTime[i];
                    account.Orders.Remove(ordersToRemove[i]);
                    if (ordersToRemove[i].LinkedOrder != null)
                    {
                        ordersToRemove[i].LinkedOrder.DateTimeRemove = ordersToRemoveDateTime[i];
                        account.Orders.Remove(ordersToRemove[i].LinkedOrder);
                    }
                }
            }
            return isMakeADeals;
        }*/

        public bool CheckOrdersExecution(DataSourceCandles[] dataSourcesCandles, Account account, List<DataSource> approvedDataSources, int[] fileIndexes, int[] candleIndexes, bool isMarket, bool isStop, bool isLimit) //функция проверяет заявки на их исполнение в текущей свечке, возвращает false если не было сделок, и true если были совершены сделки. approvedDataSources - список с источниками данных, заявки которых будут проверяться на исполнение. isMarket, isStop, isLimit - если true, будут проверяться на исполнение эти заявки
        {
            bool isMakeADeals = false; //были ли совершены сделки
            //исполняем рыночные заявки
            if (isMarket)
            {
                List<Order> ordersToRemove = new List<Order>(); //заявки которые нужно удалить из заявок
                List<DateTime> ordersToRemoveDateTime = new List<DateTime>(); //дата снятия заявок
                foreach (Order order in account.Orders)
                {
                    if (order.TypeOrder.Id == 2 && approvedDataSources.Contains(order.DataSource)) //рыночная заявка
                    {
                        //определяем индекс источника данных со свечками текущей заявки
                        int dataSourcesCandlesIndex = 0;
                        for (int i = 0; i < dataSourcesCandles.Length; i++)
                        {
                            if (dataSourcesCandles[i].DataSource == order.DataSource)
                            {
                                dataSourcesCandlesIndex = i;
                            }
                        }
                        int slippage = _modelData.Settings.Where(i => i.Id == 3).First().IntValue; //количество пунктов на которое цена исполнения рыночной заявки будет хуже
                        slippage += Slippage(dataSourcesCandles[dataSourcesCandlesIndex], fileIndexes[dataSourcesCandlesIndex], candleIndexes[dataSourcesCandlesIndex], order.Count); //добавляем проскальзывание
                        slippage = order.Direction == true ? slippage : -slippage; //для покупки проскальзывание идет вверх, для продажи вниз
                        isMakeADeals = MakeADeal(account, order, order.Count, dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].C + slippage * dataSourcesCandles[dataSourcesCandlesIndex].DataSource.PriceStep, dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime) ? true : isMakeADeals;
                        if (order.Count == 0)
                        {
                            ordersToRemove.Add(order);
                            ordersToRemoveDateTime.Add(dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime);
                        }
                    }
                }
                //снимаем полностью исполненные заявки
                for (int i = 0; i < ordersToRemove.Count; i++)
                {
                    ordersToRemove[i].DateTimeRemove = ordersToRemoveDateTime[i];
                    account.Orders.Remove(ordersToRemove[i]);
                    if (ordersToRemove[i].LinkedOrder != null)
                    {
                        ordersToRemove[i].LinkedOrder.DateTimeRemove = ordersToRemoveDateTime[i];
                        account.Orders.Remove(ordersToRemove[i].LinkedOrder);
                    }
                }
            }

            //проверяем стоп-заявки на исполнение
            if (isStop)
            {
                List<Order> ordersToRemove = new List<Order>(); //заявки которые нужно удалить из заявок
                List<DateTime> ordersToRemoveDateTime = new List<DateTime>(); //дата снятия заявок
                foreach (Order order in account.Orders)
                {
                    if (order.TypeOrder.Id == 3 && approvedDataSources.Contains(order.DataSource)) //стоп-заявка
                    {
                        //определяем индекс источника данных со свечками текущей заявки
                        int dataSourcesCandlesIndex = 0;
                        for (int i = 0; i < dataSourcesCandles.Length; i++)
                        {
                            if (dataSourcesCandles[i].DataSource == order.DataSource)
                            {
                                dataSourcesCandlesIndex = i;
                            }
                        }
                        //проверяем, зашла ли цена в текущей свечке за цену заявки
                        bool isStopExecute = (order.Direction == true && dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H >= order.Price) || (order.Direction == false && dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L <= order.Price); //заявка на покупку, и верхняя цена выше цены заявки или заявка на продажу, и нижняя цена ниже цены заявки
                        if (isStopExecute)
                        {
                            int slippage = _modelData.Settings.Where(i => i.Id == 3).First().IntValue; //количество пунктов на которое цена исполнения рыночной заявки будет хуже
                            slippage += Slippage(dataSourcesCandles[dataSourcesCandlesIndex], fileIndexes[dataSourcesCandlesIndex], candleIndexes[dataSourcesCandlesIndex], order.Count); //добавляем проскальзывание
                            slippage = order.Direction == true ? slippage : -slippage; //для покупки проскальзывание идет вверх, для продажи вниз
                            isMakeADeals = MakeADeal(account, order, order.Count, order.Price + slippage * dataSourcesCandles[dataSourcesCandlesIndex].DataSource.PriceStep, dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime) ? true : isMakeADeals;
                            if (order.Count == 0)
                            {
                                ordersToRemove.Add(order);
                                ordersToRemoveDateTime.Add(dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime);
                            }
                        }
                    }
                }
                //снимаем полностью исполненные заявки
                for (int i = 0; i < ordersToRemove.Count; i++)
                {
                    ordersToRemove[i].DateTimeRemove = ordersToRemoveDateTime[i];
                    account.Orders.Remove(ordersToRemove[i]);
                    if (ordersToRemove[i].LinkedOrder != null)
                    {
                        ordersToRemove[i].LinkedOrder.DateTimeRemove = ordersToRemoveDateTime[i];
                        account.Orders.Remove(ordersToRemove[i].LinkedOrder);
                    }
                }
            }

            //проверяем лимитные заявки на исполнение
            if (isLimit)
            {
                List<Order> ordersToRemove = new List<Order>(); //заявки которые нужно удалить из заявок
                List<DateTime> ordersToRemoveDateTime = new List<DateTime>(); //дата снятия заявок
                foreach (Order order in account.Orders)
                {
                    if (order.TypeOrder.Id == 1 && approvedDataSources.Contains(order.DataSource)) //лимитная заявка
                    {
                        //определяем индекс источника данных со свечками текущей заявки
                        int dataSourcesCandlesIndex = 0;
                        for (int i = 0; i < dataSourcesCandles.Length; i++)
                        {
                            if (dataSourcesCandles[i].DataSource == order.DataSource)
                            {
                                dataSourcesCandlesIndex = i;
                            }
                        }
                        //проверяем, зашла ли цена в текущей свечке за цену заявки
                        bool isLimitExecute = (order.Direction == true && dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L < order.Price) || (order.Direction == false && dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H > order.Price); //заявка на покупку, и нижняя цена ниже цены покупки или заявка на продажу, и верхняя цена выше цены продажи
                        if (isLimitExecute)
                        {
                            //определяем количество лотов, которое находится за ценой заявки, и которое могло быть куплено/продано на текущей свечке
                            int stepCount = (int)Math.Round((dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H - dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L + order.DataSource.PriceStep) / order.DataSource.PriceStep); //количество пунктов цены
                            decimal stepLots = (decimal)dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].V / stepCount; //среднее количество лотов на 1 пункт цены
                            int stepsOver = order.Direction ? (int)Math.Round((order.Price - dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L) / order.DataSource.PriceStep) : (int)Math.Round((dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H - order.Price) / order.DataSource.PriceStep); //количество пунктов за ценой заявки
                            decimal overLots = stepLots * stepsOver / 2; //количество лотов которое могло быть куплено/продано на текущей свечке (делить на 2 т.к. лишь половина от лотов - это сделки в нужной нам операции (купить или продать))
                            if (order.DataSource.Instrument.Id != 3) //если это не криптовалюта, округляем количество лотов до целого
                            {
                                overLots = Math.Round(overLots);
                            }
                            if (overLots > 0) //если есть лоты которые могли быть исполнены на текущей свечке, совершаем сделку
                            {
                                decimal dealCount = order.Count <= overLots ? order.Count : overLots;
                                isMakeADeals = MakeADeal(account, order, dealCount, order.Price, dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime) ? true : isMakeADeals;
                                if (order.Count == 0)
                                {
                                    ordersToRemove.Add(order);
                                    ordersToRemoveDateTime.Add(dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime);
                                }
                            }
                        }
                    }
                }
                //снимаем полностью исполненные заявки
                for (int i = 0; i < ordersToRemove.Count; i++)
                {
                    ordersToRemove[i].DateTimeRemove = ordersToRemoveDateTime[i];
                    account.Orders.Remove(ordersToRemove[i]);
                    if (ordersToRemove[i].LinkedOrder != null)
                    {
                        ordersToRemove[i].LinkedOrder.DateTimeRemove = ordersToRemoveDateTime[i];
                        account.Orders.Remove(ordersToRemove[i].LinkedOrder);
                    }
                }
            }
            return isMakeADeals;
        }

        private bool MakeADeal(Account account, Order order, decimal lotsCount, double price, DateTime dateTime) //совершает сделку, возвращает false если сделка не была совершена, и true если была совершена. Закрывает открытые позиции если они есть, и открывает новые если заявка не была исполнена полностью на закрытие позиций, высчитывает результат трейда, обновляет занятые и свободные средства во всех валютах, удаляет закрытые сделки в открытых позициях
        {
            bool isMakeADeal = false; //была ли совершена сделка
            //определяем хватает ли средств на 1 лот, если да, определяем хватает ли средств на lotsCount, если нет устанавливаем минимально доступное количество
            //стоимость 1 лота
            double lotsCost = order.DataSource.Instrument.Id == 2 ? order.DataSource.Cost : price; //если инструмент источника данных - фьючерс, устанавливаем стоимость в стоимость фьючерса, иначе - устанавливаем стоимость 1 лота в стоимость с графика
            double lotsComission = order.DataSource.Comissiontype.Id == 2 ? lotsCost * order.DataSource.Comission : order.DataSource.Comission; //комиссия на 1 лот
            double freeDeposit = account.FreeForwardDepositCurrencies.Where(i => i.Currency == order.DataSource.Currency).First().Deposit; //свободный остаток в валюте источника данных
            double takenDeposit = account.TakenForwardDepositCurrencies.Where(i => i.Currency == order.DataSource.Currency).First().Deposit; //занятые средства на открытые позиции в валюте источника данных
            //определяем максимально доступное количество лотов
            decimal maxLotsCount = Math.Truncate((decimal)freeDeposit / (decimal)(lotsCost + lotsComission));
            decimal reverseLotsCount = 0;//количество лотов в открытой позиции с обратным направлением
            foreach (Deal deal in account.CurrentPosition)
            {
                if (deal.DataSource == order.DataSource && deal.Order.Direction != order.Direction) //если сделка совершена по тому же источнику данных что и заявка, но отличается с ней в направлении
                {
                    reverseLotsCount += deal.Count;
                }
            }
            maxLotsCount += reverseLotsCount; //прибавляем к максимально доступному количеству лотов, количество лотов в открытой позиции с обратным направлением
            //если это не форвардное тестирование с торговлей депозитом, устанавливаем максимально доступное количество лотов в 1
            if (account.IsForwardDepositTrading == false)
            {
                maxLotsCount = 1;
            }
            if (maxLotsCount > 0) //если максимально доступное количество лотов для совершения сделки по заявке > 0, совершаем сделку
            {
                decimal dealLotsCount = lotsCount > maxLotsCount ? maxLotsCount : lotsCount; //если количество лотов для сделки больше максимально доступного, устанавливаем в максимально доступное
                //вычитаем из неисполненных лотов заявки dealLotsCount
                order.Count -= dealLotsCount;
                if (order.LinkedOrder != null)
                {
                    order.LinkedOrder.Count = order.Count;
                }
                //записываем сделку
                account.AllDeals.Add(new Deal { Number = account.AllDeals.Count, IdDataSource = order.DataSource.Id, DataSource = order.DataSource, OrderNumber = order.Number, Order = order, Price = price, Count = dealLotsCount, DateTime = dateTime });
                Deal currentDeal = new Deal { Number = account.AllDeals.Count, IdDataSource = order.DataSource.Id, DataSource = order.DataSource, OrderNumber = order.Number, Order = order, Price = price, Count = dealLotsCount, DateTime = dateTime };
                account.CurrentPosition.Add(currentDeal);
                isMakeADeal = true; //запоминаем что была совершена сделка
                //вычитаем комиссию на сделку из свободных средств
                freeDeposit -= (double)((decimal)lotsComission * dealLotsCount);
                //закрываем открытые позиции которые были закрыты данной сделкой
                int i = 0;
                while (i < account.CurrentPosition.Count - 1 && currentDeal.Count > 0) //проходим по всем сделкам кроме последней (только что добавленной)
                {
                    if (account.CurrentPosition[i].DataSource == currentDeal.DataSource && account.CurrentPosition[i].Order.Direction != currentDeal.Order.Direction) //если совпадает источник данных, но отличается направление сделки
                    {
                        decimal decrementCount = account.CurrentPosition[i].Count > currentDeal.Count ? currentDeal.Count : account.CurrentPosition[i].Count; //количество для уменьшения лотов в сделке
                        //определяем денежный результат трейда и прибавляем его к свободным средствам
                        double priceSell = account.CurrentPosition[i].Order.Direction == false ? account.CurrentPosition[i].Price : currentDeal.Price; //цена продажи в трейде
                        double priceBuy = account.CurrentPosition[i].Order.Direction == true ? account.CurrentPosition[i].Price : currentDeal.Price; //цена покупки в трейде
                        double resultMoney = (double)(decrementCount * (decimal)(((priceSell - priceBuy) / account.CurrentPosition[i].DataSource.PriceStep) * account.CurrentPosition[i].DataSource.CostPriceStep)); //определяю разность цен между проджей и покупкой, делю на шаг цены и умножаю на стоимость шага цены, получаю денежное значение для 1 лота, и умножаю на количество лотов
                        freeDeposit += resultMoney;
                        //определяем стоимость закрытых лотов в открытой позиции, вычитаем её из занятых средств и прибавляем к свободным
                        double closedCost = account.CurrentPosition[i].Order.DataSource.Instrument.Id == 2 ? account.CurrentPosition[i].Order.DataSource.Cost : account.CurrentPosition[i].Price;
                        closedCost = (double)((decimal)closedCost * decrementCount); //умножаем стоимость на количество
                        takenDeposit -= closedCost; //вычитаем из занятых на открытые позиции средств
                        freeDeposit += closedCost; //прибавляем к свободным средствам
                        //вычитаем закрытое количесво из открытых позиций
                        account.CurrentPosition[i].Count -= decrementCount;
                        currentDeal.Count -= decrementCount;
                    }
                    i++;
                }
                //определяем стоимость занятых средств на оставшееся (незакрытое) количество лотов текущей сделки, вычитаем её из сободных средств и добавляем к занятым
                double currentCost = currentDeal.DataSource.Instrument.Id == 2 ? currentDeal.DataSource.Cost : currentDeal.Price;
                currentCost = (double)((decimal)currentCost * currentDeal.Count); //умножаем стоимость на количество
                freeDeposit -= currentCost; //вычитаем из свободных средств
                takenDeposit += currentCost; //добавляем к занятым на открытые позиции средствам
                //удаляем из открытых позиций сделки с нулевым количеством
                for (int j = account.CurrentPosition.Count - 1; j >= 0; j--)
                {
                    if (account.CurrentPosition[j].Count == 0)
                    {
                        account.CurrentPosition.RemoveAt(j);
                    }
                }
                //обновляем средства во всех валютах
                account.FreeForwardDepositCurrencies = CalculateDepositCurrrencies(freeDeposit, order.DataSource.Currency);
                account.TakenForwardDepositCurrencies = CalculateDepositCurrrencies(takenDeposit, order.DataSource.Currency);
                //если открытые позиции пусты и была совершена сделка, записываем состояние депозита
                if (account.CurrentPosition.Count == 0 && isMakeADeal)
                {
                    account.DepositCurrenciesChanges.Add(account.FreeForwardDepositCurrencies);
                }
            }
            return isMakeADeal;
        }

        private List<DepositCurrency> CalculateDepositCurrrencies(double deposit, Currency inputCurrency) //возвращает значения депозита во всех валютах
        {
            List<DepositCurrency> depositCurrencies = new List<DepositCurrency>();

            double dollarCostDeposit = deposit / inputCurrency.DollarCost; //определяем долларовую стоимость
            foreach (Currency currency in _modelData.Currencies)
            {
                //переводим доллоровую стоимость в валютную, умножая на стоимость 1 доллара
                double cost = Math.Round(dollarCostDeposit * currency.DollarCost, 2);
                depositCurrencies.Add(new DepositCurrency { Currency = currency, Deposit = cost });
            }

            return depositCurrencies;
        }

        private int Slippage(DataSourceCandles dataSourceCandles, int fileIndex, int candleIndex, decimal lotsCountInOrder) //возвращает размер проскальзывания в пунктах
        {
            int candleCount = 20; //количество свечек для определения среднего количества лотов на 1 пункт цены
            candleCount = candleIndex < candleCount ? candleIndex + 1 : candleCount; //чтобы избежать обращения к несуществующему индексу
            decimal lotsCount = 0; //количество лотов
            int pointsCount = 0; //количество пунктов цены на которых было куплено/продано данное количество лотов
            for (int i = 0; i < candleCount; i++)
            {
                lotsCount += (decimal)dataSourceCandles.Candles[fileIndex][candleIndex - i].V;
                pointsCount += (int)Math.Round((dataSourceCandles.Candles[fileIndex][candleIndex - i].H - dataSourceCandles.Candles[fileIndex][candleIndex - i].L) / dataSourceCandles.DataSource.CostPriceStep); //делим high - low на стоимость 1 пункта цены и получаем количество пунктов
            }
            pointsCount = pointsCount == 0 ? 1 : pointsCount; //чтобы избежать деления на 0
            decimal lotsInOnePoint = lotsCount / pointsCount; //количество лотов в 1 пункте цены
            lotsInOnePoint = lotsInOnePoint == 0 ? (decimal)0.001 : lotsInOnePoint; //чтобы избежать деления на 0
            int slippage = (int)Math.Round(lotsCountInOrder / lotsInOnePoint / 2); //делю количество лотов в заявке на количество лотов в 1 пункте и получаю количество пунктов на которое размажется цена, поделив это количество на 2 получаю среднее значение проскальзывания
            return slippage;
        }

        private void TestBatchTopModelDetermining(TestBatch testBatch, Testing testing) //определение топ-модели среди оптимизационных тестов тестовой связки
        {
            if (testing.AlgorithmParametersAllIntValues.Length == 0) //если параметров нет - оптимизационный тест всего один, топ модель - testBatch.OptimizationTestRuns[0]
            {
                testBatch.TopModelTestRun = testBatch.OptimizationTestRuns[0];
            }
            else if (testing.IsConsiderNeighbours) //если поиск топ-модели учитывает соседей, то для двух и более параметров - определяем оси двумерной плоскости поиска топ-модели с соседями и размер осей группы и определяем список с лучшими группами в порядке убывания и ищем топ-модель в группе, а для одного параметра - определяем размер группы и определяем список с лучшими группами в порядке убывания и ищем топ-модель (если из-за фильтров не найдена модель, ищем топ-модель в следующей лучшей группе, пока не кончатся группы)
            {
                if (testing.AlgorithmParametersAllIntValues.Length == 1) //если параметр всего один
                {
                    int xAxisCountParameterValue = testing.AlgorithmParametersAllIntValues.Length > 0 ? testing.AlgorithmParametersAllIntValues.Length : testing.AlgorithmParametersAllDoubleValues.Length; //количество значений параметра
                    int xAxisGroupSize = (int)Math.Round(xAxisCountParameterValue * (testing.SizeNeighboursGroupPercent / 100));
                    xAxisGroupSize = xAxisGroupSize < 2 ? 2 : xAxisGroupSize; //если меньше 2-х, устанавливаем как 2
                    xAxisGroupSize = xAxisCountParameterValue < 2 ? 1 : xAxisGroupSize; //если количество значений параметра меньше 2-х, устанавливаем как 1

                    List<TestRun[]> testRunGroups = new List<TestRun[]>(); //список с группами
                    List<double> amountGroupsValue = new List<double>(); //суммарное значение критерия оценки для групп
                                                                         //формируем группы
                    int startIndex = 0; //индекс первого элемента для группы
                    int endIndex = startIndex + (xAxisGroupSize - 1); //индекс последнего элемента для группы
                    while (endIndex < testBatch.OptimizationTestRuns.Count)
                    {
                        TestRun[] testRuns = new TestRun[xAxisGroupSize];
                        for (int i = 0; i < xAxisGroupSize; i++)
                        {
                            testRuns[i] = testBatch.OptimizationTestRuns[startIndex + i];
                        }
                        startIndex++;
                        endIndex = startIndex + (xAxisGroupSize - 1);
                    }
                    //вычисляем суммарные значения критерия оценки для групп
                    for (int i = 0; i < testRunGroups.Count; i++)
                    {
                        double amountValue = 0;
                        for (int k = 0; k < testRunGroups[i].Length; k++)
                        {
                            amountValue += testRunGroups[i][k].EvaluationCriteriaValues[testing.TopModelEvaluationCriteriaIndex].DoubleValue;
                        }
                        amountGroupsValue.Add(amountValue);
                    }
                    //сортируем список групп по убыванию суммарного значения критерия оценки
                    TestRun[] saveGroup; //элемент списка для сохранения после удаления из списка
                    double saveValue; //элемент списка для сохранения после удаления из списка
                    for (int i = 0; i < amountGroupsValue.Count; i++)
                    {
                        for (int k = 0; k < amountGroupsValue.Count - 1; k++)
                        {
                            if (amountGroupsValue[k] < amountGroupsValue[k + 1])
                            {
                                saveGroup = testRunGroups[k];
                                testRunGroups[k] = testRunGroups[k + 1];
                                testRunGroups[k + 1] = saveGroup;

                                saveValue = amountGroupsValue[k];
                                amountGroupsValue[k] = amountGroupsValue[k + 1];
                                amountGroupsValue[k + 1] = saveValue;
                            }
                        }
                    }
                    //сортируем testRun-ы в группах в порядке убытвания критерия оценки
                    TestRun saveTestRun; //элемент списка для сохранения после удаления из списка
                    for (int u = 0; u < testRunGroups.Count; u++)
                    {
                        for (int i = 0; i < testRunGroups[u].Length; i++)
                        {
                            for (int k = 0; k < testRunGroups[u].Length - 1; k++)
                            {
                                if (testRunGroups[u][k].EvaluationCriteriaValues[testing.TopModelEvaluationCriteriaIndex].DoubleValue < testRunGroups[u][k + 1].EvaluationCriteriaValues[testing.TopModelEvaluationCriteriaIndex].DoubleValue)
                                {
                                    saveTestRun = testRunGroups[u][k];
                                    testRunGroups[u][k] = testRunGroups[u][k + 1];
                                    testRunGroups[u][k + 1] = saveTestRun;
                                }
                            }
                        }
                    }
                    //проходим по всем группам, и в каждой группе проходим по всем testRun-ам, и ищем первый который соответствует фильтрам
                    bool isFindTopModel = false;
                    int groupIndex = 0;
                    while (isFindTopModel == false && groupIndex < testRunGroups.Count)
                    {
                        int testRunIndex1 = 0;
                        while (isFindTopModel == false && testRunIndex1 < testRunGroups[groupIndex].Length)
                        {
                            //проходим по всем фильтрам
                            bool isFilterFail = false;
                            foreach (TopModelFilter topModelFilter in testing.TopModelCriteria.TopModelFilters)
                            {
                                if (topModelFilter.CompareSign == CompareSign.GetMore()) //знак сравнения фильтра Больше
                                {
                                    if (testRunGroups[groupIndex][testRunIndex1].EvaluationCriteriaValues.Where(j => j.EvaluationCriteria == topModelFilter.EvaluationCriteria).First().DoubleValue <= topModelFilter.Value)
                                    {
                                        isFilterFail = true;
                                    }
                                }
                                else //знак сравнения фильтра Меньше
                                {
                                    if (testRunGroups[groupIndex][testRunIndex1].EvaluationCriteriaValues.Where(j => j.EvaluationCriteria == topModelFilter.EvaluationCriteria).First().DoubleValue >= topModelFilter.Value)
                                    {
                                        isFilterFail = true;
                                    }
                                }
                            }
                            //если testRun удовлетворяет всем фильтрам, записываем его как топ-модель
                            if (isFilterFail == false)
                            {
                                testBatch.TopModelTestRun = testRunGroups[groupIndex][testRunIndex1];
                                isFindTopModel = true;
                            }
                        }
                        groupIndex++;
                    }
                }
                else //если параметров 2 и более
                {
                    //определяем оси двумерной плоскости поиска топ-модели с соседями
                    if (testing.IsAxesSpecified) //если оси указаны, присваиваем указанные оси
                    {
                        testBatch.AxesTopModelSearchPlane = testing.AxesTopModelSearchPlane;
                    }
                    else //если оси не указаны, находим оси двумерной плоскости поиска топ-модели с соседями, для которых волатильность критерия оценки минимальная
                    {
                        //формируем список со всеми параметрами
                        /*List<int[]> indicatorsAndAlgorithmParameters = new List<int[]>(); //список с параметрами (0-й элемент массива - тип параметра: 1-индикатор, 2-алгоритм, 1-й элемент массива - индекс параметра)
                        for (int i = 0; i < IndicatorsParametersAllIntValues.Length; i++)
                        {
                            indicatorsAndAlgorithmParameters.Add(new int[2] { 1, i }); //запоминаем что параметр индикатор с индексом i
                        }
                        for (int i = 0; i < AlgorithmParametersAllIntValues.Length; i++)
                        {
                            indicatorsAndAlgorithmParameters.Add(new int[2] { 2, i }); //запоминаем что параметр индикатор с индексом i
                        }*/
                        //находим максимальную площадь плоскости
                        int maxArea = 0;
                        int axisX = 0; //одна ось плоскости
                        int axisY = 0; //вторая ось плоскости
                        //новое
                        for (int i = 0; i < testing.AlgorithmParametersAllIntValues.Length; i++)
                        {
                            for (int k = 0; k < testing.AlgorithmParametersAllIntValues.Length; k++)
                            {
                                if (i != k)
                                {
                                    int iCount = 0; //количество элементов в параметре с индексом i
                                    iCount = testing.AlgorithmParametersAllIntValues[i].Count > 0 ? testing.AlgorithmParametersAllIntValues[i].Count : testing.AlgorithmParametersAllDoubleValues[i].Count; //если количество элементов в int values больше нуля, присваиваем количеству параметра количество int values элементов, индаче количество double values элементов
                                    int kCount = 0; //количество элементов в параметре с индексом k
                                    kCount = testing.AlgorithmParametersAllIntValues[i].Count > 0 ? testing.AlgorithmParametersAllIntValues[i].Count : testing.AlgorithmParametersAllDoubleValues[i].Count; //если количество элементов в int values больше нуля, присваиваем количеству параметра количество int values элементов, индаче количество double values элементов
                                    if (iCount * kCount > maxArea) //если площадь данной комбинации больше максимальной, запоминаем площадь плоскости и её оси
                                    {
                                        maxArea = iCount * kCount;
                                        axisX = i;
                                        axisY = k;
                                    }
                                }
                            }
                        }
                        //формируем список с комбинациями параметров, которые имеют минимально допустимую площадь плоскости
                        double minMaxArea = 0.6; //минимально допустимая площадь плоскости от максимального. Чтобы исключить выбор осей с небольшой площадью но большой средней волатильностью
                        List<int[]> parametersCombination = new List<int[]>(); //комбинации из 2-х параметров, площадь которых в пределах допустимой
                        //новое
                        for (int i = 0; i < testing.AlgorithmParametersAllIntValues.Length; i++)
                        {
                            for (int k = 0; k < testing.AlgorithmParametersAllIntValues.Length; k++)
                            {
                                if (i != k)
                                {
                                    int iCount = 0; //количество элементов в параметре с индексом i
                                    iCount = testing.AlgorithmParametersAllIntValues[i].Count > 0 ? testing.AlgorithmParametersAllIntValues[i].Count : testing.AlgorithmParametersAllDoubleValues[i].Count; //если количество элементов в int values больше нуля, присваивае
                                    int kCount = 0; //количество элементов в параметре с индексом k
                                    kCount = testing.AlgorithmParametersAllIntValues[i].Count > 0 ? testing.AlgorithmParametersAllIntValues[i].Count : testing.AlgorithmParametersAllDoubleValues[i].Count; //если количество элементов в int values больше нуля, присваиваем количеству параметра количество int values элементов, иначе количество double values элементов
                                    if (iCount * kCount >= maxArea * minMaxArea) //если площадь данной комбинации в пределах минимальной, сохраняем комбинацию
                                    {
                                        //проверяем есть ли уже такая комбинация, чтобы не записать одну и ту же несколько раз
                                        bool isFind = parametersCombination.Where(j => (j[0] == i && j[1] == k) || (j[0] == k && j[1] == i)).Any();
                                        if (isFind == false)
                                        {
                                            parametersCombination.Add(new int[2] { i, k }); //запоминаем комбинацию
                                        }
                                    }
                                }
                            }
                        }

                        //определяем волатильность критерия оценки для каждой комбинации параметров
                        List<double> averageVolatilityParametersCombination = new List<double>(); //средняя волатильность на еденицу параметра (суммарная волатильность/количество элементов) для комбинаций параметров
                        for (int i = 0; i < parametersCombination.Count; i++)
                        {
                            //parametersCombination[i][0] - индекс первого параметра комбинации
                            //parametersCombination[i][1] - индекс второго параметра комбинации
                            bool xParameterIsInt = false; //параметр типа int, true - int, false - double
                            bool yParameterIsInt = false; //параметр типа int, true - int, false - double
                            int xParameterCountValues = 0; //количество элементов в параметре X
                                                           //если количество элементов в int values больше нуля, присваиваем количеству параметра количество int values элементов, иначе количество double values элементов
                            if (testing.AlgorithmParametersAllIntValues[parametersCombination[i][0]].Count > 0)
                            {
                                xParameterCountValues = testing.AlgorithmParametersAllIntValues[parametersCombination[i][0]].Count;
                                xParameterIsInt = true;
                            }
                            else
                            {
                                xParameterCountValues = testing.AlgorithmParametersAllDoubleValues[parametersCombination[i][0]].Count;
                            }

                            int yParameterCountValues = 0; //количество элементов в параметре Y
                                                           //если количество элементов в int values больше нуля, присваиваем количеству параметра количество int values элементов, иначе количество double values элементов
                            if (testing.AlgorithmParametersAllIntValues[parametersCombination[i][1]].Count > 0)
                            {
                                yParameterCountValues = testing.AlgorithmParametersAllIntValues[parametersCombination[i][1]].Count;
                                yParameterIsInt = true;
                            }
                            else
                            {
                                yParameterCountValues = testing.AlgorithmParametersAllDoubleValues[parametersCombination[i][1]].Count;
                            }

                            double amountVolatility = 0; //суммарная волатильность
                            int countIncreaseVolatility = 0; //количество прибавлений суммарной волатильности. На это значение будет делиться суммарная волатильность для получения средней
                                                             //перебираем все testRun-ы слева направо, переходя на следующую строку, и суммируем разности соседних тестов взятые по модулю
                            for (int x = 0; x < xParameterCountValues; x++)
                            {
                                for (int y = 1; y < yParameterCountValues; y++)
                                {
                                    //находим testRun-ы со значениями параметров x и y - 1, а так же x и y
                                    int indexXParameter = parametersCombination[i][0]; //индекс 1-го параметра комбинации (в параметрах алгоритма)
                                    int indexYParameter = parametersCombination[i][1]; //индекс 2-го параметра комбинации (в параметрах алгоритма)
                                    //значение параметра X текущей комбинации параметров
                                    int xParameterValueInt = xParameterIsInt ? testing.AlgorithmParametersAllIntValues[indexXParameter][x] : 0;
                                    double xParameterValueDouble = xParameterIsInt ? 0 : testing.AlgorithmParametersAllDoubleValues[indexXParameter][x];
                                    //значение параметра Y текущей комбинации параметров
                                    int yParameterValueInt = yParameterIsInt ? testing.AlgorithmParametersAllIntValues[indexYParameter][y] : 0;
                                    double yParameterValueDouble = yParameterIsInt ? 0 : testing.AlgorithmParametersAllDoubleValues[indexYParameter][y];
                                    //значение параметра Y - 1 текущей комбинации параметров
                                    int yDecrementParameterValueInt = yParameterIsInt ? testing.AlgorithmParametersAllIntValues[indexYParameter][y - 1] : 0;
                                    double yDecrementParameterValueDouble = yParameterIsInt ? 0 : testing.AlgorithmParametersAllDoubleValues[indexYParameter][y - 1];

                                    List<TestRun> previousTestRuns = new List<TestRun>(); //список с testRun-ми, у которых имеются значения параметров x и y - 1
                                    List<TestRun> nextTestRuns = new List<TestRun>(); //список с testRun-ми, у которых имеются значения параметров x и y
                                    //далее поиск с помощью where() testRun-ов с комбинациями значений параметров x и y - 1, x и y, исходя из того в каком списке находится каждый из параметров комбинации и какого типа
                                    //новое
                                    if (xParameterIsInt == true && yParameterIsInt == true)
                                    {
                                        previousTestRuns = testBatch.OptimizationTestRuns.Where(j => j.AlgorithmParameterValues[indexXParameter].IntValue == xParameterValueInt && j.AlgorithmParameterValues[indexYParameter].IntValue == yDecrementParameterValueInt).ToList();
                                        nextTestRuns = testBatch.OptimizationTestRuns.Where(j => j.AlgorithmParameterValues[indexXParameter].IntValue == xParameterValueInt && j.AlgorithmParameterValues[indexYParameter].IntValue == yParameterValueInt).ToList();
                                    }
                                    if (xParameterIsInt == true && yParameterIsInt == false)
                                    {
                                        previousTestRuns = testBatch.OptimizationTestRuns.Where(j => j.AlgorithmParameterValues[indexXParameter].IntValue == xParameterValueInt && j.AlgorithmParameterValues[indexYParameter].DoubleValue == yDecrementParameterValueDouble).ToList();
                                        nextTestRuns = testBatch.OptimizationTestRuns.Where(j => j.AlgorithmParameterValues[indexXParameter].IntValue == xParameterValueInt && j.AlgorithmParameterValues[indexYParameter].DoubleValue == yParameterValueDouble).ToList();
                                    }
                                    if (xParameterIsInt == false && yParameterIsInt == true)
                                    {
                                        previousTestRuns = testBatch.OptimizationTestRuns.Where(j => j.AlgorithmParameterValues[indexXParameter].DoubleValue == xParameterValueDouble && j.AlgorithmParameterValues[indexYParameter].IntValue == yDecrementParameterValueInt).ToList();
                                        nextTestRuns = testBatch.OptimizationTestRuns.Where(j => j.AlgorithmParameterValues[indexXParameter].DoubleValue == xParameterValueDouble && j.AlgorithmParameterValues[indexYParameter].IntValue == yParameterValueInt).ToList();
                                    }
                                    if (xParameterIsInt == false && yParameterIsInt == false)
                                    {
                                        previousTestRuns = testBatch.OptimizationTestRuns.Where(j => j.AlgorithmParameterValues[indexXParameter].DoubleValue == xParameterValueDouble && j.AlgorithmParameterValues[indexYParameter].DoubleValue == yDecrementParameterValueDouble).ToList();
                                        nextTestRuns = testBatch.OptimizationTestRuns.Where(j => j.AlgorithmParameterValues[indexXParameter].DoubleValue == xParameterValueDouble && j.AlgorithmParameterValues[indexYParameter].DoubleValue == yParameterValueDouble).ToList();
                                    }

                                    //получаем значение волатильности для всех testRun с текущей комбинацией параметров TopModelEvaluationCriteriaIndex
                                    for (int u = 0; u < previousTestRuns.Count; u++)
                                    {
                                        //прибавляем волатильность между соседними testRun-ми к суммарной волатильности
                                        amountVolatility += Math.Abs(nextTestRuns[u].EvaluationCriteriaValues[testing.TopModelEvaluationCriteriaIndex].DoubleValue - previousTestRuns[u].EvaluationCriteriaValues[testing.TopModelEvaluationCriteriaIndex].DoubleValue);
                                        countIncreaseVolatility++;
                                    }
                                }
                            }

                            //перебираем все testRun-ы сверху-вниз, переходя на следующую колонку, и суммируем разности соседних тестов взятые по модулю
                            for (int y = 0; y < yParameterCountValues; y++)
                            {
                                for (int x = 1; x < xParameterCountValues; x++)
                                {
                                    //находим testRun-ы со значениями параметров x и y - 1, а так же x и y
                                    int indexXParameter = parametersCombination[i][0]; //индекс 1-го параметра комбинации (в параметрах алгоритма)
                                    int indexYParameter = parametersCombination[i][1]; //индекс 2-го параметра комбинации (в параметрах алгоритма)
                                    //значение параметра X текущей комбинации параметров
                                    int xParameterValueInt = xParameterIsInt ? testing.AlgorithmParametersAllIntValues[indexXParameter][x] : 0;
                                    double xParameterValueDouble = xParameterIsInt ? 0 : testing.AlgorithmParametersAllDoubleValues[indexXParameter][x];
                                    //значение параметра Y текущей комбинации параметров
                                    int yParameterValueInt = yParameterIsInt ? testing.AlgorithmParametersAllIntValues[indexYParameter][y] : 0;
                                    double yParameterValueDouble = yParameterIsInt ? 0 : testing.AlgorithmParametersAllDoubleValues[indexYParameter][y];
                                    //значение параметра X - 1 текущей комбинации параметров
                                    int xDecrementParameterValueInt = xParameterIsInt ? testing.AlgorithmParametersAllIntValues[indexXParameter][x - 1] : 0;
                                    double xDecrementParameterValueDouble = xParameterIsInt ? 0 : testing.AlgorithmParametersAllDoubleValues[indexXParameter][x - 1];

                                    List<TestRun> previousTestRuns = new List<TestRun>(); //список с testRun-ми, у которых имеются значения параметров x и y - 1
                                    List<TestRun> nextTestRuns = new List<TestRun>(); //список с testRun-ми, у которых имеются значения параметров x и y
                                    //далее поиск с помощью where() testRun-ов с комбинациями значений параметров x и y - 1, x и y, исходя из того в каком списке находится каждый из параметров комбинации и какого типа
                                    //новое
                                    if (xParameterIsInt == true && yParameterIsInt == true)
                                    {
                                        previousTestRuns = testBatch.OptimizationTestRuns.Where(j => j.AlgorithmParameterValues[indexXParameter].IntValue == xDecrementParameterValueInt && j.AlgorithmParameterValues[indexYParameter].IntValue == yParameterValueInt).ToList();
                                        nextTestRuns = testBatch.OptimizationTestRuns.Where(j => j.AlgorithmParameterValues[indexXParameter].IntValue == xParameterValueInt && j.AlgorithmParameterValues[indexYParameter].IntValue == yParameterValueInt).ToList();
                                    }
                                    if (xParameterIsInt == true && yParameterIsInt == false)
                                    {
                                        previousTestRuns = testBatch.OptimizationTestRuns.Where(j => j.AlgorithmParameterValues[indexXParameter].IntValue == xDecrementParameterValueInt && j.AlgorithmParameterValues[indexYParameter].DoubleValue == yParameterValueDouble).ToList();
                                        nextTestRuns = testBatch.OptimizationTestRuns.Where(j => j.AlgorithmParameterValues[indexXParameter].IntValue == xParameterValueInt && j.AlgorithmParameterValues[indexYParameter].DoubleValue == yParameterValueDouble).ToList();
                                    }
                                    if (xParameterIsInt == false && yParameterIsInt == true)
                                    {
                                        previousTestRuns = testBatch.OptimizationTestRuns.Where(j => j.AlgorithmParameterValues[indexXParameter].DoubleValue == xDecrementParameterValueDouble && j.AlgorithmParameterValues[indexYParameter].IntValue == yParameterValueInt).ToList();
                                        nextTestRuns = testBatch.OptimizationTestRuns.Where(j => j.AlgorithmParameterValues[indexXParameter].DoubleValue == xParameterValueDouble && j.AlgorithmParameterValues[indexYParameter].IntValue == yParameterValueInt).ToList();
                                    }
                                    if (xParameterIsInt == false && yParameterIsInt == true)
                                    {
                                        previousTestRuns = testBatch.OptimizationTestRuns.Where(j => j.AlgorithmParameterValues[indexXParameter].DoubleValue == xDecrementParameterValueDouble && j.AlgorithmParameterValues[indexYParameter].DoubleValue == yParameterValueDouble).ToList();
                                        nextTestRuns = testBatch.OptimizationTestRuns.Where(j => j.AlgorithmParameterValues[indexXParameter].DoubleValue == xParameterValueDouble && j.AlgorithmParameterValues[indexYParameter].DoubleValue == yParameterValueDouble).ToList();
                                    }

                                    //получаем значение волатильности для всех testRun с текущей комбинацией параметров TopModelEvaluationCriteriaIndex
                                    for (int u = 0; u < previousTestRuns.Count; u++)
                                    {
                                        //прибавляем волатильность между соседними testRun-ми к суммарной волатильности
                                        amountVolatility += Math.Abs(nextTestRuns[u].EvaluationCriteriaValues[testing.TopModelEvaluationCriteriaIndex].DoubleValue - previousTestRuns[u].EvaluationCriteriaValues[testing.TopModelEvaluationCriteriaIndex].DoubleValue);
                                        countIncreaseVolatility++;
                                    }
                                }
                            }

                            averageVolatilityParametersCombination.Add(amountVolatility / (countIncreaseVolatility / 2)); //записываем среднюю волатильность для данной комбинации (делим на 2, т.к. мы дважды проходились по плоскости и суммировали общую волатильность: слева-направо и вниз, а так же сверху-вниз и направо)
                        }

                        //выбираем комбинацию параметров с самой низкой средней волатильностью
                        int indexMinAverageVolatility = 0;
                        double minAverageVolatility = averageVolatilityParametersCombination[0];
                        for (int i = 1; i < averageVolatilityParametersCombination.Count; i++)
                        {
                            if (averageVolatilityParametersCombination[i] < minAverageVolatility)
                            {
                                indexMinAverageVolatility = i;
                                minAverageVolatility = averageVolatilityParametersCombination[i];
                            }
                        }

                        //формируем первый параметр оси
                        AxesParameter axesParameterX = new AxesParameter();
                        axesParameterX.AlgorithmParameter = testing.Algorithm.AlgorithmParameters[parametersCombination[indexMinAverageVolatility][0]];
                        //формируем второй параметр оси
                        AxesParameter axesParameterY = new AxesParameter();
                        axesParameterY.AlgorithmParameter = testing.Algorithm.AlgorithmParameters[parametersCombination[indexMinAverageVolatility][1]];

                        List<AxesParameter> axesTopModelSearchPlane = new List<AxesParameter>();
                        axesTopModelSearchPlane.Add(axesParameterX);
                        axesTopModelSearchPlane.Add(axesParameterY);
                        testBatch.AxesTopModelSearchPlane = axesTopModelSearchPlane;
                    }

                    //оси определили, далее определяем размер группы соседних тестов
                    //определяем размер двумерной плоскости с выбранными осями
                    //количество значений параметра X
                    bool isXAxisIntValue = testBatch.AxesTopModelSearchPlane[0].AlgorithmParameter.ParameterValueType.Id == 1 ? true : false; //тип значения параметра
                    int xAxisParameterIndex = testing.Algorithm.AlgorithmParameters.IndexOf(testing.Algorithm.AlgorithmParameters.Where(j => j == testBatch.AxesTopModelSearchPlane[0].AlgorithmParameter).First()); //индекс параметра в списке параметров
                    int xAxisCountParameterValue = isXAxisIntValue ? testing.AlgorithmParametersAllIntValues[xAxisParameterIndex].Count : testing.AlgorithmParametersAllDoubleValues[xAxisParameterIndex].Count; //запоминаем количество значений параметра

                    //количество значений параметра Y
                    bool isYAxisIntValue = testBatch.AxesTopModelSearchPlane[1].AlgorithmParameter.ParameterValueType.Id == 1 ? true : false; //тип значения параметра
                    int yAxisParameterIndex = testing.Algorithm.AlgorithmParameters.IndexOf(testing.Algorithm.AlgorithmParameters.Where(j => j == testBatch.AxesTopModelSearchPlane[1].AlgorithmParameter).First()); //индекс параметра в списке параметров
                    int yAxisCountParameterValue = isYAxisIntValue ? testing.AlgorithmParametersAllIntValues[yAxisParameterIndex].Count : testing.AlgorithmParametersAllDoubleValues[yAxisParameterIndex].Count; //запоминаем количество значений параметра

                    //определяем размер группы соседних тестов
                    double groupArea = xAxisCountParameterValue * yAxisCountParameterValue * (testing.SizeNeighboursGroupPercent / 100); //площадь группы соседних тестов
                    int xAxisSize = (int)Math.Round(Math.Sqrt(groupArea)); //размер группы по оси X
                    int yAxisSize = xAxisSize; //размер группы по оси Y
                                               //если размер сторон группы меньше 2, устанавливаем в 2, если размер оси позволяет
                    xAxisSize = xAxisSize < 2 && xAxisCountParameterValue >= 2 ? 2 : xAxisSize;
                    yAxisSize = yAxisSize < 2 && yAxisCountParameterValue >= 2 ? 2 : yAxisSize;
                    //если размер сторон группы меньше 1, устанавливаем в 1
                    xAxisSize = xAxisSize < 1 ? 1 : xAxisSize;
                    yAxisSize = yAxisSize < 1 ? 1 : yAxisSize;
                    //если одна из сторон группы больше размера оси
                    if (xAxisSize > xAxisCountParameterValue || yAxisSize > yAxisCountParameterValue)
                    {
                        if (xAxisSize > xAxisCountParameterValue)
                        {
                            xAxisSize = xAxisCountParameterValue; //устанавливаем размер стороны группы в размер оси
                            yAxisSize = (int)Math.Round(groupArea / xAxisSize); //размер второй стороны высчитываем как площадь группы / размер первой оси
                        }
                        if (yAxisSize > yAxisCountParameterValue)
                        {
                            yAxisSize = yAxisCountParameterValue; //устанавливаем размер стороны группы в размер оси
                            xAxisSize = (int)Math.Round(groupArea / yAxisSize); //размер второй стороны высчитываем как площадь группы / размер первой оси
                        }
                    }

                    //формируем список с комбинациями параметров тестов групп
                    List<List<int[]>> groupsParametersCombinations = new List<List<int[]>>(); //список групп, група содержит список тестов, тест содержит массив индексов значений из AlgorithmParametersAllIntValues или AlgorithmParametersAllDoubleValues для параметров алгоритма
                    /*новое
                    groupsParameterCombinations{
                        [0](1-я группа) => {
                            [0](1-й тест группы) => индексы_значений_алгоритма{ 1, 5 }
                        }
                    }
                    */
                    /*старое
                    groupsParameterCombinations{
                        [0](1-я группа) => {
                            [0](1-й тест группы) => {
                                [0] => индексы_значений_индикаторов{ 1, 5 },
                                [1] => индексы_значений_алгоритма{ 4, 2 }
                            }
                        }
                    }
                    */
                    //формируем группы с комбинациями параметров плоскости поиска топ-модели
                    //проходим по оси X столько раз, сколько помещается размер стороны группы по оси X
                    for (int x = 0; x < xAxisCountParameterValue - (xAxisSize - 1); x++)
                    {
                        //проходим по оси Y столько раз, сколько помещается размер стороны группы по оси Y
                        for (int y = 0; y < yAxisCountParameterValue - (yAxisSize - 1); y++)
                        {
                            List<int[]> currentGroup = new List<int[]>();
                            //проходим по всем элементам группы
                            for (int par1 = x; par1 < x + xAxisSize; par1++)
                            {
                                for (int par2 = y; par2 < y + yAxisSize; par2++)
                                {
                                    int[] testRunParametersCombination = new int[testing.Algorithm.AlgorithmParameters.Count]; //индексы значений алгоритма

                                    //записываем параметр оси X
                                    int xParameterIndex = testing.Algorithm.AlgorithmParameters.IndexOf(testing.Algorithm.AlgorithmParameters.Where(j => j == testBatch.AxesTopModelSearchPlane[0].AlgorithmParameter).First()); //индекс параметра в списке параметров
                                    testRunParametersCombination[xParameterIndex] = par1; //записываем индекс значения параметра в значениях параметра алгоритма

                                    //записываем параметр оси Y
                                    int yParameterIndex = testing.Algorithm.AlgorithmParameters.IndexOf(testing.Algorithm.AlgorithmParameters.Where(j => j == testBatch.AxesTopModelSearchPlane[1].AlgorithmParameter).First()); //индекс параметра в списке параметров
                                    testRunParametersCombination[yParameterIndex] = par2; //записываем индекс значения параметра в значениях параметра алгоритма
                                    currentGroup.Add(testRunParametersCombination);
                                }
                            }
                            groupsParametersCombinations.Add(currentGroup);
                        }
                    }
                    //после того как сформированы группы только с двумя параметрами (осей), на их основе создаются группы с оставшимися (не входящими в оси) параметрами: берется группа, и полностью дублируется для каждого значения параметра и в элементы этой группы вставляется значение параметра
                    //формируем группы с оставшимися параметрами
                    //проходим по всем параметрами
                    for (int i = 0; i < testing.AlgorithmParametersAllIntValues.Length; i++)
                    {
                        bool isXParameter = false;
                        if (testing.Algorithm.AlgorithmParameters[i].Id == testBatch.AxesTopModelSearchPlane[0].AlgorithmParameter.Id)
                        {
                            isXParameter = true;
                        }

                        bool isYParameter = false;
                        if (testing.Algorithm.AlgorithmParameters[i].Id == testBatch.AxesTopModelSearchPlane[1].AlgorithmParameter.Id)
                        {
                            isYParameter = true;
                        }

                        //если параметр не X и не Y
                        if (isXParameter == false && isYParameter == false)
                        {
                            //формируем новые группы с комбинациями значений текущего параметра
                            List<List<int[]>> newGroupsParametersCombinations = new List<List<int[]>>();
                            int countValues = testing.AlgorithmParametersAllIntValues[i].Count; //количество значений параметра
                                                                                        //проходим по всем значениям параметра
                            for (int k = 0; k < countValues; k++)
                            {
                                //копируем старую группу, и в каждую комбинацию параметров вставляем значение текущего параметра
                                List<List<int[]>> currentNewGroupsParametersCombinations = new List<List<int[]>>();
                                //проходим по всем старым группам
                                for (int u = 0; u < groupsParametersCombinations.Count; u++)
                                {
                                    List<int[]> newParameterCombinations = new List<int[]>();
                                    //проходим по всем комбинациям группы
                                    for (int r = 0; r < groupsParametersCombinations[u].Count; r++)
                                    {
                                        int[] newParameterCombination = new int[testing.Algorithm.AlgorithmParameters.Count];
                                        //копируем параметры алгоритма
                                        for (int algorithmParameterIndex = 0; algorithmParameterIndex < groupsParametersCombinations[u][r].Length; algorithmParameterIndex++)
                                        {
                                            newParameterCombination[algorithmParameterIndex] = groupsParametersCombinations[u][r][algorithmParameterIndex];
                                        }
                                        newParameterCombination[i] = k; //вставляем индекс значения текущего параметра

                                        newParameterCombinations.Add(newParameterCombination);
                                    }
                                    currentNewGroupsParametersCombinations.Add(newParameterCombinations); //формируем группы с текущим значением текущего параметра
                                }
                                newGroupsParametersCombinations.AddRange(currentNewGroupsParametersCombinations); //добавляем в новые группы, группы с текущим значением текущего парамета
                            }
                            groupsParametersCombinations = newGroupsParametersCombinations; //обновляем все группы. Теперь для нового параметра будут использоваться группы с новым количеством комбинаций параметров
                        }
                    }

                    //формируем список групп с тестами на основе групп с комбинациями параметров теста
                    List<TestRun[]> testRunsGroups = new List<TestRun[]>();
                    for (int i = 0; i < groupsParametersCombinations.Count; i++)
                    {
                        TestRun[] testRunsGroup = new TestRun[groupsParametersCombinations[i].Count]; //группа с testRun-ами
                        for (int k = 0; k < groupsParametersCombinations[i].Count; k++)
                        {
                            //находим testRun с текущей комбинацией параметров
                            int tRunIndex = 0;
                            bool isTestRunFind = false;
                            while (tRunIndex < testBatch.OptimizationTestRuns.Count && isTestRunFind == false)
                            {
                                bool isAllEqual = true; //все ли значения параметров testRun-а равны текущей комбинации

                                //проходим по всем параметрам алгоритма, и сравниваем значения параметров алгоритма текущей комбинации со значениями параметров алгоритма текущего testRun-а
                                for (int algParIndex = 0; algParIndex < groupsParametersCombinations[i][k].Length; algParIndex++)
                                {
                                    if (testBatch.OptimizationTestRuns[tRunIndex].AlgorithmParameterValues[algParIndex].AlgorithmParameter.ParameterValueType.Id == 1) //если параметр тип int
                                    {
                                        isAllEqual = testBatch.OptimizationTestRuns[tRunIndex].AlgorithmParameterValues[algParIndex].IntValue != testing.AlgorithmParametersAllIntValues[algParIndex][groupsParametersCombinations[i][k][algParIndex]] ? false : isAllEqual; //если значение параметра testRun != значению параметра текущей комбинации, отмечаем что isAllEqual == false;
                                    }
                                    else //параметр типа double
                                    {
                                        isAllEqual = testBatch.OptimizationTestRuns[tRunIndex].AlgorithmParameterValues[algParIndex].DoubleValue != testing.AlgorithmParametersAllDoubleValues[algParIndex][groupsParametersCombinations[i][k][algParIndex]] ? false : isAllEqual; //если значение параметра testRun != значению параметра текущей комбинации, отмечаем что isAllEqual == false;
                                    }
                                }
                                if (isAllEqual) //если все параметры текущей комбинации равны параметрам текущего testRun-а, отмечаем что testRun найден
                                {
                                    isTestRunFind = true;
                                }
                                else
                                {
                                    tRunIndex++;
                                }
                            }
                            testRunsGroup[k] = testBatch.OptimizationTestRuns[tRunIndex]; //добавляем testRun в группу соседних тестов
                        }
                        testRunsGroups.Add(testRunsGroup); //добавляем в группы, группу соседних тестов
                    }

                    //формируем список со средними значениями критерия оценки групп
                    List<double> averageGroupsValues = new List<double>();
                    //проходим по всем группам
                    for (int i = 0; i < testRunsGroups.Count; i++)
                    {
                        double totalGroupValue = 0;
                        for (int k = 0; k < testRunsGroups[i].Length; k++)
                        {
                            totalGroupValue += testRunsGroups[i][k].EvaluationCriteriaValues[testing.TopModelEvaluationCriteriaIndex].DoubleValue;
                        }
                        averageGroupsValues.Add(totalGroupValue / testRunsGroups[i].Length);
                    }

                    //сортируем группы по среднему значению критерия оценки в порядке убывания
                    TestRun[] saveGroup; //элемент списка для сохранения после удаления из списка
                    double saveValue; //элемент списка для сохранения после удаления из списка
                    for (int i = 0; i < averageGroupsValues.Count; i++)
                    {
                        for (int k = 0; k < averageGroupsValues.Count - 1; k++)
                        {
                            if (averageGroupsValues[k] < averageGroupsValues[k + 1])
                            {
                                saveGroup = testRunsGroups[k];
                                testRunsGroups[k] = testRunsGroups[k + 1];
                                testRunsGroups[k + 1] = saveGroup;

                                saveValue = averageGroupsValues[k];
                                averageGroupsValues[k] = averageGroupsValues[k + 1];
                                averageGroupsValues[k + 1] = saveValue;
                            }
                        }
                    }

                    bool isTopModelFind = false;
                    int groupIndex = 0;
                    //проходим по всем группам, сортируем тесты в группе в порядке убывания критерия оценки, и ищем тест в группе который соответствует фильтрам
                    while (isTopModelFind == false && groupIndex < testRunsGroups.Count)
                    {
                        //сортируем тесты в группе в порядке убывания критерия оценки
                        for (int i = 0; i < testRunsGroups[groupIndex].Length; i++)
                        {
                            for (int k = 0; k < testRunsGroups[groupIndex].Length - 1; k++)
                            {
                                if (testRunsGroups[groupIndex][k].EvaluationCriteriaValues[testing.TopModelEvaluationCriteriaIndex].DoubleValue < testRunsGroups[groupIndex][k + 1].EvaluationCriteriaValues[testing.TopModelEvaluationCriteriaIndex].DoubleValue)
                                {
                                    TestRun saveTestRun = testRunsGroups[groupIndex][k];
                                    testRunsGroups[groupIndex][k] = testRunsGroups[groupIndex][k + 1];
                                    testRunsGroups[groupIndex][k + 1] = saveTestRun;
                                }
                            }
                        }
                        //проходим по тестам группы, и ищем первый, который соответствует фильтрам
                        int tRunIndex = 0;
                        while (isTopModelFind == false && tRunIndex < testRunsGroups[groupIndex].Length)
                        {
                            //проходим по всем фильтрам
                            bool isFilterFail = false;
                            foreach (TopModelFilter topModelFilter in testing.TopModelCriteria.TopModelFilters)
                            {
                                if (topModelFilter.CompareSign == CompareSign.GetMore()) //знак сравнения фильтра Больше
                                {
                                    if (testRunsGroups[groupIndex][tRunIndex].EvaluationCriteriaValues.Where(j => j.EvaluationCriteria == topModelFilter.EvaluationCriteria).First().DoubleValue <= topModelFilter.Value)
                                    {
                                        isFilterFail = true;
                                    }
                                }
                                else //знак сравнения фильтра Меньше
                                {
                                    if (testRunsGroups[groupIndex][tRunIndex].EvaluationCriteriaValues.Where(j => j.EvaluationCriteria == topModelFilter.EvaluationCriteria).First().DoubleValue >= topModelFilter.Value)
                                    {
                                        isFilterFail = true;
                                    }
                                }
                            }
                            //если testRun удовлетворяет всем фильтрам, записываем его как топ-модель
                            if (isFilterFail == false)
                            {
                                testBatch.SetTopModel(testRunsGroups[groupIndex][tRunIndex]);
                                isTopModelFind = true;
                            }
                            tRunIndex++;
                        }
                        groupIndex++;
                    }
                }
            }
            else //если поиск топ-модели не учитывает соседей, ищем топ-модель среди оптимизационных тестов
            {
                //проходим по всем оптимизационным тестами, и ищем топ-модель, которая соответствует фильтрам
                bool isFirstTopModelFind = false; //найден ли первый тест, удовлетворяющий условиям фильтров, чтобы понять есть с чем сравнивать или нет
                double topModelValue = 0;
                TestRun topModelTestRun = new TestRun();
                foreach (TestRun testRun in testBatch.OptimizationTestRuns)
                {
                    //проверяем, соответствует ли текущий testRun условиям фильтров
                    bool isFilterFail = false;
                    //проходим по всем фильтрам
                    foreach (TopModelFilter topModelFilter in testing.TopModelCriteria.TopModelFilters)
                    {
                        if (topModelFilter.CompareSign == CompareSign.GetMore()) //знак сравнения фильтра Больше
                        {
                            if (testRun.EvaluationCriteriaValues.Where(j => j.EvaluationCriteria == topModelFilter.EvaluationCriteria).First().DoubleValue <= topModelFilter.Value)
                            {
                                isFilterFail = true;
                            }
                        }
                        else //знак сравнения фильтра Меньше
                        {
                            if (testRun.EvaluationCriteriaValues.Where(j => j.EvaluationCriteria == topModelFilter.EvaluationCriteria).First().DoubleValue >= topModelFilter.Value)
                            {
                                isFilterFail = true;
                            }
                        }
                    }

                    if (isFilterFail == false) //если testRun удовлетворяет всем фильтрам
                    {
                        if (isFirstTopModelFind == false) //если первая топ-модель еще не найдена, записываем текущий testRun как топ-модель
                        {
                            topModelTestRun = testRun;
                            topModelValue = testRun.EvaluationCriteriaValues[testing.TopModelEvaluationCriteriaIndex].DoubleValue;
                            isFirstTopModelFind = true;
                        }
                        else //если уже есть топ-модель с которой можно сравнивать, сравниваем текущий testRun с топ-моделью
                        {
                            if (testRun.EvaluationCriteriaValues[testing.TopModelEvaluationCriteriaIndex].DoubleValue > topModelValue)
                            {
                                topModelTestRun = testRun;
                                topModelValue = testRun.EvaluationCriteriaValues[testing.TopModelEvaluationCriteriaIndex].DoubleValue;
                            }
                        }
                    }
                }
                if (isFirstTopModelFind)
                {
                    testBatch.SetTopModel(topModelTestRun);
                }
            }


            //определяем статистическую значимость
            int lossCount = 0;
            double lossMoney = 0;
            int profitCount = 0;
            double profitMoney = 0;
            //проходим по всем testRun-ам
            for (int i = 0; i < testBatch.OptimizationTestRuns.Count; i++)
            {
                double testRunProfit = testBatch.OptimizationTestRuns[i].EvaluationCriteriaValues.Where(j => j.EvaluationCriteria.Id == 1).First().DoubleValue; //EvaluationCriteria.Id == 1 - Чистая доходность
                if (testRunProfit < 0)
                {
                    lossCount++;
                    lossMoney += testRunProfit;
                }
                else
                {
                    profitCount++;
                    profitMoney += testRunProfit;
                }
            }
            //записываем статистическую значимость
            string[] totalTests = new string[3] { ModelFunctions.SplitDigitsInt(lossCount + profitCount), "100.0%", ModelFunctions.SplitDigitsDouble(lossMoney + profitMoney) + " " + testing.DefaultCurrency.Name };
            string[] lossTests = new string[3] { ModelFunctions.SplitDigitsInt(lossCount), Math.Round((double)lossCount / (lossCount + profitCount) * 100, 1).ToString() + "%", ModelFunctions.SplitDigitsDouble(lossMoney) + " " + testing.DefaultCurrency.Name };
            string[] profitTests = new string[3] { ModelFunctions.SplitDigitsInt(profitCount), Math.Round((double)profitCount / (lossCount + profitCount) * 100, 1).ToString() + "%", ModelFunctions.SplitDigitsDouble(profitMoney) + " " + testing.DefaultCurrency.Name };

            testBatch.StatisticalSignificance.Add(totalTests);
            testBatch.StatisticalSignificance.Add(lossTests);
            testBatch.StatisticalSignificance.Add(profitTests);

            testBatch.IsTopModelDetermining = true; //отмечаем что определили топ-модель для данного testBatch

            if (testing.IsForwardTesting && testBatch.IsTopModelWasFind) //если это форвардное тестирование и топ-модель была найдена, записываем параметры для форвардного теста
            {
                testBatch.ForwardTestRun.AlgorithmParameterValues = testBatch.TopModelTestRun.AlgorithmParameterValues;
                if (testing.IsForwardDepositTrading) //если это форвардное тестирование с торговлей депозитом, записываем параметры для форвардного теста с торговлей депозитом
                {
                    testBatch.ForwardTestRunDepositTrading.AlgorithmParameterValues = testBatch.TopModelTestRun.AlgorithmParameterValues;
                }
            }
        }

        private void TestingEnding(bool isSuccess, Testing testing) //оповещение представления о том что тестирование закончено, isSucces - флаг того что тестирование выполнено успешно.
        {
            TestingProgress testingProgress = new TestingProgress { StepDescription = "Шаг 3/4:  Симуляция тестов", StepTasksCount = 1, CompletedStepTasksCount = 1, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = TimeSpan.FromSeconds(1), CancelPossibility = false, IsFinishSimulation = true, IsSuccessSimulation = isSuccess, IsFinish = false };
            if (isSuccess)
            {
                testing.TestingDuration = ModelTesting.StopwatchTesting.Elapsed; //записываем длительность тестирования
                testing.DateTimeSimulationEnding = DateTime.Now; //записываем дату и время завершения выполнения симуляции тестирования
                testingProgress.Testing = testing;
            }
            else //если тестирование не выполнено успешно, отмечаем что тестирование завершено, и переходить на запись результатов не нужно
            {
                testingProgress.IsFinish = true;
            }
            DispatcherInvoke((Action)(() => {
                _mainCommunicationChannel.TestingProgress.Clear();
                _mainCommunicationChannel.TestingProgress.Add(testingProgress);
            }));
        }

        public AlgorithmIndicatorValues AlgorithmIndicatorCalculate(Testing testing, DataSourceCandles dataSourceCandles, AlgorithmIndicator algorithmIndicator, List<AlgorithmParameterValue> algorithmParameterValues) //возвращает вычисленные значения индикатора алгоритма для свечек из dataSourceCandles
        {
            AlgorithmIndicatorValues algorithmIndicatorValues = new AlgorithmIndicatorValues { AlgorithmIndicator = algorithmIndicator, Values = new AlgorithmIndicatorValue[dataSourceCandles.Candles.Length][] };
            int algorithmIndicatorIndex = testing.Algorithm.AlgorithmIndicators.IndexOf(algorithmIndicator); //индекс индикатора алгоритма
            for (int i = 0; i < dataSourceCandles.Candles.Length; i++)
            {
                algorithmIndicatorValues.Values[i] = new AlgorithmIndicatorValue[dataSourceCandles.Candles[i].Length]; //определяем количество значений индикатора в файле
                for(int k = 0; k < dataSourceCandles.Candles[i].Length; k++)
                {
                    int[] indicatorParametersIntValues = new int[algorithmIndicator.IndicatorParameterRanges.Count];
                    double[] indicatorParametersDoubleValues = new double[algorithmIndicator.IndicatorParameterRanges.Count];
                    for(int u = 0; u < algorithmIndicator.IndicatorParameterRanges.Count; u++)
                    {
                        indicatorParametersIntValues[u] = algorithmParameterValues.Where(j => j.AlgorithmParameter.Id == algorithmIndicator.IndicatorParameterRanges[u].AlgorithmParameter.Id).First().IntValue;
                        indicatorParametersDoubleValues[u] = algorithmParameterValues.Where(j => j.AlgorithmParameter.Id == algorithmIndicator.IndicatorParameterRanges[u].AlgorithmParameter.Id).First().DoubleValue;
                    }
                    //копируем объект скомпилированного индикатора, чтобы из разных потоков не обращаться к одному объекту и к одним свойствам объекта
                    dynamic compiledIndicatorCopy = testing.CompiledIndicators[algorithmIndicatorIndex].Clone();
                    //вычисляем значение индикатора
                    IndicatorCalculateResult indicatorCalculateResult = compiledIndicatorCopy.Calculate(dataSourceCandles.Candles[i], k, indicatorParametersIntValues, indicatorParametersDoubleValues);
                    algorithmIndicatorValues.Values[i][k] = new AlgorithmIndicatorValue();
                    algorithmIndicatorValues.Values[i][k].IsNotOverIndex = indicatorCalculateResult.OverIndex == 0 ? true : false; //если не было превышения индекса устанавливаем true, иначе false
                    algorithmIndicatorValues.Values[i][k].Value = indicatorCalculateResult.Value;
                }
            }

            return algorithmIndicatorValues;
        }
    }
}

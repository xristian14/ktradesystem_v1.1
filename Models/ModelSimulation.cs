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
            //удаляем комментарии
            string scriptAlgorithmStr = algorithm.Script;
            while(scriptAlgorithmStr.IndexOf("/*") != -1 && scriptAlgorithmStr.IndexOf("*/") != -1)
            {
                scriptAlgorithmStr = scriptAlgorithmStr.Remove(scriptAlgorithmStr.IndexOf("/*"), scriptAlgorithmStr.IndexOf("*/") - scriptAlgorithmStr.IndexOf("/*") + 2);
            }
            //удаляем дублирующиеся пробелы
            StringBuilder scriptAlgorithm = ModelFunctions.RemoveDuplicateSpaces(scriptAlgorithmStr);
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
                        order.Price = ModelFunctions.RoundToIncrement(price, dataSourceForCalculate.PriceStep);
                        order.Count = count;
                        order.StartCount = count;

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
        /// <summary>
        /// Разделяет временной период на более котроткие временные интервалы, с отступом от начала offset, длительностью duration. Следующий интервал начнется с (даты начала предыдущего + spacing). Возвращает список с датами интервалов, в 0-м элементе - дата начала интервала, в 1-м - дата окончания интервала + 1 минута. isMinDuration - если true, интервал будет создан даже если в оставшийся период не помещается полная длительность, но помещается минимально допустимая длительность, размер которой определяется в настройках, id=2; - если false, будут создаваться интервалы только с полной длительностью duration.
        /// </summary>
        private List<DateTime[]> SplitPeriod(DateTimeDuration offset, DateTime startDateTime, DateTime endDateTime, DateTimeDuration duration, DateTimeDuration spacing, bool isMinDuration)
        {
            List<DateTime[]> intervals = new List<DateTime[]>();
            double step = 0.25; //шаг уменьшения длительности
            TimeSpan overDate = TimeSpan.FromMinutes(1);
            DateTime currentDateTime = startDateTime.AddYears(offset.Years).AddMonths(offset.Months).AddDays(offset.Days);
            TimeSpan minDuration = currentDateTime.AddYears(duration.Years).AddMonths(duration.Months).AddDays(duration.Days) - currentDateTime;
            minDuration = isMinDuration ? TimeSpan.FromDays(Math.Round(minDuration.TotalDays * (_modelData.Settings.Where(i => i.Id == 2).First().IntValue / 100.0))) : minDuration;
            while (currentDateTime + minDuration <= endDateTime)
            {
                //определяем длительность, которая помещается в оставшийся период
                double currentDurationPercent = 100;
                TimeSpan currentDuration = currentDateTime.AddYears(duration.Years).AddMonths(duration.Months).AddDays(duration.Days) - currentDateTime;
                if (isMinDuration)
                {
                    currentDuration = TimeSpan.FromDays(Math.Round((currentDateTime.AddYears(duration.Years).AddMonths(duration.Months).AddDays(duration.Days) - currentDateTime).TotalDays * (currentDurationPercent / 100.0)));
                    while (currentDateTime + currentDuration > endDateTime)
                    {
                        currentDurationPercent -= step;
                        currentDuration = TimeSpan.FromDays(Math.Round((currentDateTime.AddYears(duration.Years).AddMonths(duration.Months).AddDays(duration.Days) - currentDateTime).TotalDays * (currentDurationPercent / 100.0)));
                    }
                }
                
                DateTime currentEndDateTime = currentDateTime + currentDuration + overDate;
                intervals.Add(new DateTime[2] { new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second), new DateTime(currentEndDateTime.Year, currentEndDateTime.Month, currentEndDateTime.Day, currentEndDateTime.Hour, currentEndDateTime.Minute, currentEndDateTime.Second) });

                currentDateTime = currentDateTime.AddYears(spacing.Years).AddMonths(spacing.Months).AddDays(spacing.Days);
                minDuration = currentDateTime.AddYears(duration.Years).AddMonths(duration.Months).AddDays(duration.Days) - currentDateTime;
                minDuration = isMinDuration ? TimeSpan.FromDays(Math.Round(minDuration.TotalDays * (_modelData.Settings.Where(i => i.Id == 2).First().IntValue / 100.0))) : minDuration;
            }
            return intervals;
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
                //для форвардного теста, делаем длительности оптимизационных тестов полными, для не форвардного - минимально допустимыми
                //элементы списков с интервалами - массивы: в 0-м элементе дата начала временного интервала, в 1-м дата окончания
                List<DateTime[]> optimizationIntervals = testing.IsForwardTesting ? SplitPeriod(new DateTimeDuration { Years = 0, Months = 0, Days = 0 }, dataSourceGroup.StartPeriodTesting, dataSourceGroup.EndPeriodTesting, testing.DurationOptimizationTests, testing.OptimizationTestSpacing, false) : SplitPeriod(new DateTimeDuration { Years = 0, Months = 0, Days = 0 }, dataSourceGroup.StartPeriodTesting, dataSourceGroup.EndPeriodTesting, testing.DurationOptimizationTests, testing.OptimizationTestSpacing, true);
                List<DateTime[]> forwardIntervals = testing.IsForwardTesting ? SplitPeriod(testing.DurationOptimizationTests, dataSourceGroup.StartPeriodTesting, dataSourceGroup.EndPeriodTesting, testing.DurationForwardTest, testing.OptimizationTestSpacing, true) : new List<DateTime[]>();
                //для форвардного тестирования мог быть создан интервал оптимизационного теста, а на форвардный тест данного оптимизационного интервала оставшегося временного периода могло не хватить, тогда мы удаляем его
                if (testing.IsForwardTesting)
                {
                    int minCountIntervals = Math.Min(optimizationIntervals.Count, forwardIntervals.Count);
                    while(optimizationIntervals.Count > minCountIntervals)
                    {
                        optimizationIntervals.RemoveAt(optimizationIntervals.Count - 1);
                    }
                    while (forwardIntervals.Count > minCountIntervals)
                    {
                        forwardIntervals.RemoveAt(forwardIntervals.Count - 1);
                    }
                }
                
                //формируем тестовые связки для каждого временного интервала
                for(int t = 0; t < optimizationIntervals.Count; t++)
                {
                    //создаем testBatch
                    TestBatch testBatch = new TestBatch { Number = testing.TestBatches.Count + 1, DataSourceGroup = dataSourceGroup, DataSourceGroupIndex = testing.DataSourceGroups.IndexOf(dataSourceGroup), StatisticalSignificance = new List<double>(), IsTopModelDetermining = false, IsTopModelWasFind = false, OptimizationPerfectProfits = new List<PerfectProfit>(), ForwardPerfectProfits = new List<PerfectProfit>(), NeighboursTestRunNumbers = new List<int>() };

                    int testRunNumber = 1; //номер тестового прогона

                    //формируем оптимизационные тесты
                    List<TestRun> optimizationTestRuns = new List<TestRun>();
                    for (int i = 0; i < allCombinations.Count; i++)
                    {
                        List<DepositCurrency> freeForwardDepositCurrencies = new List<DepositCurrency>(); //свободные средства в открытых позициях
                        List<DepositCurrency> takenForwardDepositCurrencies = new List<DepositCurrency>(); //занятые средства(на которые куплены лоты) в открытых позициях
                        foreach (Currency currency in _modelData.Currencies)
                        {
                            freeForwardDepositCurrencies.Add(new DepositCurrency { Currency = currency, Deposit = 0, DateTime = optimizationIntervals[t][0] });
                            takenForwardDepositCurrencies.Add(new DepositCurrency { Currency = currency, Deposit = 0, DateTime = optimizationIntervals[t][0] });
                        }

                        List<DepositCurrency> firstDepositCurrenciesChanges = new List<DepositCurrency>(); //начальное состояние депозита
                        foreach (DepositCurrency depositCurrency in freeForwardDepositCurrencies)
                        {
                            firstDepositCurrenciesChanges.Add(new DepositCurrency { Currency = depositCurrency.Currency, Deposit = 0, DateTime = optimizationIntervals[t][0] });
                        }
                        List<List<DepositCurrency>> depositCurrenciesChanges = new List<List<DepositCurrency>>();
                        depositCurrenciesChanges.Add(firstDepositCurrenciesChanges);

                        Account account = new Account { Orders = new List<Order>(), AllOrders = new List<Order>(), CurrentPosition = new List<Deal>(), AllDeals = new List<Deal>(), AccountVariables = AccountVariables.GetAccountVariables(), DefaultCurrency = testing.DefaultCurrency, FreeForwardDepositCurrencies = freeForwardDepositCurrencies, TakenForwardDepositCurrencies = takenForwardDepositCurrencies, DepositCurrenciesChanges = depositCurrenciesChanges, Totalcomission = 0 };
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
                        TestRun testRun = new TestRun { Number = testRunNumber, TestBatch = testBatch, IsOptimizationTestRun = true, Account = account, StartPeriod = optimizationIntervals[t][0], EndPeriod = optimizationIntervals[t][1], AlgorithmParameterValues = algorithmParameterValues, EvaluationCriteriaValues = new List<EvaluationCriteriaValue>(), DealsDeviation = new List<string>(), LoseDeviation = new List<string>(), ProfitDeviation = new List<string>(), LoseSeriesDeviation = new List<string>(), ProfitSeriesDeviation = new List<string>(), IsComplete = false };
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
                            freeForwardDepositCurrencies.Add(new DepositCurrency { Currency = currency, Deposit = 0, DateTime = forwardIntervals[t][0] });
                            takenForwardDepositCurrencies.Add(new DepositCurrency { Currency = currency, Deposit = 0, DateTime = forwardIntervals[t][0] });
                        }

                        List<DepositCurrency> firstDepositCurrenciesChanges = new List<DepositCurrency>(); //начальное состояние депозита
                        foreach (DepositCurrency depositCurrency in freeForwardDepositCurrencies)
                        {
                            firstDepositCurrenciesChanges.Add(new DepositCurrency { Currency = depositCurrency.Currency, Deposit = 0, DateTime = forwardIntervals[t][0] });
                        }
                        List<List<DepositCurrency>> depositCurrenciesChanges = new List<List<DepositCurrency>>();
                        depositCurrenciesChanges.Add(firstDepositCurrenciesChanges);

                        Account account = new Account { Orders = new List<Order>(), AllOrders = new List<Order>(), CurrentPosition = new List<Deal>(), AllDeals = new List<Deal>(), AccountVariables = AccountVariables.GetAccountVariables(), DefaultCurrency = testing.DefaultCurrency, IsForwardDepositTrading = false, FreeForwardDepositCurrencies = freeForwardDepositCurrencies, TakenForwardDepositCurrencies = takenForwardDepositCurrencies, DepositCurrenciesChanges = depositCurrenciesChanges, Totalcomission = 0 };
                        TestRun testRun = new TestRun { Number = testRunNumber, TestBatch = testBatch, IsOptimizationTestRun = false, Account = account, StartPeriod = forwardIntervals[t][0], EndPeriod = forwardIntervals[t][1], EvaluationCriteriaValues = new List<EvaluationCriteriaValue>(), DealsDeviation = new List<string>(), LoseDeviation = new List<string>(), ProfitDeviation = new List<string>(), LoseSeriesDeviation = new List<string>(), ProfitSeriesDeviation = new List<string>(), IsComplete = false };
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
                            freeForwardDepositCurrencies.Add(new DepositCurrency { Currency = depositCurrency.Currency, Deposit = depositCurrency.Deposit, DateTime = forwardIntervals[t][0] });
                            takenForwardDepositCurrencies.Add(new DepositCurrency { Currency = depositCurrency.Currency, Deposit = 0, DateTime = forwardIntervals[t][0] });
                        }

                        List<DepositCurrency> firstDepositCurrenciesChanges = new List<DepositCurrency>(); //начальное состояние депозита
                        foreach (DepositCurrency depositCurrency in freeForwardDepositCurrencies)
                        {
                            firstDepositCurrenciesChanges.Add(new DepositCurrency { Currency = depositCurrency.Currency, Deposit = depositCurrency.Deposit, DateTime = forwardIntervals[t][0] });
                        }
                        List<List<DepositCurrency>> depositCurrenciesChanges = new List<List<DepositCurrency>>();
                        depositCurrenciesChanges.Add(firstDepositCurrenciesChanges);

                        Account account = new Account { Orders = new List<Order>(), AllOrders = new List<Order>(), CurrentPosition = new List<Deal>(), AllDeals = new List<Deal>(), AccountVariables = AccountVariables.GetAccountVariables(), DefaultCurrency = testing.DefaultCurrency, IsForwardDepositTrading = true, FreeForwardDepositCurrencies = freeForwardDepositCurrencies, TakenForwardDepositCurrencies = takenForwardDepositCurrencies, DepositCurrenciesChanges = depositCurrenciesChanges, Totalcomission = 0 };
                        TestRun testRun = new TestRun { Number = testRunNumber, TestBatch = testBatch, IsOptimizationTestRun = false, Account = account, StartPeriod = forwardIntervals[t][0], EndPeriod = forwardIntervals[t][1], EvaluationCriteriaValues = new List<EvaluationCriteriaValue>(), DealsDeviation = new List<string>(), LoseDeviation = new List<string>(), ProfitDeviation = new List<string>(), LoseSeriesDeviation = new List<string>(), ProfitSeriesDeviation = new List<string>(), IsComplete = false };
                        testRunNumber++;
                        //добавляем форвардный тест с торговлей депозитом в testBatch
                        testBatch.ForwardTestRunDepositTrading = testRun;
                    }
                    testing.TestBatches.Add(testBatch);
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
                        testing.DataSourcesCandles.Add(new DataSourceCandles { DataSource = dataSources[i], Candles = new Candle[dataSources[i].DataSourceFiles.Count][], GapIndexes = new List<int>[dataSources[i].DataSourceFiles.Count], AlgorithmIndicatorsValues = new AlgorithmIndicatorValues[testing.Algorithm.AlgorithmIndicators.Count] });
                        bool isCandleIntervalMoreThanGapInterval = dataSources[i].Interval.Duration.TotalHours >= _modelData.Settings.Where(a => a.Id == 7).First().DoubleValue; //интервал свечки больше или равен временному промежутку для гэпа, или нет
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
                            List<int> gaps = new List<int>();
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
                                if(r > 0) //если это не первая свечка, проверяем, является ли она гэпом
                                {
                                    if (isCandleIntervalMoreThanGapInterval) //если временной интервал свечки больше или равен временному интервалу гэпа, определяем гэп по 1.5 превышению временного интервала свечки
                                    {
                                        if((candles[r].DateTime - candles[r - 1].DateTime).TotalHours >= dataSources[i].Interval.Duration.TotalHours * 1.5) //если разница между датами текущей и прошлой свечек в 1.5 раз больше временного интервала свечки, значит эта свечка считается гэпом
                                        {
                                            gaps.Add(r); //запоминаем индекс свечки с гэпом
                                        }
                                    }
                                    else //временной интервал свечки меньше временного интервала гэпа
                                    {
                                        if ((candles[r].DateTime - candles[r - 1].DateTime).TotalHours >= _modelData.Settings.Where(a => a.Id == 7).First().DoubleValue) //если разница между датами текущей и прошлой свечек больше или равна временному интервалу гэпа, значит эта свечка считается гэпом
                                        {
                                            gaps.Add(r); //запоминаем индекс свечки с гэпом
                                        }
                                    }
                                }
                                line = streamReader.ReadLine();
                                r++;
                            }
                            streamReader.Close();
                            fileStream.Close();

                            testing.DataSourcesCandles[i].Candles[k] = candles;
                            testing.DataSourcesCandles[i].GapIndexes[k] = gaps;

                            readFilesCount++;
                            DispatcherInvoke((Action)(() => {
                                _mainCommunicationChannel.TestingProgress.Clear();
                                _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 1/4:  Считывание файлов источников данных", StepTasksCount = filesCount, CompletedStepTasksCount = readFilesCount, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatchReadDataSources.Elapsed, CancelPossibility = true, IsFinishSimulation = false, IsSuccessSimulation = false, IsFinish = false });
                            }));
                            if (cancellationToken.IsCancellationRequested) //если был запрос на отмену операции, прекращем функцию
                            {
                                TestingEnding(false, testing);
                                return;
                            }
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
                                algorithmIndicatorCatalog.AlgorithmIndicatorCatalogElements.Add(new AlgorithmIndicatorCatalogElement { AlgorithmParameterValues = new List<AlgorithmParameterValue>(), FileName = "withoutParameters.dat", IsComplete = false });
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
                                    AlgorithmIndicatorCatalogElement algorithmIndicatorCatalogElement = new AlgorithmIndicatorCatalogElement { AlgorithmParameterValues = new List<AlgorithmParameterValue>(), FileName = fileName, IsComplete = false };
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
                    int algorithmIndicatorsValuesCount = 0; //количество значений всех индикаторов
                    int calculatedAlgorithmIndicatorsCount = 0; //количество вычисленных индикаторов
                    for (int i = 0; i < testing.DataSourcesCandles.Count; i++)
                    {
                        foreach (AlgorithmIndicatorCatalog algorithmIndicatorCatalog in testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs)
                        {
                            algorithmIndicatorsValuesCount += algorithmIndicatorCatalog.AlgorithmIndicatorCatalogElements.Count;
                        }
                    }
                    DispatcherInvoke((Action)(() => {
                        _mainCommunicationChannel.TestingProgress.Clear();
                        _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 2/4:  Вычисление индикаторов", StepTasksCount = algorithmIndicatorsValuesCount, CompletedStepTasksCount = calculatedAlgorithmIndicatorsCount, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatchCalculateIndicators.Elapsed, CancelPossibility = true, IsFinishSimulation = false, IsSuccessSimulation = false, IsFinish = false });
                    }));

                    if(algorithmIndicatorsValuesCount > 0) //имеются ли индикаторы для рассчета
                    {
                        //определяем количество используемых потоков
                        int processorCountAlgorithmIndicator = Environment.ProcessorCount;
                        processorCountAlgorithmIndicator -= _modelData.Settings.Where(i => i.Id == 1).First().BoolValue ? 1 : 0; //если в настройках выбрано оставлять один поток, вычитаем из количества потоков
                        if (algorithmIndicatorsValuesCount < processorCountAlgorithmIndicator) //если тестов меньше чем число доступных потоков, устанавливаем количество потоков на количество тестов, т.к. WaitAll ругается если задача в tasks null
                        {
                            processorCountAlgorithmIndicator = algorithmIndicatorsValuesCount;
                        }
                        //processorCountAlgorithmIndicator = 1; //эту строку я использую если нужно проследить рассчет индикатора, чтобы не переключаться между другими потоками

                        Task[] tasksAlgorithmIndicatorCatalogElement = new Task[processorCountAlgorithmIndicator]; //задачи
                        int[][] tasksExecutingAlgorithmIndicatorCatalogElement = new int[processorCountAlgorithmIndicator][]; //массив с выполняющимися индикаторами алгоритма, в 0-м элементе индекс DataSourcesCandles, в 1-м индекс AlgorithmIndicatorCatalogs, во 2-м индекс AlgorithmIndicatorCatalogElements
                        int[][][] algorithmIndicatorsStatus = new int[testing.DataSourcesCandles.Count][][]; //статусы выполненности algorithmIndicatorCatalogElement-ов, первый массив - массив с DataSourcesCandles, второй - массив с AlgorithmIndicatorCatalogs, третий - массив с AlgorithmIndicatorCatalogElements. У невыполненного значение 0, у запущенного 1, а у выполненного 2
                        int[][][] algorithmIndicatorsStatus2 = new int[testing.DataSourcesCandles.Count][][]; //индексы потока в tasksAlgorithmIndicatorCatalogElement в котором выполнялся algorithmIndicatorCatalogElement
                                                                                                              //заполняем статусы
                        for (int i = 0; i < testing.DataSourcesCandles.Count; i++)
                        {
                            algorithmIndicatorsStatus[i] = new int[testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs.Length][];
                            algorithmIndicatorsStatus2[i] = new int[testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs.Length][];
                            for (int k = 0; k < testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs.Length; k++)
                            {
                                algorithmIndicatorsStatus[i][k] = Enumerable.Repeat(0, testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs[k].AlgorithmIndicatorCatalogElements.Count).ToArray(); //заполняем нулями
                                algorithmIndicatorsStatus2[i][k] = Enumerable.Repeat(-1, testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs[k].AlgorithmIndicatorCatalogElements.Count).ToArray(); //заполняем индексы задач в tasksAlgorithmIndicatorCatalogElement -1
                            }
                        }
                        bool isAllAlgorithmIndicatorsComplete = false; //выполнены ли все AlgorithmIndicator-ы
                        int num = 0; //номер задачи, нужен для начального заполнения массива tasks
                        while (isAllAlgorithmIndicatorsComplete == false)
                        {
                            if (tasksAlgorithmIndicatorCatalogElement[tasksAlgorithmIndicatorCatalogElement.Length - 1] == null) //если пока еще не заполнен массив с задачами, заполняем его
                            {
                                //находим первый AlgorithmIndicator, который еще не запущен (имеет статус 0)
                                int dsCandlesIndex = 0;
                                int algorithmIndicatorCatalogsIndex = 0;
                                int algorithmIndicatorCatalogElementIndex = 0;
                                bool isFindAlgorithmIndicatorCatalogElement = false;
                                while (isFindAlgorithmIndicatorCatalogElement == false)
                                {
                                    if (algorithmIndicatorsStatus[dsCandlesIndex][algorithmIndicatorCatalogsIndex][algorithmIndicatorCatalogElementIndex] == 0)
                                    {
                                        isFindAlgorithmIndicatorCatalogElement = true;
                                    }
                                    else
                                    {
                                        algorithmIndicatorCatalogElementIndex++;
                                        if (algorithmIndicatorCatalogElementIndex >= algorithmIndicatorsStatus[dsCandlesIndex][algorithmIndicatorCatalogsIndex].Length) //если вышли за границы индекса algorithmIndicatorCatalogElement, переходим на следующий algorithmIndicatorCatalog
                                        {
                                            algorithmIndicatorCatalogElementIndex = 0;
                                            algorithmIndicatorCatalogsIndex++;
                                            if (algorithmIndicatorCatalogsIndex >= algorithmIndicatorsStatus[dsCandlesIndex].Length) //если вышли за границы индекса algorithmIndicatorCatalogs, переходим на следующий dataSourcesCandles
                                            {
                                                algorithmIndicatorCatalogsIndex = 0;
                                                dsCandlesIndex++;
                                            }
                                        }
                                    }
                                }

                                DataSourceCandles dataSourceCandles = testing.DataSourcesCandles[dsCandlesIndex];
                                AlgorithmIndicator algorithmIndicator = testing.DataSourcesCandles[dsCandlesIndex].AlgorithmIndicatorCatalogs[algorithmIndicatorCatalogsIndex].AlgorithmIndicator;
                                AlgorithmIndicatorCatalogElement algorithmIndicatorCatalogElement = testing.DataSourcesCandles[dsCandlesIndex].AlgorithmIndicatorCatalogs[algorithmIndicatorCatalogsIndex].AlgorithmIndicatorCatalogElements[algorithmIndicatorCatalogElementIndex];

                                Task task = Task.Run(() => AlgorithmIndicatorCatalogElementCalculate(testing, dataSourceCandles, algorithmIndicator, algorithmIndicatorCatalogElement));
                                tasksAlgorithmIndicatorCatalogElement[num] = task;
                                tasksExecutingAlgorithmIndicatorCatalogElement[num] = new int[3] { dsCandlesIndex, algorithmIndicatorCatalogsIndex, algorithmIndicatorCatalogElementIndex }; //запоминаем индексы algorithmIndicatorCatalogElement, который выполняется в текущей задачи (в элементе массива tasksExecutingAlgorithmIndicatorCatalogElement с индексом num)
                                algorithmIndicatorsStatus[dsCandlesIndex][algorithmIndicatorCatalogsIndex][algorithmIndicatorCatalogElementIndex] = 1; //отмечаем что testRun имеет статус запущен
                                algorithmIndicatorsStatus2[dsCandlesIndex][algorithmIndicatorCatalogsIndex][algorithmIndicatorCatalogElementIndex] = num;
                                num++; //увеличиваем индекс задачи
                            }
                            else //иначе ждем и обрабатываем выполненные задачи
                            {
                                bool isAnyComplete = false;
                                //ждем пока один из выполняющихся algorithmIndicatorCatalogElement-ов не будет выполнен
                                while (isAnyComplete == false)
                                {
                                    Thread.Sleep(20);
                                    int taskIndex = 0;
                                    while (taskIndex < tasksExecutingAlgorithmIndicatorCatalogElement.Length && isAnyComplete == false) //проходим по всем задачам и смотрим на статусы выполненности algorithmIndicatorCatalogElement-ов
                                    {
                                        //если в задаче находится algorithmIndicatorCatalogElement со статусом Запущен (если нет, то он уже выполнен, и не был заменен новым testRun-ом т.к. они закончились)
                                        if (algorithmIndicatorsStatus[tasksExecutingAlgorithmIndicatorCatalogElement[taskIndex][0]][tasksExecutingAlgorithmIndicatorCatalogElement[taskIndex][1]][tasksExecutingAlgorithmIndicatorCatalogElement[taskIndex][2]] == 1)
                                        {
                                            if (testing.DataSourcesCandles[tasksExecutingAlgorithmIndicatorCatalogElement[taskIndex][0]].AlgorithmIndicatorCatalogs[tasksExecutingAlgorithmIndicatorCatalogElement[taskIndex][1]].AlgorithmIndicatorCatalogElements[tasksExecutingAlgorithmIndicatorCatalogElement[taskIndex][2]].IsComplete)
                                            {
                                                isAnyComplete = true;
                                            }
                                        }
                                        taskIndex++;
                                    }
                                }

                                if (cancellationToken.IsCancellationRequested) //если был запрос на отмену операции, прекращем функцию
                                {
                                    Task.WaitAll(tasksAlgorithmIndicatorCatalogElement);
                                    TestingEnding(false, testing);
                                    return;
                                }
                                DispatcherInvoke((Action)(() => {
                                    _mainCommunicationChannel.TestingProgress.Clear();
                                    _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 2/4:  Вычисление индикаторов", StepTasksCount = algorithmIndicatorsValuesCount, CompletedStepTasksCount = calculatedAlgorithmIndicatorsCount, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatchCalculateIndicators.Elapsed, CancelPossibility = true, IsFinishSimulation = false, IsSuccessSimulation = false, IsFinish = false });
                                }));

                                //обрабатываем выполненные algorithmIndicatorCatalogElement-ы
                                int taskIndex1 = 0;
                                while (taskIndex1 < tasksExecutingAlgorithmIndicatorCatalogElement.Length) //проходим по всем задачам и смотрим на статусы выполненности algorithmIndicatorCatalogElement-ов, у выполненных, которые имееют статус Запущен, отмечаем в статусе как выполнен
                                {
                                    if (algorithmIndicatorsStatus[tasksExecutingAlgorithmIndicatorCatalogElement[taskIndex1][0]][tasksExecutingAlgorithmIndicatorCatalogElement[taskIndex1][1]][tasksExecutingAlgorithmIndicatorCatalogElement[taskIndex1][2]] == 1) //если algorithmIndicatorCatalogElement в текущей задаче имеет статус Запущен
                                    {
                                        if (testing.DataSourcesCandles[tasksExecutingAlgorithmIndicatorCatalogElement[taskIndex1][0]].AlgorithmIndicatorCatalogs[tasksExecutingAlgorithmIndicatorCatalogElement[taskIndex1][1]].AlgorithmIndicatorCatalogElements[tasksExecutingAlgorithmIndicatorCatalogElement[taskIndex1][2]].IsComplete)
                                        {
                                            algorithmIndicatorsStatus[tasksExecutingAlgorithmIndicatorCatalogElement[taskIndex1][0]][tasksExecutingAlgorithmIndicatorCatalogElement[taskIndex1][1]][tasksExecutingAlgorithmIndicatorCatalogElement[taskIndex1][2]] = 2; //отмечаем в статусе algorithmIndicatorCatalogElement-а что он выполнен
                                            calculatedAlgorithmIndicatorsCount++; //увеличиваем количество выполненных индикаторов на 1
                                        }
                                    }
                                    taskIndex1++;
                                }

                                //проходим по всем задачам, и для каждой завершенной, ищем невыполненный и незапущенный тест, и если нашли, то запускаем его в задаче
                                int indexTask = 0;
                                while (indexTask < tasksExecutingAlgorithmIndicatorCatalogElement.Length)
                                {
                                    //определяем, имеет ли algorithmIndicatorCatalogElement в данной задаче статус Выполнен
                                    if (algorithmIndicatorsStatus[tasksExecutingAlgorithmIndicatorCatalogElement[indexTask][0]][tasksExecutingAlgorithmIndicatorCatalogElement[indexTask][1]][tasksExecutingAlgorithmIndicatorCatalogElement[indexTask][2]] == 2)
                                    {
                                        //ищем невыполненный и незапущенный тест среди всех тестов
                                        AlgorithmIndicatorCatalogElement algorithmIndicatorCatalogElement = new AlgorithmIndicatorCatalogElement();
                                        int dsCandlesIndex = 0;
                                        int algorithmIndicatorCatalogsIndex = 0;
                                        int algorithmIndicatorCatalogElementIndex = 0;
                                        bool isFindAlgorithmIndicatorCatalogElement = false;
                                        //ищем algorithmIndicatorCatalogElement со статусом не запущен
                                        while (isFindAlgorithmIndicatorCatalogElement == false && dsCandlesIndex < testing.DataSourcesCandles.Count)
                                        {
                                            if (algorithmIndicatorsStatus[dsCandlesIndex][algorithmIndicatorCatalogsIndex][algorithmIndicatorCatalogElementIndex] == 0)
                                            {
                                                isFindAlgorithmIndicatorCatalogElement = true;
                                                algorithmIndicatorCatalogElement = testing.DataSourcesCandles[dsCandlesIndex].AlgorithmIndicatorCatalogs[algorithmIndicatorCatalogsIndex].AlgorithmIndicatorCatalogElements[algorithmIndicatorCatalogElementIndex];
                                            }
                                            else
                                            {
                                                algorithmIndicatorCatalogElementIndex++;
                                                if (algorithmIndicatorCatalogElementIndex >= algorithmIndicatorsStatus[dsCandlesIndex][algorithmIndicatorCatalogsIndex].Length) //если вышли за границы индекса algorithmIndicatorCatalogElement, переходим на следующий algorithmIndicatorCatalog
                                                {
                                                    algorithmIndicatorCatalogElementIndex = 0;
                                                    algorithmIndicatorCatalogsIndex++;
                                                    if (algorithmIndicatorCatalogsIndex >= algorithmIndicatorsStatus[dsCandlesIndex].Length) //если вышли за границы индекса algorithmIndicatorCatalogs, переходим на следующий dataSourcesCandles
                                                    {
                                                        algorithmIndicatorCatalogsIndex = 0;
                                                        dsCandlesIndex++;
                                                    }
                                                }
                                            }
                                        }
                                        //если нашли не запущенный algorithmIndicatorCatalogElement, запускаем его в текущей задаче
                                        if (isFindAlgorithmIndicatorCatalogElement)
                                        {
                                            DataSourceCandles dataSourceCandles = testing.DataSourcesCandles[dsCandlesIndex];
                                            AlgorithmIndicator algorithmIndicator = testing.DataSourcesCandles[dsCandlesIndex].AlgorithmIndicatorCatalogs[algorithmIndicatorCatalogsIndex].AlgorithmIndicator;

                                            Task task = Task.Run(() => AlgorithmIndicatorCatalogElementCalculate(testing, dataSourceCandles, algorithmIndicator, algorithmIndicatorCatalogElement));
                                            tasksAlgorithmIndicatorCatalogElement[indexTask] = task;
                                            tasksExecutingAlgorithmIndicatorCatalogElement[indexTask] = new int[3] { dsCandlesIndex, algorithmIndicatorCatalogsIndex, algorithmIndicatorCatalogElementIndex }; //запоминаем индексы algorithmIndicatorCatalogElement, который выполняется в текущей задачи (в элементе массива tasksExecutingAlgorithmIndicatorCatalogElement с индексом num)
                                            algorithmIndicatorsStatus[dsCandlesIndex][algorithmIndicatorCatalogsIndex][algorithmIndicatorCatalogElementIndex] = 1; //отмечаем что testRun имеет статус запущен
                                            algorithmIndicatorsStatus2[dsCandlesIndex][algorithmIndicatorCatalogsIndex][algorithmIndicatorCatalogElementIndex] = indexTask; //запоминаем индекс task в котором выполняется данный algorithmIndicatorCatalogElement
                                        }
                                    }
                                    indexTask++;
                                }
                                //смотрим на статусы algorithmIndicatorCatalogElement-ов в задачах, и если нет ни одного со статусом Запущен, значит все algorithmIndicatorCatalogElement-ы выполнены, тестирование окончено
                                bool isAnyLaunched = false;
                                for (int i = 0; i < tasksExecutingAlgorithmIndicatorCatalogElement.Length; i++)
                                {
                                    if (algorithmIndicatorsStatus[tasksExecutingAlgorithmIndicatorCatalogElement[i][0]][tasksExecutingAlgorithmIndicatorCatalogElement[i][1]][tasksExecutingAlgorithmIndicatorCatalogElement[i][2]] == 1)
                                    {
                                        isAnyLaunched = true;
                                    }
                                }
                                if (isAnyLaunched == false)
                                {
                                    isAllAlgorithmIndicatorsComplete = true; //если ни один из algorithmIndicatorCatalogElement-ов в задачах не имеет статус Запущен, отмечаем что тестирование окончено
                                }
                            }
                        }
                        stopwatchCalculateIndicators.Stop();
                        DispatcherInvoke((Action)(() => {
                            _mainCommunicationChannel.TestingProgress.Clear();
                            _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 2/4:  Вычисление индикаторов", StepTasksCount = algorithmIndicatorsValuesCount, CompletedStepTasksCount = calculatedAlgorithmIndicatorsCount, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatchCalculateIndicators.Elapsed, CancelPossibility = true, IsFinishSimulation = false, IsSuccessSimulation = false, IsFinish = false });
                        }));
                    }

                    //вычисляем идеальную прибыль для всех тестовых связок
                    foreach(TestBatch testBatch in testing.TestBatches)
                    {
                        //вычисляем идеальную прибыль для каждого DataSourceCandles
                        DateTime startDateTime = testBatch.OptimizationTestRuns[0].StartPeriod; //дата начала подсчета идеальной прибыли
                        DateTime endDateTime = testBatch.OptimizationTestRuns[0].EndPeriod; //дата окончания подсчета идеальной прибыли
                        int iteration = 1;
                        do
                        {
                            if (iteration == 2) //если это не первая итерация, значит нужно вычислить идеальную прибыль для форвардного периода, устанавливаем форвардные даты начала и окончания
                            {
                                startDateTime = testBatch.ForwardTestRun.StartPeriod;
                                endDateTime = testBatch.ForwardTestRun.EndPeriod;
                            }
                            List<PerfectProfit> perfectProfits = iteration == 1 ? testBatch.OptimizationPerfectProfits : testBatch.ForwardPerfectProfits; //для первой итерации, устанавливаем список с идеальными прибылями для оптимизационного периода, для второй итерации, для форвардного периода
                            foreach (DataSourceCandles dataSourceCandles in testing.DataSourcesCandles)
                            {
                                if(testBatch.DataSourceGroup.DataSourceAccordances.Where(a=>a.DataSource.Id== dataSourceCandles.DataSource.Id).Any()) //вычисляем идеальную прибыль только для источников данных группы тестовой связки
                                {
                                    //проходим по всем свечкам источников данных, пока не достигнем времени окончания теста, не выйдем за границы имеющихся файлов, или не получим запрос на отмену тестирования
                                    PerfectProfit perfectProfit = new PerfectProfit { IdDataSource = dataSourceCandles.DataSource.Id };
                                    DateTime currentDateTime = startDateTime;
                                    double pricesAmount = 0; //сумма разности цен закрытия, взятой по модулю
                                    int fileIndex = 0;
                                    int candleIndex = 0;
                                    bool isOverFileIndex = false; //вышел ли какой-либо из индексов файлов за границы массива файлов источника данных
                                    while (DateTime.Compare(currentDateTime, endDateTime) < 0 && isOverFileIndex == false && cancellationToken.IsCancellationRequested == false)
                                    {
                                        if (candleIndex > 0) //чтобы не обращаться к прошлой свечке при смене файла
                                        {
                                            currentDateTime = dataSourceCandles.Candles[fileIndex][candleIndex].DateTime;
                                            pricesAmount += Math.Abs(dataSourceCandles.Candles[fileIndex][candleIndex].C - dataSourceCandles.Candles[fileIndex][candleIndex - 1].C); //прибавляем разность цен закрытия, взятую по модулю
                                        }

                                        //переходим на следующую свечку, пока не дойдем до даты которая позже текущей
                                        bool isOverDate = DateTime.Compare(dataSourceCandles.Candles[fileIndex][candleIndex].DateTime, currentDateTime) > 0; //дошли ли до даты которая позже текущей

                                        //переходим на следующую свечку, пока не дойдем до даты которая позже текущей или пока не выйдем за пределы файлов
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

                                        //обновляем текущую дату
                                        if (isOverFileIndex == false)
                                        {
                                            currentDateTime = dataSourceCandles.Candles[fileIndex][candleIndex].DateTime;
                                        }
                                    }
                                    perfectProfit.Value = pricesAmount / dataSourceCandles.DataSource.PriceStep * dataSourceCandles.DataSource.CostPriceStep; //записываем идеальную прибыль
                                    perfectProfits.Add(perfectProfit);
                                }
                            }
                            iteration++;
                        }
                        while (testing.IsForwardTesting && iteration < 3);
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
                                                FindSurfaceAxes(testBatch, testing); //находим оси для тремхмерной поверхности
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
            int[] gapIndexes = new int[testRun.TestBatch.DataSourceGroup.DataSourceAccordances.Count]; //индексы (для всех источников данных группы) элемента списка , содержащего индекс свечки с гэпом
            bool[] gaps = new bool[gapIndexes.Length]; //является ли текущая свечка гэпом, для каждого источника данных группы

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
                gapIndexes[i] = 0;
                gaps[i] = false;
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

            //копируем объект скомпилированного алгоритма, чтобы из разных потоков не обращаться к одному объекту и к одним свойствам объекта
            dynamic CompiledAlgorithmCopy = testing.CompiledAlgorithm.Clone();

            //проходим по всем свечкам источников данных, пока не достигнем времени окончания теста, не выйдем за границы имеющихся файлов, или не получим запрос на отмену тестирования
            while (DateTime.Compare(currentDateTime, testRun.EndPeriod) < 0 && isOverFileIndex == false && cancellationToken.IsCancellationRequested == false)
            {
                //определяем гэпы для текущих свечек
                for (int i = 0; i < dataSourceCandles.Length; i++)
                {
                    gaps[i] = false;
                    if (gapIndexes[i] < dataSourceCandles[i].GapIndexes[fileIndexes[i]].Count) //проверяем, не вышли ли за границы списка с индексами свечек с гэпом
                    {
                        if (candleIndexes[i] == dataSourceCandles[i].GapIndexes[fileIndexes[i]][gapIndexes[i]]) //равняется ли индекс свечки, индексу свечки с гэпом
                        {
                            gaps[i] = true;
                            gapIndexes[i]++; //переходим на следующий индекс свечки с гэпом
                        }
                    }
                }
                //обрабатываем текущие заявки (только тех источников данных, текущие свечки которых равняются текущей дате)
                //формируем список источников данных для которых будут проверяться заявки на исполнение (те, даты которых равняются текущей дате)
                List <DataSource> approvedDataSources = new List<DataSource>();
                for (int i = 0; i < dataSourceCandles.Length; i++)
                {
                    if (DateTime.Compare(dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, currentDateTime) == 0)
                    {
                        approvedDataSources.Add(dataSourceCandles[i].DataSource);
                    }
                }
                //проверяем заявки на исполнение
                bool isWereDeals = CheckOrdersExecution(dataSourceCandles, testRun.Account, approvedDataSources, fileIndexes, candleIndexes, gaps, false, true, true); //были ли совершены сделки при проверке исполнения заявок

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
                        dataSourcesForCalculate[i].MinLotCount = dataSourceCandles[i].DataSource.MinLotCount;
                        dataSourcesForCalculate[i].PriceStep = dataSourceCandles[i].DataSource.PriceStep;
                        dataSourcesForCalculate[i].CostPriceStep = dataSourceCandles[i].DataSource.CostPriceStep;
                        dataSourcesForCalculate[i].MinLotsCost = dataSourceCandles[i].DataSource.MarginType.Id == 2 ? dataSourceCandles[i].DataSource.MarginCost * dataSourceCandles[i].DataSource.MinLotMarginPartCost : dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].C * dataSourceCandles[i].DataSource.MinLotMarginPartCost;
                        dataSourcesForCalculate[i].Price = averagePricePosition;
                        dataSourcesForCalculate[i].CountBuy = isBuyDirection ? volumePosition : 0;
                        dataSourcesForCalculate[i].CountSell = isBuyDirection ? 0 : volumePosition;
                        dataSourcesForCalculate[i].TimeInCandle = dataSourceCandles[i].DataSource.Interval.Duration;
                        dataSourcesForCalculate[i].TradingStartTimeOfDay = dataSourceFileWorkingPeriod.TradingStartTime;
                        dataSourcesForCalculate[i].TradingEndTimeOfDay = dataSourceFileWorkingPeriod.TradingEndTime;
                        dataSourcesForCalculate[i].Candles = dataSourceCandles[i].Candles[fileIndexes[i]];
                        dataSourcesForCalculate[i].CurrentCandleIndex = candleIndexes[i];
                    }

                    AccountForCalculate accountForCalculate = new AccountForCalculate { FreeRubleMoney = testRun.Account.FreeForwardDepositCurrencies.Where(j => j.Currency.Id == 1).First().Deposit, FreeDollarMoney = testRun.Account.FreeForwardDepositCurrencies.Where(j => j.Currency.Id == 2).First().Deposit, TakenRubleMoney = testRun.Account.TakenForwardDepositCurrencies.Where(j => j.Currency.Id == 1).First().Deposit, TakenDollarMoney = testRun.Account.TakenForwardDepositCurrencies.Where(j => j.Currency.Id == 2).First().Deposit, IsForwardDepositTrading = testRun.Account.IsForwardDepositTrading, AccountVariables = testRun.Account.AccountVariables };
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
                        //если это не форвардное тестирование с торговлей депозитом, устанавливаем размер заявок в минимальное количество лотов, а так же устанавливаем DateTimeSubmit для заявок
                        foreach (Order order in algorithmCalculateResult.Orders)
                        {
                            if (testRun.Account.IsForwardDepositTrading == false)
                            {
                                order.Count = order.DataSource.MinLotCount;
                                order.StartCount = order.DataSource.MinLotCount;
                            }
                            order.DateTimeSubmit = currentDateTime;
                        }
                        //приводим заявки к виду который прислал пользователь в алгоритме
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

                        //проверяем исполнение рыночных заявок, выставленных на текущей свечке
                        if (iteration == 1)
                        {
                            CheckOrdersExecution(dataSourceCandles, testRun.Account, approvedDataSources, fileIndexes, candleIndexes, gaps, true, false, false);
                        }

                        //если на текущей свечке были совершены сделки, проверяем стоп-заявки на исполнение (чтобы если на текущей свечке была открыта позиция, после выставления стоп-заявки проверить её на исполнение на текущей свечке)
                        if (isWereDeals && iteration == 1)
                        {
                            isWereDealsStopLoss = CheckOrdersExecution(dataSourceCandles, testRun.Account, approvedDataSources, fileIndexes, candleIndexes, gaps, false, true, false); //были ли совершены сделки при проверке исполнения стоп-заявок
                        }
                    }
                }
                while (isWereDealsStopLoss && iteration == 1); //если этой первое исполнение алгоритма, и при проверке стоп-заявок были сделки, еще раз прогоняем алгоритм чтобы обновить заявки

                //находим среди следующих свечек, свечку с саммой ранней датой. Обновляем текущую дату на эту дату. Затем переходим на следующую свечку, если она раньше или равняется текущей дате, и переходим на следующий файл источника данных только если следующий файл имеется, т.к. мы будем обращаться по текущему индексу и свечки и файла к тем источникам данных которые закончились, во время прохода по тем источникам данных которые еще не закончились (например файл с недельными свечками закончился, а часовые еще есть, и мы еще 7 дней можем торговать на часовых свечках, обращаясь к информации последней недельной свечки)
                //ищем среди следующих свечек самую раннюю дату
                int[] nextCandleIndexes = new int[candleIndexes.Length]; //индексы свечек, свечки, которая позже текущей
                int[] nextFileIndexes = new int[fileIndexes.Length]; //индексы файлов, свечки, которая позже текущей
                for(int i = 0; i < nextFileIndexes.Length; i++)
                {
                    nextFileIndexes[i] = -1; //если свечки, которая позже текущей не найдно, значение индекса файла равняется -1
                }
                DateTime nextEarliestDateTime = dataSourceCandles[0].Candles[fileIndexes[0]][candleIndexes[0]].DateTime; //присваиваем любое значение
                bool isEndAllDataSources = true;
                bool isInitNextDateTime = false;
                for (int i = 0; i < dataSourceCandles.Length; i++)
                {
                    //доходим до свечки, которая позже текущей свечки в данном источнике данных
                    //переходим на следующую свечку, пока не дойдем до даты которая позже текущей свечки
                    bool isOverDate = fileIndexes[i] < dataSourceCandles[i].Candles.Length ? DateTime.Compare(dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, currentDateTime) > 0 : false; //дошли ли до даты которая позже текущей
                    int candleIndex = candleIndexes[i];
                    int fileIndex = fileIndexes[i];
                    //переходим на следующую свечку, пока не дойдем до даты которая позже текущей или пока не выйдем за пределы файлов
                    while (isOverDate == false && fileIndex < dataSourceCandles[i].Candles.Length)
                    {
                        candleIndex++;
                        //если массив со свечками файла подошел к концу, переходим на следующий файл
                        if (candleIndex >= dataSourceCandles[i].Candles[fileIndex].Length)
                        {
                            candleIndex = 0;
                            fileIndex++;
                        }
                        //если индекс файла не вышел за пределы массива, проверяем, дошли ли до даты которая позже текущей свечки
                        if (fileIndex < dataSourceCandles[i].Candles.Length)
                        {
                            isOverDate = DateTime.Compare(dataSourceCandles[i].Candles[fileIndex][candleIndex].DateTime, dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime) > 0;
                        }
                    }
                    //если не вышли за пределы файлов, значит нашли дату которая позже текущей свечки
                    if(fileIndex < dataSourceCandles[i].Candles.Length)
                    {
                        nextCandleIndexes[i] = candleIndex; //запоминаем индексы файла и свечки, следующей по дате свечки, у данного источника данных
                        nextFileIndexes[i] = fileIndex;
                        isEndAllDataSources = false; //отмечаем что не все файлы закончились
                        if (!isInitNextDateTime)
                        {
                            isInitNextDateTime = true;
                            nextEarliestDateTime = dataSourceCandles[i].Candles[fileIndex][candleIndex].DateTime;
                        }
                        else
                        {
                            if(DateTime.Compare(nextEarliestDateTime, dataSourceCandles[i].Candles[fileIndex][candleIndex].DateTime) > 0)
                            {
                                nextEarliestDateTime = dataSourceCandles[i].Candles[fileIndex][candleIndex].DateTime;
                            }
                        }
                    }
                }
                //если не все файлы закончились, переходим на следующую свечку
                if (!isEndAllDataSources)
                {
                    currentDateTime = nextEarliestDateTime;
                    for(int i = 0; i < dataSourceCandles.Length; i++)
                    {
                        if(nextFileIndexes[i] > -1) //если найдена свечка, которая позже текущей
                        {
                            //если свечка, которая позже текущей раньше, или равняется обновленной текущей дате, значит переходим на неё (свечка не в будущем)
                            if (DateTime.Compare(dataSourceCandles[i].Candles[nextFileIndexes[i]][nextCandleIndexes[i]].DateTime, currentDateTime) <= 0)
                            {
                                if (nextFileIndexes[i] > fileIndexes[i]) //если перешли на следующий файл, обнуляем индекс гэпа
                                {
                                    gapIndexes[i] = 0;
                                }
                                candleIndexes[i] = nextCandleIndexes[i];
                                fileIndexes[i] = nextFileIndexes[i];
                            }
                        }
                    }
                }
                else
                {
                    isOverFileIndex = true;
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
                //копируем объект скомпилированного критерия оценки, чтобы из разных потоков не обращаться к одному объекту и к одним свойствам объекта
                dynamic CompiledEvaluationCriteriaCopy = testing.CompiledEvaluationCriterias[i].Clone();
                EvaluationCriteriaValue evaluationCriteriaValue = CompiledEvaluationCriteriaCopy.Calculate(dataSourceCandles, testRun, _modelData.Settings);
                evaluationCriteriaValue.EvaluationCriteria = _modelData.EvaluationCriterias[i];
                testRun.EvaluationCriteriaValues.Add(evaluationCriteriaValue);
            }

            //ModelFunctions.TestEvaluationCriteria(testRun); //так я отлаживаю критерии оценки

            testRun.IsComplete = true;
        }

        public bool CheckOrdersExecution(DataSourceCandles[] dataSourcesCandles, Account account, List<DataSource> approvedDataSources, int[] fileIndexes, int[] candleIndexes, bool[] gaps, bool isMarket, bool isStop, bool isLimit) //функция проверяет заявки на их исполнение в текущей свечке, возвращает false если не было сделок, и true если были совершены сделки. approvedDataSources - список с источниками данных, заявки которых будут проверяться на исполнение. isMarket, isStop, isLimit - если true, будут проверяться на исполнение эти заявки
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
                        int slippage = gaps[dataSourcesCandlesIndex] ? 0 : dataSourcesCandles[dataSourcesCandlesIndex].DataSource.PointsSlippage; //если текущая свечка - гэп, убираем базовое проскальзывание, и оставляем только вычисляемое, чтобы цена исполнения заявки была по худщей цене в свечке, и если объем большой то и проскальзывание было
                        slippage += Slippage(dataSourcesCandles[dataSourcesCandlesIndex], fileIndexes[dataSourcesCandlesIndex], candleIndexes[dataSourcesCandlesIndex], order.Count); //добавляем проскальзывание
                        slippage = order.Direction == true ? slippage : -slippage; //для покупки проскальзывание идет вверх, для продажи вниз
                        double dealPrice = dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].C;
                        if (gaps[dataSourcesCandlesIndex]) //если текущая свечка - гэп, устанавливаем худшую цену
                        {
                            dealPrice = order.Direction ? dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H : dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L;
                        }
                        isMakeADeals = MakeADeal(account, order, order.Count, dealPrice + slippage * dataSourcesCandles[dataSourcesCandlesIndex].DataSource.PriceStep, dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime) ? true : isMakeADeals;
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
                            int slippage = gaps[dataSourcesCandlesIndex] ? 0 : dataSourcesCandles[dataSourcesCandlesIndex].DataSource.PointsSlippage; //если текущая свечка - гэп, убираем базовое проскальзывание, и оставляем только вычисляемое, чтобы цена исполнения заявки была по худщей цене в свечке, и если объем большой то и проскальзывание было
                            slippage += Slippage(dataSourcesCandles[dataSourcesCandlesIndex], fileIndexes[dataSourcesCandlesIndex], candleIndexes[dataSourcesCandlesIndex], order.Count); //добавляем проскальзывание
                            slippage = order.Direction == true ? slippage : -slippage; //для покупки проскальзывание идет вверх, для продажи вниз
                            double dealPrice = order.Price;
                            if (gaps[dataSourcesCandlesIndex]) //если текущая свечка - гэп, устанавливаем худшую цену
                            {
                                dealPrice = order.Direction ? dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H : dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L;
                            }
                            isMakeADeals = MakeADeal(account, order, order.Count, dealPrice + slippage * dataSourcesCandles[dataSourcesCandlesIndex].DataSource.PriceStep, dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime) ? true : isMakeADeals;
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
                            if (overLots > 0) //если есть лоты которые могли быть исполнены на текущей свечке, совершаем сделку
                            {
                                decimal dealCount = order.Count <= overLots ? order.Count : overLots;
                                double dealPrice = order.Price;

                                //если цена лимитной заявки находится вне цены свечки (цена покупки выше самой худшей цены свечки, или цена продажи ниже самой худшей цены свечки), устанавливаем цену исполнения как у рыночной заявки
                                if ((order.Direction == true && dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H < order.Price) || (order.Direction == false && dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L > order.Price))
                                {
                                    int slippage = gaps[dataSourcesCandlesIndex] ? 0 : dataSourcesCandles[dataSourcesCandlesIndex].DataSource.PointsSlippage; //если текущая свечка - гэп, убираем базовое проскальзывание, и оставляем только вычисляемое, чтобы цена исполнения заявки была по худщей цене в свечке, и если объем большой то и проскальзывание было
                                    slippage += Slippage(dataSourcesCandles[dataSourcesCandlesIndex], fileIndexes[dataSourcesCandlesIndex], candleIndexes[dataSourcesCandlesIndex], order.Count); //добавляем проскальзывание
                                    slippage = order.Direction == true ? slippage : -slippage; //для покупки проскальзывание идет вверх, для продажи вниз
                                    dealPrice = dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].C + slippage * dataSourcesCandles[dataSourcesCandlesIndex].DataSource.PriceStep;
                                }

                                if (gaps[dataSourcesCandlesIndex]) //если текущая свечка - гэп, устанавливаем худшую цену
                                {
                                    dealPrice = order.Direction ? dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H : dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L;
                                }
                                isMakeADeals = MakeADeal(account, order, dealCount, dealPrice, dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime) ? true : isMakeADeals;
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
            //определяем хватает ли средств на минимальное количество лотов, если да, определяем хватает ли средств на lotsCount, если нет устанавливаем минимально доступное количество
            //стоимость минимального количества лотов
            double minLotsCost = order.DataSource.MarginType.Id == 2 ? order.DataSource.MarginCost * order.DataSource.MinLotMarginPartCost : price * order.DataSource.MinLotMarginPartCost; //для фиксированной маржи, устанавливаем фиксированную маржу источника данных, помноженную на часть стоимости минимального количества лотов относительно маржи, для маржи с графика, устанавливаем стоимость с график, помноженную на часть стоимости минимального количества лотов относительно маржи
            double minLotsComission = order.DataSource.Comissiontype.Id == 2 ? minLotsCost * (order.DataSource.Comission / 100) : order.DataSource.Comission; //комиссия на минимальное количество лотов
            double freeDeposit = account.FreeForwardDepositCurrencies.Where(i => i.Currency == order.DataSource.Currency).First().Deposit; //свободный остаток в валюте источника данных
            double takenDeposit = account.TakenForwardDepositCurrencies.Where(i => i.Currency == order.DataSource.Currency).First().Deposit; //занятые средства на открытые позиции в валюте источника данных
            //определяем максимально доступное количество лотов
            decimal maxLotsCount = (decimal)ModelFunctions.TruncateToIncrement(freeDeposit / (minLotsCost + minLotsComission), (double)order.DataSource.MinLotCount);
            decimal reverseDirectionLotsCount = 0;//количество лотов в открытой позиции с обратным направлением
            decimal currentDirectionLotsCount = 0;//количество лотов в открытой позиции с текущим направлением
            foreach (Deal deal in account.CurrentPosition)
            {
                if (deal.DataSource == order.DataSource && deal.Order.Direction != order.Direction) //если сделка совершена по тому же источнику данных что и заявка, но отличается с ней в направлении
                {
                    if(deal.Order.Direction != order.Direction)
                    {
                        reverseDirectionLotsCount += deal.Count;
                    }
                    else
                    {
                        currentDirectionLotsCount += deal.Count;
                    }
                }
            }
            maxLotsCount += reverseDirectionLotsCount; //прибавляем к максимально доступному количеству лотов, количество лотов в открытой позиции с обратным направлением
            //если это не форвардное тестирование с торговлей депозитом, устанавливаем максимально доступное количество лотов в минимальное количество лотов
            if (account.IsForwardDepositTrading == false)
            {
                maxLotsCount = currentDirectionLotsCount == 0 ? order.DataSource.MinLotCount : 0; //если количество лотов в открытых сделках с текущим направлением равно нулю, устанавливаем доступное количество лотов в минимальное, если же есть открытые позиции с текущим направлением, устанавливаем в ноль
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
                account.AllDeals.Add(new Deal { Number = account.AllDeals.Count, IdDataSource = order.DataSource.Id, DataSource = order.DataSource, OrderNumber = order.Number, Order = order, Direction = order.Direction, Price = price, Count = dealLotsCount, DateTime = dateTime });
                Deal currentDeal = new Deal { Number = account.AllDeals.Count, IdDataSource = order.DataSource.Id, DataSource = order.DataSource, OrderNumber = order.Number, Order = order, Direction = order.Direction, Price = price, Count = dealLotsCount, DateTime = dateTime };
                account.CurrentPosition.Add(currentDeal);
                isMakeADeal = true; //запоминаем что была совершена сделка
                //вычитаем комиссию на сделку из свободных средств
                double comission = (double)((decimal)minLotsComission * (dealLotsCount / order.DataSource.MinLotCount));
                freeDeposit -= (double)((decimal)minLotsComission * (dealLotsCount / order.DataSource.MinLotCount));
                account.Totalcomission += comission;
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
                        double resultMoney = (double)((decimal)(priceSell - priceBuy) / (decimal)account.CurrentPosition[i].DataSource.PriceStep * (decimal)account.CurrentPosition[i].DataSource.CostPriceStep * (decrementCount / account.CurrentPosition[i].DataSource.MinLotCount)); //количество пунктов трейда * стоимость 1 пункта * количество минимального количества лотов
                        freeDeposit += resultMoney;
                        //определяем стоимость закрытых лотов в открытой позиции, вычитаем её из занятых средств и прибавляем к свободным
                        double closedCost = account.CurrentPosition[i].Order.DataSource.MarginType.Id == 2 ? account.CurrentPosition[i].Order.DataSource.MarginCost * order.DataSource.MinLotMarginPartCost : account.CurrentPosition[i].Price * order.DataSource.MinLotMarginPartCost;
                        closedCost = (double)((decimal)closedCost * (decrementCount / account.CurrentPosition[i].DataSource.MinLotCount)); //умножаем стоимость на количество
                        takenDeposit -= closedCost; //вычитаем из занятых на открытые позиции средств
                        freeDeposit += closedCost; //прибавляем к свободным средствам
                        //вычитаем закрытое количесво из открытых позиций
                        account.CurrentPosition[i].Count -= decrementCount;
                        currentDeal.Count -= decrementCount;
                    }
                    i++;
                }
                //определяем стоимость занятых средств на оставшееся (незакрытое) количество лотов текущей сделки, вычитаем её из сободных средств и добавляем к занятым
                double currentCost = currentDeal.DataSource.MarginType.Id == 2 ? currentDeal.DataSource.MarginCost * order.DataSource.MinLotMarginPartCost : currentDeal.Price * order.DataSource.MinLotMarginPartCost;
                currentCost = (double)((decimal)currentCost * (currentDeal.Count / order.DataSource.MinLotCount)); //умножаем стоимость на количество
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
                account.FreeForwardDepositCurrencies = CalculateDepositCurrrencies(freeDeposit, order.DataSource.Currency, dateTime);
                account.TakenForwardDepositCurrencies = CalculateDepositCurrrencies(takenDeposit, order.DataSource.Currency, dateTime);
                //если открытые позиции пусты и была совершена сделка, записываем состояние депозита
                if (account.CurrentPosition.Count == 0 && isMakeADeal)
                {
                    account.DepositCurrenciesChanges.Add(account.FreeForwardDepositCurrencies);
                }
            }
            return isMakeADeal;
        }

        private List<DepositCurrency> CalculateDepositCurrrencies(double deposit, Currency inputCurrency, DateTime dateTime) //возвращает значения депозита во всех валютах
        {
            List<DepositCurrency> depositCurrencies = new List<DepositCurrency>();

            double dollarCostDeposit = deposit / inputCurrency.DollarCost; //определяем долларовую стоимость
            foreach (Currency currency in _modelData.Currencies)
            {
                //переводим доллоровую стоимость в валютную, умножая на стоимость 1 доллара
                double cost = dollarCostDeposit * currency.DollarCost;
                depositCurrencies.Add(new DepositCurrency { Currency = currency, Deposit = cost, DateTime = dateTime });
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
            else if (testing.IsConsiderNeighbours) //если поиск топ-модели учитывает соседей, определяем размер группы по всем неединичным осям, затем формируем список с группами, находим самую лучшую группу и ищем топ-модель (если из-за фильтров не найдена модель, ищем топ-модель в следующей лучшей группе, пока не кончатся группы)
            {
                List<int> nonSingleAlgorithmParameterIndexes = new List<int>(); //индексы параметров алгоритма, которые имеют два и более значений
                for(int i = 0; i < testing.Algorithm.AlgorithmParameters.Count; i++)
                {
                    int currentParameterCountValues = testing.AlgorithmParametersAllIntValues[i].Count > 0 ? testing.AlgorithmParametersAllIntValues[i].Count : testing.AlgorithmParametersAllDoubleValues[i].Count; //количество значений у текущего параметра
                    if(currentParameterCountValues >= 2)
                    {
                        nonSingleAlgorithmParameterIndexes.Add(i);
                    }
                }
                double requiredGroupSize = testBatch.OptimizationTestRuns.Count * (testing.SizeNeighboursGroupPercent / 100); //требуемый размер группы
                int lastGroupSize = 0;
                int currentGroupSize = 0;
                int groupWidth = 1; //размер группы во всех осях
                bool isLastGroupSizeNearThanCurrent = false; //прошлый размер ближе к требуемому размеру чем текущий
                bool isGroupWidthEqualMaxParameterCountValues = false; //равняется ли текущий размер группы во всех осях максимальному количеству значений параметров
                do
                {
                    groupWidth++;
                    lastGroupSize = currentGroupSize;
                    currentGroupSize = 1;
                    //проходим по всем неединичным параметрам, и определяем размер группы для текущего размера групы по всем осям
                    for (int i = 0; i < nonSingleAlgorithmParameterIndexes.Count; i++)
                    {
                        isGroupWidthEqualMaxParameterCountValues = true;
                        int currentParameterCountValues = testing.AlgorithmParametersAllIntValues[nonSingleAlgorithmParameterIndexes[i]].Count > 0 ? testing.AlgorithmParametersAllIntValues[nonSingleAlgorithmParameterIndexes[i]].Count : testing.AlgorithmParametersAllDoubleValues[nonSingleAlgorithmParameterIndexes[i]].Count; //количество значений у текущего параметра
                        if (currentParameterCountValues > groupWidth)
                        {
                            currentGroupSize *= groupWidth;
                            isGroupWidthEqualMaxParameterCountValues = false; //отмечаем что размер группы во всех осях еще не равняется максимальному количеству значений параметров
                        }
                        else //если текущий размер группы во всех осях превышает количество значений текущего параметра, считаем что размер группы по текущей оси равняется количеству значений текущего параметра
                        {
                            currentGroupSize *= currentParameterCountValues;
                        }
                    }
                    isLastGroupSizeNearThanCurrent = groupWidth == 2 ? false : Math.Abs(requiredGroupSize - currentGroupSize) > Math.Abs(requiredGroupSize - lastGroupSize); //для первой итерации устанавливаем false, для следующих, если прошлый размер группы ближе к требуемому устанавливаем в true
                } while (isGroupWidthEqualMaxParameterCountValues == false && isLastGroupSizeNearThanCurrent == false);
                groupWidth = isLastGroupSizeNearThanCurrent ? groupWidth - 1 : groupWidth;
                List<int> nonSingleAlgorithmParameterGroupSize = new List<int>(); //размеры группы для каждого параметра (сколько значений каждого параметра находится в одной группе)
                List<int> nonSingleAlgorithmParameterCountValues = new List<int>(); //количество значений у каждого параметра
                for (int i = 0; i < nonSingleAlgorithmParameterIndexes.Count; i++)
                {
                    int parameterIndex = nonSingleAlgorithmParameterIndexes[i]; //индекс параметра
                    int currentParameterCountValues = testing.AlgorithmParametersAllIntValues[parameterIndex].Count > 0 ? testing.AlgorithmParametersAllIntValues[parameterIndex].Count : testing.AlgorithmParametersAllDoubleValues[parameterIndex].Count; //количество значений у текущего параметра
                    nonSingleAlgorithmParameterGroupSize.Add(currentParameterCountValues > groupWidth ? groupWidth : currentParameterCountValues);
                    nonSingleAlgorithmParameterCountValues.Add(currentParameterCountValues);
                }
                //формируем группы
                List<List<TestRun>> groups = new List<List<TestRun>>(); //группы, каждая группа содержит тестовые прогоны группы
                List<List<int>> groupsIndexes = new List<List<int>>(); //группы, каждая группа содержит тестовые прогоны группы
                List<List<string>> groupsParameterValues = new List<List<string>>(); //группы, каждая группа содержит тестовые прогоны группы
                List<int> nonSingleAlgorithmParameterOffsets = new List<int>(); //список со смещениями для каждого неединичного параметра группы относительно начального значения. Например (0,0,0) означает что текущая группа начинается с первых значений у каждого параметра, (0,0,1) означает что текущая группа начинается с первого значения для первых двух параметров, а третий параметр начинается со второго значения, то есть если группа 3х3х3, то значения параметров будут с индексами: (0-2,0-2,1-3) Так же при переходе на следующую группу, будет прибавлено 1 к последнему параметру, и если размер группы не помещается в нем, то будет переход на следующем параметре, то есть с (0,0,2) перейдет на (0,1,0)
                nonSingleAlgorithmParameterOffsets.AddRange(Enumerable.Repeat(0, nonSingleAlgorithmParameterGroupSize.Count)); //заполняем нулями
                bool isGroupsEnd = false; //кончились ли группы
                do
                {
                    List<TestRun> group = new List<TestRun>(); //тестове прогоны группы
                    List<int> groupIndexes = new List<int>(); //тестове прогоны группы
                    List<string> groupParameterValues = new List<string>(); //тестове прогоны группы
                    List<int> parameterIndexesCombination = new List<int>(nonSingleAlgorithmParameterOffsets); //индексы параметров комбинаций. Копируем начальные индексы текущей группы, и перебираем все комбинации
                    bool isCombinationsEnd = false; //кончились ли комбинации параметров текущей группы
                    do
                    {
                        //формируем список с текущей комбинацией значений параметров
                        List<AlgorithmParameterValue> algorithmParameterValues = new List<AlgorithmParameterValue>();
                        for (int i = 0; i < nonSingleAlgorithmParameterIndexes.Count; i++)
                        {
                            int parameterIndex = nonSingleAlgorithmParameterIndexes[i]; //индекс параметра
                            int valueIndex = parameterIndexesCombination[i]; //индекс значения параметра
                            algorithmParameterValues.Add(new AlgorithmParameterValue { AlgorithmParameter = testing.Algorithm.AlgorithmParameters[parameterIndex], IntValue = testing.Algorithm.AlgorithmParameters[parameterIndex].ParameterValueType.Id == 1 ? testing.AlgorithmParametersAllIntValues[parameterIndex][valueIndex] : 0, DoubleValue = testing.Algorithm.AlgorithmParameters[parameterIndex].ParameterValueType.Id == 2 ? testing.AlgorithmParametersAllDoubleValues[parameterIndex][valueIndex] : 0 });
                        }
                        int testRunIndex = ModelFunctions.FindTestRunIndexByAlgorithmParameterValues(testBatch.OptimizationTestRuns, algorithmParameterValues);
                        group.Add(testBatch.OptimizationTestRuns[testRunIndex]); //записываем индекс тестового прогона с текущей комбинацией значений параметров
                        groupIndexes.Add(testRunIndex);
                        string testRunParameterValues = "";
                        for(int i = 0; i < nonSingleAlgorithmParameterIndexes.Count; i++)
                        {
                            int pIndex = nonSingleAlgorithmParameterIndexes[i];
                            int valIndex = parameterIndexesCombination[i]; //индекс значения параметра
                            testRunParameterValues += testing.Algorithm.AlgorithmParameters[pIndex].ParameterValueType.Id == 1 ? testing.AlgorithmParametersAllIntValues[pIndex][valIndex].ToString() : testing.AlgorithmParametersAllDoubleValues[pIndex][valIndex].ToString();
                            testRunParameterValues += ", ";
                        }
                        testRunParameterValues = testRunParameterValues.Substring(0, testRunParameterValues.Length - 2);
                        groupParameterValues.Add(testRunParameterValues);
                        //переходим на следующую комбинацию значений параметров
                        parameterIndexesCombination[parameterIndexesCombination.Count - 1]++; //увеличиваем индекс значения последнего параметра
                        int index = parameterIndexesCombination.Count - 1;
                        //пока индекс значения параметра превышет размер группы для данного параметра, сбрасываем индекс значения в начальный, и переходим у предыдущего параметра на следующий индекс значения
                        while (parameterIndexesCombination[index] - nonSingleAlgorithmParameterOffsets[index] >= nonSingleAlgorithmParameterGroupSize[index])
                        {
                            parameterIndexesCombination[index] = nonSingleAlgorithmParameterOffsets[index]; //сбрасываем индекс значения в начальный
                            if (index > 0)
                            {
                                index--;
                                parameterIndexesCombination[index]++; //переходим у предыдущего параметра на следующий индекс значения
                            }
                            else //если индекс занчения текущего параметра вышел за границы размера группы для текущего параметра, и этот параметр имеет индекс 0, значит мы перебрали все комбинации
                            {
                                isCombinationsEnd = true;
                            }
                        }
                    } while (isCombinationsEnd == false);
                    groups.Add(group);
                    groupsIndexes.Add(groupIndexes);
                    groupsParameterValues.Add(groupParameterValues);

                    //переходим на следующую группу
                    nonSingleAlgorithmParameterOffsets[nonSingleAlgorithmParameterOffsets.Count - 1]++; //увеличиваем индекс значения последнего параметра
                    int indexPar = nonSingleAlgorithmParameterOffsets.Count - 1;
                    //пока индекс значения параметра плюс размер группы у данного параметра превышет количество значений данного параметра, сбрасываем индекс значения в начальный, и переходим у предыдущего параметра на следующий индекс значения
                    while (nonSingleAlgorithmParameterOffsets[indexPar] + nonSingleAlgorithmParameterGroupSize[indexPar] > nonSingleAlgorithmParameterCountValues[indexPar])
                    {
                        nonSingleAlgorithmParameterOffsets[indexPar] = 0; //сбрасываем индекс значения в начальный
                        if (indexPar > 0)
                        {
                            indexPar--;
                            nonSingleAlgorithmParameterOffsets[indexPar]++; //переходим у предыдущего параметра на следующий индекс значения
                        }
                        else //если индекс занчения текущего параметра вышел за границы размера группы для текущего параметра, и этот параметр имеет индекс 0, значит мы перебрали все группы
                        {
                            isGroupsEnd = true;
                        }
                    }
                } while (isGroupsEnd == false);

                //формируем список со средними значениями критерия оценки групп
                List<double> averageGroupsValues = new List<double>();
                //проходим по всем группам
                for (int i = 0; i < groups.Count; i++)
                {
                    double totalGroupValue = 0;
                    for (int k = 0; k < groups[i].Count; k++)
                    {
                        totalGroupValue += groups[i][k].EvaluationCriteriaValues[testing.TopModelEvaluationCriteriaIndex].DoubleValue;
                    }
                    averageGroupsValues.Add(totalGroupValue / groups[i].Count);
                }

                //сортируем группы по среднему значению критерия оценки в порядке убывания
                List<TestRun> saveGroup; //элемент списка для сохранения после удаления из списка
                double saveValue; //элемент списка для сохранения после удаления из списка
                for (int i = 0; i < averageGroupsValues.Count; i++)
                {
                    for (int k = 0; k < averageGroupsValues.Count - 1; k++)
                    {
                        if (averageGroupsValues[k] < averageGroupsValues[k + 1])
                        {
                            saveGroup = groups[k];
                            groups[k] = groups[k + 1];
                            groups[k + 1] = saveGroup;

                            saveValue = averageGroupsValues[k];
                            averageGroupsValues[k] = averageGroupsValues[k + 1];
                            averageGroupsValues[k + 1] = saveValue;
                        }
                    }
                }

                bool isTopModelFind = false;
                int groupIndex = 0;
                //проходим по всем группам, сортируем тесты в группе в порядке убывания критерия оценки, и ищем тест в группе который соответствует фильтрам
                while (isTopModelFind == false && groupIndex < groups.Count)
                {
                    //сортируем тесты в группе в порядке убывания критерия оценки
                    for (int i = 0; i < groups[groupIndex].Count; i++)
                    {
                        for (int k = 0; k < groups[groupIndex].Count - 1; k++)
                        {
                            if (groups[groupIndex][k].EvaluationCriteriaValues[testing.TopModelEvaluationCriteriaIndex].DoubleValue < groups[groupIndex][k + 1].EvaluationCriteriaValues[testing.TopModelEvaluationCriteriaIndex].DoubleValue)
                            {
                                TestRun saveTestRun = groups[groupIndex][k];
                                groups[groupIndex][k] = groups[groupIndex][k + 1];
                                groups[groupIndex][k + 1] = saveTestRun;
                            }
                        }
                    }
                    //проходим по тестам группы, и ищем первый, который соответствует фильтрам
                    int tRunIndex = 0;
                    while (isTopModelFind == false && tRunIndex < groups[groupIndex].Count)
                    {
                        //проходим по всем фильтрам
                        bool isFilterFail = false;
                        foreach (TopModelFilter topModelFilter in testing.TopModelCriteria.TopModelFilters)
                        {
                            if (topModelFilter.CompareSign == CompareSign.GetMore()) //знак сравнения фильтра Больше
                            {
                                if (groups[groupIndex][tRunIndex].EvaluationCriteriaValues.Where(j => j.EvaluationCriteria == topModelFilter.EvaluationCriteria).First().DoubleValue <= topModelFilter.Value)
                                {
                                    isFilterFail = true;
                                }
                            }
                            else //знак сравнения фильтра Меньше
                            {
                                if (groups[groupIndex][tRunIndex].EvaluationCriteriaValues.Where(j => j.EvaluationCriteria == topModelFilter.EvaluationCriteria).First().DoubleValue >= topModelFilter.Value)
                                {
                                    isFilterFail = true;
                                }
                            }
                        }
                        //если testRun удовлетворяет всем фильтрам, записываем его как топ-модель
                        if (isFilterFail == false)
                        {
                            testBatch.SetTopModel(groups[groupIndex][tRunIndex]);
                            isTopModelFind = true;
                            //записываем номера соседних тестов топ-модели
                            foreach(TestRun testRun in groups[groupIndex])
                            {
                                if(testRun.Number != testBatch.TopModelTestRunNumber)
                                {
                                    testBatch.NeighboursTestRunNumbers.Add(testRun.Number);
                                }
                            }
                        }
                        tRunIndex++;
                    }
                    groupIndex++;
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
                else if(testRunProfit > 0)
                {
                    profitCount++;
                    profitMoney += testRunProfit;
                }
            }
            //записываем статистическую значимость
            testBatch.StatisticalSignificance.Add(profitCount);
            testBatch.StatisticalSignificance.Add(profitMoney);
            testBatch.StatisticalSignificance.Add(lossCount);
            testBatch.StatisticalSignificance.Add(lossMoney);

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

        private void FindSurfaceAxes(TestBatch testBatch, Testing testing) //находит оси для тремхмерной поверхности
        {
            List<int> nonSingleAlgorithmParameterIndexes = new List<int>(); //индексы параметров алгоритма, которые имеют два и более значений
            List<int> singleAlgorithmParameterIndexes = new List<int>(); //индексы параметров алгоритма, которые имеют одно значение
            for (int i = 0; i < testing.Algorithm.AlgorithmParameters.Count; i++)
            {
                int currentParameterCountValues = testing.AlgorithmParametersAllIntValues[i].Count > 0 ? testing.AlgorithmParametersAllIntValues[i].Count : testing.AlgorithmParametersAllDoubleValues[i].Count; //количество значений у текущего параметра
                if (currentParameterCountValues >= 2)
                {
                    nonSingleAlgorithmParameterIndexes.Add(i);
                }
                else
                {
                    singleAlgorithmParameterIndexes.Add(i);
                }
            }

            List<List<int>> allAxisCombinations = new List<List<int>>(); //все комбинации осей
            bool[] used = Enumerable.Repeat(false, nonSingleAlgorithmParameterIndexes.Count).ToArray();
            ModelFunctions.CreatePermutation(nonSingleAlgorithmParameterIndexes.Count, used, new List<int>(), allAxisCombinations); //создаем перестановки

            double softlyVolatilityValue = 0; //самое гладкое значение волатильности
            List<int> softlyParametersCombination = new List<int>(); //комбинация с самым гладким значением волатильности
            for(int combinationIndex = 0; combinationIndex < allAxisCombinations.Count; combinationIndex++) //проходим по всем комбинациям
            {
                //формируем левую и верхнюю оси плоскости
                List<int> leftAxisAlgorithmParameterIndexes = new List<int>(); //индексы параметров алгоритма левой оси
                List<int> topAxisAlgorithmParameterIndexes = new List<int>(); //индексы параметров алгоритма верхней оси
                List<int> leftAxisAlgorithmParameterCountValues = new List<int>(); //количество значений параметров алгоритма левой оси
                List<int> topAxisAlgorithmParameterCountValues = new List<int>(); //количество значений параметров алгоритма верхней оси
                for (int i = 0; i < allAxisCombinations[combinationIndex].Count; i++)
                {
                    if (i < allAxisCombinations[combinationIndex].Count / 2.0)
                    {
                        int parameterIndex = nonSingleAlgorithmParameterIndexes[allAxisCombinations[combinationIndex][i]]; //индекс параметра
                        int currentParameterCountValues = testing.AlgorithmParametersAllIntValues[parameterIndex].Count > 0 ? testing.AlgorithmParametersAllIntValues[parameterIndex].Count : testing.AlgorithmParametersAllDoubleValues[parameterIndex].Count; //количество значений у текущего параметра
                        leftAxisAlgorithmParameterIndexes.Add(parameterIndex); //в левую ось добавляем первую половину параметров
                        leftAxisAlgorithmParameterCountValues.Add(currentParameterCountValues); //запоминаем количество значений у параметра
                    }
                    else
                    {
                        int parameterIndex = nonSingleAlgorithmParameterIndexes[allAxisCombinations[combinationIndex][i]]; //индекс параметра
                        int currentParameterCountValues = testing.AlgorithmParametersAllIntValues[parameterIndex].Count > 0 ? testing.AlgorithmParametersAllIntValues[parameterIndex].Count : testing.AlgorithmParametersAllDoubleValues[parameterIndex].Count; //количество значений у текущего параметра
                        topAxisAlgorithmParameterIndexes.Add(parameterIndex); //в верхнюю ось добавляем вторую половину параметров
                        topAxisAlgorithmParameterCountValues.Add(currentParameterCountValues); //запоминаем количество значений у параметра
                    }
                }
                int[] leftAxisAlgorithmParameterValuesCurrentCombination = Enumerable.Repeat(0, leftAxisAlgorithmParameterIndexes.Count).ToArray(); //текущая комбинация значений параметров левой оси
                int[] topAxisAlgorithmParameterValuesCurrentCombination = Enumerable.Repeat(0, topAxisAlgorithmParameterIndexes.Count).ToArray(); //текущая комбинация значений параметров верхней оси

                double totalVolatility = 0; //суммарная волатильность
                int directionIteration = 1; //номер итерации, которые определяет направление прохода по тестовым прогонам. 1 - слева-направо и сверху-вниз, 2 - сверху-вниз и слева-направо
                //дважды делаем проход по всем тестовым прогонам, на первой итерации по колонкам с переходом по строкам (слева-направо и сверху-вниз), на второй итерации по строкам с переходом по колонкам (сверху-вниз и слева-направо)
                do
                {
                    double currentDoubleValue = 0; //значение текущего теста
                    double lastDoubleValue = 0;//значение прошлого теста
                    int numberInOneLine = 0; //номер теста в одной линии (в строке или колонке, в зависимости от направления движения слева-направо и сверху-вниз или сверху-вниз и слева-направо)
                    bool isAlgorithmParameterValuesCombinationEnd = false;
                    //проходим по всем тестовым прогонам и вычисляем волатильность
                    while (isAlgorithmParameterValuesCombinationEnd == false)
                    {
                        //формируем список с текущей комбинацией значений параметров
                        List<AlgorithmParameterValue> algorithmParameterValues = new List<AlgorithmParameterValue>();
                        for (int i = 0; i < leftAxisAlgorithmParameterIndexes.Count; i++) //добавляем параметры левой оси
                        {
                            int parameterIndex = leftAxisAlgorithmParameterIndexes[i]; //индекс параметра
                            int valueIndex = leftAxisAlgorithmParameterValuesCurrentCombination[i]; //индекс значения параметра
                            algorithmParameterValues.Add(new AlgorithmParameterValue { AlgorithmParameter = testing.Algorithm.AlgorithmParameters[parameterIndex], IntValue = testing.Algorithm.AlgorithmParameters[parameterIndex].ParameterValueType.Id == 1 ? testing.AlgorithmParametersAllIntValues[parameterIndex][valueIndex] : 0, DoubleValue = testing.Algorithm.AlgorithmParameters[parameterIndex].ParameterValueType.Id == 2 ? testing.AlgorithmParametersAllDoubleValues[parameterIndex][valueIndex] : 0 });
                        }
                        for (int i = 0; i < topAxisAlgorithmParameterIndexes.Count; i++) //добавляем параметры верхней оси
                        {
                            int parameterIndex = topAxisAlgorithmParameterIndexes[i]; //индекс параметра
                            int valueIndex = topAxisAlgorithmParameterValuesCurrentCombination[i]; //индекс значения параметра
                            algorithmParameterValues.Add(new AlgorithmParameterValue { AlgorithmParameter = testing.Algorithm.AlgorithmParameters[parameterIndex], IntValue = testing.Algorithm.AlgorithmParameters[parameterIndex].ParameterValueType.Id == 1 ? testing.AlgorithmParametersAllIntValues[parameterIndex][valueIndex] : 0, DoubleValue = testing.Algorithm.AlgorithmParameters[parameterIndex].ParameterValueType.Id == 2 ? testing.AlgorithmParametersAllDoubleValues[parameterIndex][valueIndex] : 0 });
                        }
                        currentDoubleValue = testBatch.OptimizationTestRuns[ModelFunctions.FindTestRunIndexByAlgorithmParameterValues(testBatch.OptimizationTestRuns, algorithmParameterValues)].EvaluationCriteriaValues[testing.TopModelEvaluationCriteriaIndex].DoubleValue;

                        string algorithmParameterValuesString = "";
                        foreach(AlgorithmParameterValue algorithmParameterValue in algorithmParameterValues)
                        {
                            algorithmParameterValuesString += algorithmParameterValue.AlgorithmParameter.Name + "=";
                            algorithmParameterValuesString += algorithmParameterValue.AlgorithmParameter.ParameterValueType.Id == 1 ? algorithmParameterValue.IntValue.ToString() : algorithmParameterValue.DoubleValue.ToString();
                            algorithmParameterValuesString += ", ";
                        }
                        algorithmParameterValuesString = algorithmParameterValuesString.Substring(0, algorithmParameterValuesString.Length - 2);

                        if (numberInOneLine > 0) //если на линии с тестами был уже до этого хотя бы один тест, вычисляем разницу между текущим и прошлым тестом
                        {
                            totalVolatility += Math.Abs(currentDoubleValue - lastDoubleValue);
                        }
                        lastDoubleValue = currentDoubleValue;
                        numberInOneLine++;

                        //переходим на следующую комбинацию значений параметров
                        //переходим на следующее значение параметра внутри линии
                        bool isNewLine = false; //нужно ли переходить на следующую линию (строку или столбец в зависимости от направления)
                        if (directionIteration == 1) //если это проход слева-направо и сверху-вниз
                        {
                            topAxisAlgorithmParameterValuesCurrentCombination[0]++; //поскольку первый параметр - самый близкий к поверхности, увеличиваем его
                        }
                        else //иначе это проход сверху-вниз и слева-направо
                        {
                            leftAxisAlgorithmParameterValuesCurrentCombination[0]++; //поскольку первый параметр - самый близкий к поверхности, увеличиваем его
                        }
                        int indexPar = 0;
                        bool inLineMoveCondition = directionIteration == 1 ? topAxisAlgorithmParameterValuesCurrentCombination[indexPar] >= topAxisAlgorithmParameterCountValues[indexPar] : leftAxisAlgorithmParameterValuesCurrentCombination[indexPar] >= leftAxisAlgorithmParameterCountValues[indexPar];
                        //пока индекс значения параметра превышет количество значений у данного параметра, сбрасываем индекс значения в начальный, и переходим у следующего параметра на следующий индекс значения
                        while (inLineMoveCondition)
                        {
                            if (directionIteration == 1) //если это проход слева-направо и сверху-вниз
                            {
                                topAxisAlgorithmParameterValuesCurrentCombination[indexPar] = 0;
                                if (indexPar < topAxisAlgorithmParameterValuesCurrentCombination.Length - 1)
                                {
                                    indexPar++;
                                    topAxisAlgorithmParameterValuesCurrentCombination[indexPar]++; //переходим у следующего параметра на следующий индекс значения
                                }
                                else //если индекс значения текущего параметра превысил количество значений текущего параметра, и это последний параметр, значит нужно перейти на следующую линию
                                {
                                    isNewLine = true;
                                }
                            }
                            else //иначе это проход сверху-вниз и слева-направо
                            {
                                leftAxisAlgorithmParameterValuesCurrentCombination[indexPar] = 0;
                                if (indexPar < leftAxisAlgorithmParameterValuesCurrentCombination.Length - 1)
                                {
                                    indexPar++;
                                    leftAxisAlgorithmParameterValuesCurrentCombination[indexPar]++; //переходим у следующего параметра на следующий индекс значения
                                }
                                else //если индекс значения текущего параметра превысил количество значений текущего параметра, и это последний параметр, значит нужно перейти на следующую линию
                                {
                                    isNewLine = true;
                                }
                            }

                            inLineMoveCondition = directionIteration == 1 ? topAxisAlgorithmParameterValuesCurrentCombination[indexPar] >= topAxisAlgorithmParameterCountValues[indexPar] : leftAxisAlgorithmParameterValuesCurrentCombination[indexPar] >= leftAxisAlgorithmParameterCountValues[indexPar];
                        }

                        //переходим на следующую линию
                        if (isNewLine)
                        {
                            numberInOneLine = 0;
                            if (directionIteration == 2) //если это проход сверху-вниз и слева-направо
                            {
                                topAxisAlgorithmParameterValuesCurrentCombination[0]++; //поскольку первый параметр - самый близкий к поверхности, увеличиваем его
                            }
                            else //иначе это проход слева-направо и сверху-вниз
                            {
                                leftAxisAlgorithmParameterValuesCurrentCombination[0]++; //поскольку первый параметр - самый близкий к поверхности, увеличиваем его
                            }
                            indexPar = 0;
                            bool outLineMoveCondition = directionIteration == 2 ? topAxisAlgorithmParameterValuesCurrentCombination[indexPar] >= topAxisAlgorithmParameterCountValues[indexPar] : leftAxisAlgorithmParameterValuesCurrentCombination[indexPar] >= leftAxisAlgorithmParameterCountValues[indexPar];
                            //пока индекс значения параметра превышет количество значений у данного параметра, сбрасываем индекс значения в начальный, и переходим у следующего параметра на следующий индекс значения
                            while (outLineMoveCondition)
                            {
                                if (directionIteration == 2) //если это проход сверху-вниз и слева-направо
                                {
                                    topAxisAlgorithmParameterValuesCurrentCombination[indexPar] = 0;
                                    if (indexPar < topAxisAlgorithmParameterValuesCurrentCombination.Length - 1)
                                    {
                                        indexPar++;
                                        topAxisAlgorithmParameterValuesCurrentCombination[indexPar]++; //переходим у следующего параметра на следующий индекс значения
                                    }
                                    else //если индекс значения текущего параметра превысил количество значений текущего параметра, и это последний параметр, значит перебрали все комбинации значений параметров
                                    {
                                        isAlgorithmParameterValuesCombinationEnd = true;
                                    }
                                }
                                else //иначе это проход слева-направо и сверху-вниз
                                {
                                    leftAxisAlgorithmParameterValuesCurrentCombination[indexPar] = 0;
                                    if (indexPar < leftAxisAlgorithmParameterValuesCurrentCombination.Length - 1)
                                    {
                                        indexPar++;
                                        leftAxisAlgorithmParameterValuesCurrentCombination[indexPar]++; //переходим у следующего параметра на следующий индекс значения
                                    }
                                    else //если индекс значения текущего параметра превысил количество значений текущего параметра, и это последний параметр, значит перебрали все комбинации значений параметров
                                    {
                                        isAlgorithmParameterValuesCombinationEnd = true;
                                    }
                                }

                                outLineMoveCondition = directionIteration == 2 ? topAxisAlgorithmParameterValuesCurrentCombination[indexPar] >= topAxisAlgorithmParameterCountValues[indexPar] : leftAxisAlgorithmParameterValuesCurrentCombination[indexPar] >= leftAxisAlgorithmParameterCountValues[indexPar];
                            }
                        }
                    }
                    directionIteration++;
                } while (directionIteration < 3);

                //получили значение суммарной волатильности для текущей комбинации осей
                //сравниваем текущее значение волатильности с самым гладким
                if (combinationIndex > 0)
                {
                    if (totalVolatility < softlyVolatilityValue)
                    {
                        softlyVolatilityValue = totalVolatility;
                        softlyParametersCombination = allAxisCombinations[combinationIndex];
                    }
                }
                else
                {
                    softlyVolatilityValue = totalVolatility;
                    softlyParametersCombination = allAxisCombinations[combinationIndex];
                }
            }
            //формируем и записываем оси поверхности
            testBatch.FirstSurfaceAxes = new List<AxesParameter>();
            testBatch.SecondSurfaceAxes = new List<AxesParameter>();
            for (int i = 0; i < softlyParametersCombination.Count; i++)
            {
                int parameterIndex = nonSingleAlgorithmParameterIndexes[softlyParametersCombination[i]]; //индекс параметра
                if (i < softlyParametersCombination.Count / 2.0)
                {
                    testBatch.FirstSurfaceAxes.Add(new AxesParameter { AlgorithmParameter = testing.Algorithm.AlgorithmParameters[parameterIndex], IsSingle = false }); //в первую ось добавляем первую половину параметров
                }
                else
                {
                    testBatch.SecondSurfaceAxes.Add(new AxesParameter { AlgorithmParameter = testing.Algorithm.AlgorithmParameters[parameterIndex], IsSingle = false }); //во вторую ось добавляем вторую половину параметров
                }
            }
            //добавляем в оси параметры, которые имеют одно значение
            for (int i = 0; i < singleAlgorithmParameterIndexes.Count; i++)
            {
                int parameterIndex = singleAlgorithmParameterIndexes[i]; //индекс параметра
                if(testBatch.FirstSurfaceAxes.Count <= testBatch.SecondSurfaceAxes.Count) //добавляем равномерно во все оси
                {
                    testBatch.FirstSurfaceAxes.Add(new AxesParameter { AlgorithmParameter = testing.Algorithm.AlgorithmParameters[parameterIndex], IsSingle = false });
                }
                else
                {
                    testBatch.SecondSurfaceAxes.Add(new AxesParameter { AlgorithmParameter = testing.Algorithm.AlgorithmParameters[parameterIndex], IsSingle = false });
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

        public void AlgorithmIndicatorCatalogElementCalculate(Testing testing, DataSourceCandles dataSourceCandles, AlgorithmIndicator algorithmIndicator, AlgorithmIndicatorCatalogElement algorithmIndicatorCatalogElement) //возвращает вычисленные значения индикатора алгоритма для свечек из dataSourceCandles
        {
            List<AlgorithmParameterValue> algorithmParameterValues = algorithmIndicatorCatalogElement.AlgorithmParameterValues;
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
            algorithmIndicatorCatalogElement.AlgorithmIndicatorValues = algorithmIndicatorValues;
            algorithmIndicatorCatalogElement.IsComplete = true;
        }
    }
}

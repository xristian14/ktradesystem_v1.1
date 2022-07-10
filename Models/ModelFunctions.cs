using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    public class ModelFunctions //класс содержит полезные функции
    {
        public static string SplitDigitsInt(int value) //разделяет целые разряды числа пробелами
        {
            string str = value.ToString();
            str = StringReverse(str);
            string result = str;
            if (str.Length >= 3)
            {
                result = str.Substring(0, 3);
            }
            for (int i = 3; i < str.Length; i++)
            {
                if(i % 3 == 0)
                {
                    result += " ";
                }
                result += str[i];
            }
            return StringReverse(result);
        }
        public static string SplitDigitsDouble(double value, int decimals = 15) //разделяет целые разряды числа пробелами, decimals - количество разрядов в дробной части
        {
            string result = "";
            if (value is double.NaN == false)
            {
                string minus = value < 0 ? "-" : "";
                string str = Math.Round(Math.Abs(value), decimals).ToString();
                int commaIndex = str.IndexOf(',');
                string beforeCommaStr = commaIndex != -1 ? str.Substring(0, commaIndex) : str;
                string afterCommaStr = commaIndex != -1 ? str.Substring(commaIndex, str.Length - commaIndex) : "";
                int beforeCommaStrLength = beforeCommaStr.Length;
                for (int i = beforeCommaStrLength - 3; i > 0; i -= 3)
                {
                    beforeCommaStr = beforeCommaStr.Insert(i, " ");
                }
                result = minus + beforeCommaStr + afterCommaStr;
            }
            else
            {
                result = "NaN";
            }
            return result;
        }
        public static string StringReverse(string value) //возвращает перевернутую строку
        {
            char[] charArray = value.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
        public static StringBuilder RemoveDuplicateSpaces(string str) //удаляет дублирующиеся пробелы в строке, и возвращает объект StringBuilder
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(str[0]); //добавляем первый символ, т.к. stringBuilder[stringBuilder.Length-1] в цикле будет обращаться к -1 индексу
            for (int k = 1; k < str.Length; k++)
            {
                if ((stringBuilder[stringBuilder.Length - 1] == ' ' && str[k] == ' ') == false) //если последний символ в stringBuilder = пробел и добавляемый = пробел, пропускаем, если же это ложно то добавляем символ в stringBuilder
                {
                    stringBuilder.Append(str[k]);
                }
            }
            return stringBuilder;
        }
        public static double RoundToIncrement(double x, double m) //функция округляет число до определенного множителя, например, RoundToIncrement(3.14, 0.2) вернет 3.2
        {
            return Math.Round(x / m) * m;
        }

        public static double TruncateToIncrement(double x, double m) //функция обрезает число до определенного множителя, например, TruncateToIncrement(3.35, 0.2) вернет 3.2
        {
            return Math.Truncate(x / m) * m;
        }

        public static double MarginCalculate(TestRun testRun) //возвращает среднее значение маржи. Суммирует значение маржи для каждой сделки, увеличивающей позицию, и делит это значение на количество таких сделок. Сделки, увеличивающие позицию вычисляются по списку сделок. Для каждого источника данных ведется свое значение купленных/проданных для определения того увеличивает данная сделка позицию или уменьшает.
        {
            double totalMargin = 0; //суммарное значение маржи для всех сделок, увеличивающих позицию
            int countDeals = 0; //количество сделок, увеличивающих позицию

            decimal[] lotsCount = new decimal[testRun.TestBatch.DataSourceGroup.DataSourceAccordances.Count]; //количество лотов в открытой позиции для каждого источника данных
            int[] lotsCountDataSourceId = new int[lotsCount.Length]; //id источников данных, которые соответствуют элементу в lotsCount
            for(int i = 0; i < lotsCount.Length; i++)
            {
                lotsCount[i] = 0; //ставим значение открытой позиции
                lotsCountDataSourceId[i] = testRun.TestBatch.DataSourceGroup.DataSourceAccordances[i].DataSource.Id; //ставим id источника данных которому соответствует данная открытая позиция
            }
            //проходим по всем сделкам
            foreach (Deal deal in testRun.Account.AllDeals)
            {
                //определяем индекс в lotsCountDataSourceId в котором находится id источника данных таое же как в текущей сделке
                int index = 0;
                while(lotsCountDataSourceId[index] != deal.IdDataSource)
                {
                    index++;
                }
                decimal lastLotsCount = lotsCount[index]; //прошлое количество лотов в открытой позиции
                lotsCount[index] += deal.Order.Direction ? deal.Count : -deal.Count; //если сделка на покупку, добавляем количество лотов, если на продажу вычитаем из открытой позиции
                if(Math.Abs(lastLotsCount) < Math.Abs(lotsCount[index])) //если прошлый модуль количества лотов меньше текущего, значит текущая сделка увеличила позицию
                {
                    //определяем маржу
                    double dealMargin = deal.DataSource.MarginType.Id == 2 ? deal.DataSource.MarginCost * deal.DataSource.MinLotMarginPartCost : deal.Price * deal.DataSource.MinLotMarginPartCost; //для фиксированной маржи, устанавливаем фиксированную маржу источника данных, помноженную на часть стоимости минимального количества лотов относительно маржи, для маржи с графика, устанавливаем стоимость с график, помноженную на часть стоимости минимального количества лотов относительно маржи
                    totalMargin += dealMargin;
                    countDeals++;
                }
            }

            double margin = countDeals > 0 ? totalMargin / countDeals : 1; //если количество сделок, увеличивающих позицию больше 0, устанавливаем среднее значение маржи, иначе - 1 (не 0, т.к. будет деление на 0)
            return margin;
        }

        public static void CreatePermutation(int count, bool[] used, List<int> prefix, List<List<int>> outList) //создает перестановки для количества элементов в count, used - масив с count количеством элементов, который должен быть заполнен false, формирует перестиновки в список outList
        {
            if (prefix.Count != count)
            {
                for (int i = 0; i < count; i++)
                {
                    if (used[i] == false)
                    {
                        used[i] = true;
                        prefix.Add(i);
                        CreatePermutation(count, used, prefix, outList);
                        used[i] = false;
                        prefix.RemoveAt(prefix.Count - 1);
                    }
                }
            }
            else
            {
                outList.Add(prefix);
            }
        }

        public static int FindTestRunIndexByAlgorithmParameterValues(List<TestRun> testRuns, List<AlgorithmParameterValue> algorithmParameterValues) //возвращает индекс первого тестового прогона с указанными значениями параметров алгоритма, если элемент не найден вернет -1
        {
            //ищем тестовый прогон с комбинацией значений параметров
            int index = -1;
            bool isAllEqual = true; //совпадают ли все значения присланных параметров со значениями параметров текущего тестового прогона
            do
            {
                index++; //увеличиваем индекс здесь, чтобы когда тестовый прогон будет найден, после выхода из цикла индекс сохранился на найденном тестовом прогоне
                isAllEqual = true;
                foreach(AlgorithmParameterValue algorithmParameterValue in algorithmParameterValues)
                {
                    if(algorithmParameterValue.AlgorithmParameter.ParameterValueType.Id == 1) //параметр типа int
                    {
                        isAllEqual = testRuns[index].AlgorithmParameterValues.Find(a => a.AlgorithmParameter.Id == algorithmParameterValue.AlgorithmParameter.Id).IntValue == algorithmParameterValue.IntValue ? isAllEqual : false;
                    }
                    else //параметр типа double
                    {
                        isAllEqual = testRuns[index].AlgorithmParameterValues.Find(a => a.AlgorithmParameter.Id == algorithmParameterValue.AlgorithmParameter.Id).DoubleValue == algorithmParameterValue.DoubleValue ? isAllEqual : false;
                    }
                }
            }
            while (isAllEqual == false && index < testRuns.Count);
            return isAllEqual ? index : -1; //если найден тестовый прогон с значениями параметров, возвращаем его индекс, иначе -1
        }

        public static void TestEvaluationCriteria(TestRun testRun) //здесь я отлаживаю скрипты критериев оценки
        {
            //наибольший выигрыш
            double maxProfit = 0;
            int defaulCurrencyIndex = testRun.Account.DepositCurrenciesChanges[0].FindIndex(a => a.Currency == testRun.Account.DefaultCurrency);
            double currentDeposit = 0;
            double lastDeposit = testRun.Account.DepositCurrenciesChanges[0][defaulCurrencyIndex].Deposit;
            for (int i = 1; i < testRun.Account.DepositCurrenciesChanges.Count; i++)
            {
                currentDeposit = testRun.Account.DepositCurrenciesChanges[i][defaulCurrencyIndex].Deposit;
                if (currentDeposit - lastDeposit > maxProfit)
                {
                    maxProfit = currentDeposit - lastDeposit;
                }
                lastDeposit = currentDeposit;
            }
            double ResultDoubleValue = maxProfit;
            string ResultStringValue = Math.Round(ResultDoubleValue, 1) + " " + testRun.Account.DefaultCurrency.Name;
        }
    }
}

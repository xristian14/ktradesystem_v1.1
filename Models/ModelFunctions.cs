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
            string str = Math.Round(value, decimals).ToString();
            if(str.IndexOf('.') != -1)
            {
                string[] arr = str.Split('.');
                result = SplitDigitsInt(int.Parse(arr[0])) + "." + arr[1];
            }
            else
            {
                result = SplitDigitsInt(int.Parse(str));
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
                    double dealMargin = deal.DataSource.Instrument.Id == 2 ? deal.DataSource.Cost : deal.Price; //если это фьючерс, устанавливаем стоимость фьючерса, иначе цену с графика
                    totalMargin += dealMargin;
                    countDeals++;
                }
            }

            double margin = countDeals > 0 ? totalMargin / countDeals : 1; //если количество сделок, увеличивающих позицию больше 0, устанавливаем среднее значение маржи, иначе - 1 (не 0, т.к. будет деление на 0)
            return margin;
        }

        public static void TestEvaluationCriteria(TestRun testRun) //здесь я отлаживаю скрипты критериев оценки
        {
            int countWin = 0;
            double totalWin = 0;
            int countLoss = 0;
            double totalLoss = 0;
            double lastDeposit = 0;
            int iteration = 1;
            foreach (List<DepositCurrency> depositCurrencies in testRun.Account.DepositCurrenciesChanges)
            {
                if (iteration > 1)
                {
                    double currentDeposit = depositCurrencies.Where(j => j.Currency == testRun.Account.DefaultCurrency).First().Deposit;
                    if (currentDeposit > lastDeposit)
                    {
                        countWin++;
                        totalWin += currentDeposit - lastDeposit;
                    }
                    else
                    {
                        countLoss++;
                        totalLoss += currentDeposit - lastDeposit;
                    }
                }
                lastDeposit = depositCurrencies.Where(j => j.Currency == testRun.Account.DefaultCurrency).First().Deposit;
                iteration++;
            }
            double averageWin = countWin > 0 ? totalWin / countWin : 0;
            double averageLoss = countLoss > 0 ? totalLoss / countLoss : 0;
            double ResultDoubleValue = ((averageWin * (countWin - Math.Sqrt(countWin)) - averageLoss * (countLoss + Math.Sqrt(countLoss))) / ModelFunctions.MarginCalculate(testRun)) * 100;
            string ResultStringValue = Math.Round(ResultDoubleValue, 1) + " %";
        }
    }
}

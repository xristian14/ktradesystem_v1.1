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
    }
}

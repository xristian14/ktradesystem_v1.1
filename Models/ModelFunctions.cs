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
            string result = str.Substring(0, 3);
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
    }
}

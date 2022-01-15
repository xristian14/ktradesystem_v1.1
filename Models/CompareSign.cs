using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    public class CompareSign //класс для получения знаков сравнения для определения какой знак использован
    {
        public static string GetMax()
        {
            return "Max";
        }
        public static string GetMin()
        {
            return "Min";
        }
        public static string GetMore()
        {
            return "More";
        }
        public static string GetLess()
        {
            return "Less";
        }
    }
}

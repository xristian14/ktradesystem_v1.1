using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models;

namespace ktradesystem.ViewModels
{
    //класс описывает объекты которые будут использоваться как элементы списка в combobox для выбора группы источников данных
    public class DataSourceGroupTestingResultCombobox
    {
        public DataSourceGroup DataSourceGroup { get; set; } //группа источников данных
        public string NameDataSourceGroup { get; set; } //название, отображаемое в combobox
    }
}

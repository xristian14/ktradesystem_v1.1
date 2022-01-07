using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;
using System.Collections.ObjectModel;

namespace ktradesystem.ViewModels
{
    class DataSourcesForAddingDsGroupView //список с источниками данных, и выбранным источником
    {
        public DataSourceTemplate DataSourceTemplate { get; set; }
        public ObservableCollection<DataSource> DataSources { get; set; }
        public DataSource SelectedDataSource { get; set; }
    }
}

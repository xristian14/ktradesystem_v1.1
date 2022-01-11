using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    class Testing
    {
        public Algorithm Algorithm { get; set; }
        public List<DataSourceGroup> DataSourceGroups { get; set; }
        public TopModelCriteria TopModelCriteria { get; set; }
        public bool IsConsiderNeighbours { get; set; }
        public double SizeNeighboursGroupPercent { get; set; }
        public bool IsAxesSpecified { get; set; }
        public List<AxesParameter> AxesTopModelSearchPlane { get; set; }
        public bool IsForwardTesting { get; set; }
        public bool IsForwardDepositTrading { get; set; }
        public List<DepositCurrency> ForwardDepositCurrencies { get; set; }
        public DateTime StartPeriod { get; set; }
        public DateTime EndPeriod { get; set; }
        public DateTimeDuration DurationOptimizationTests { get; set; }

    }
}

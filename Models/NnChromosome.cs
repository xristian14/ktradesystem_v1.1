using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    public class NnChromosome
    {
        public List<TestRun> TestRuns { get; set; }
        public AForgeExtensions.Neuro.Learning.GeneticLearning.Chromosome Chromosome { get; set; }
        public List<NnDataSourceTemplate> NnDataSourceTemplatesOrder { get; set; }
        public List<double> NetOnMargins { get; set; }
        public void SetNnDataSourceTemplatesOrder(List<DataSourceTemplate> dataSourceTemplates)
        {

        }
        public List<Order> NeuralNetworkCompute(List<Candle[]> candles)
        {
            return new List<Order>();
        }
    }
}

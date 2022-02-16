using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models.Datatables
{
    [Serializable]
    public class Algorithm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<DataSourceTemplate> DataSourceTemplates { get; set; }
        public List<IndicatorParameterRange> IndicatorParameterRanges { get; set; }
        public List<AlgorithmParameter> AlgorithmParameters { get; set; }
        public string Script { get; set; }
        public Algorithm Clone()
        {
            Algorithm algorithm = new Algorithm { Id = Id, Name = Name, Description = Description, DataSourceTemplates = new List<DataSourceTemplate>(), IndicatorParameterRanges = new List<IndicatorParameterRange>(), AlgorithmParameters = new List<AlgorithmParameter>(), Script = Script };
            foreach(DataSourceTemplate dataSourceTemplate in DataSourceTemplates)
            {
                algorithm.DataSourceTemplates.Add(dataSourceTemplate.Clone());
            }
            foreach (IndicatorParameterRange indicatorParameterRange in IndicatorParameterRanges)
            {
                IndicatorParameterRange indicatorParameterRangeCopy = new IndicatorParameterRange { Id = indicatorParameterRange.Id, MinValue = indicatorParameterRange.MinValue, MaxValue = indicatorParameterRange.MaxValue, Step = indicatorParameterRange.Step, IsStepPercent = indicatorParameterRange.IsStepPercent, IdAlgorithm = indicatorParameterRange.IdAlgorithm };
                Indicator indicator = new Indicator { Id = indicatorParameterRange.Indicator.Id, Name = indicatorParameterRange.Indicator.Name, Description = indicatorParameterRange.Indicator.Description, Script = indicatorParameterRange.Indicator.Script, IsStandart = indicatorParameterRange.Indicator.IsStandart };
                IndicatorParameterTemplate indicatorParameterTemplate = new IndicatorParameterTemplate { Id = indicatorParameterRange.IndicatorParameterTemplate.Id, Name = indicatorParameterRange.IndicatorParameterTemplate.Name, Description = indicatorParameterRange.IndicatorParameterTemplate.Description, IdIndicator = indicatorParameterRange.IndicatorParameterTemplate.IdIndicator, ParameterValueType = indicatorParameterRange.IndicatorParameterTemplate.ParameterValueType, Indicator = indicator };
                List<IndicatorParameterTemplate> indicatorParameterTemplates = new List<IndicatorParameterTemplate>();
                foreach(IndicatorParameterTemplate indicatorParameterTemplate1 in indicatorParameterRange.Indicator.IndicatorParameterTemplates)
                {
                    indicatorParameterTemplates.Add(new IndicatorParameterTemplate { Id = indicatorParameterTemplate1.Id, Name = indicatorParameterTemplate1.Name, Description = indicatorParameterTemplate1.Description, IdIndicator = indicatorParameterTemplate1.IdIndicator, ParameterValueType = indicatorParameterTemplate1.ParameterValueType, Indicator = indicator });
                }
                indicator.IndicatorParameterTemplates = indicatorParameterTemplates;
                indicatorParameterRangeCopy.IndicatorParameterTemplate = indicatorParameterTemplate;
                indicatorParameterRangeCopy.Indicator = indicator;
                algorithm.IndicatorParameterRanges.Add(indicatorParameterRangeCopy);
            }
            foreach(AlgorithmParameter algorithmParameter in AlgorithmParameters)
            {
                algorithm.AlgorithmParameters.Add(new AlgorithmParameter { Id = algorithmParameter.Id, Name = algorithmParameter.Name, Description = algorithmParameter.Description, MinValue = algorithmParameter.MinValue, MaxValue = algorithmParameter.MaxValue, Step = algorithmParameter.Step, IsStepPercent = algorithmParameter.IsStepPercent, IdAlgorithm = algorithmParameter.IdAlgorithm, ParameterValueType = algorithmParameter.ParameterValueType });
            }
            return algorithm;
        }
    }
}

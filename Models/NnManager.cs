using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    public class NnManager
    {
        public int DataSourceGroupNumber { get; set; }
        public List<PrognosisCandles> PrognosisCandles { get; set; }
        public AForgeExtensions.Neuro.Learning.GeneticLearningNoTeacher GeneticLearningNoTeacher { get; set; }
        public AForgeExtensions.Neuro.Learning.GeneticLearning.Chromosome LastBestChromosome { get; set; }
        public NnChromosome[] NnPopulation { get; set; }
        public NnChromosome BestNnChromosome { get; set; }
        public NnChromosome ForwardTestNnChromosome { get; set; }
        public NnSettings NnSettings { get; set; }
    }
}

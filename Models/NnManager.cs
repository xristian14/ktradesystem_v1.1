﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    public class NnManager
    {
        public AForgeExtensions.Neuro.Learning.GeneticLearningNoTeacher GeneticLearningNoTeacher { get; set; }
        public NnChromosome[] NnPopulation { get; set; }
        public NnChromosome ForwardTestNnChromosome { get; set; }
        public NnSettings NnSettings { get; set; }
    }
}
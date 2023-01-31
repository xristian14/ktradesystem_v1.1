using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.ViewModels
{
    class StepsSettingsView : ViewModelBase
    {
        private string _generationsDuration;
        public string GenerationsDuration
        {
            get { return _generationsDuration; }
            set
            {
                if(int.TryParse(value, out int res))
                {
                    _generationsDuration = value;
                }
                OnPropertyChanged();
            }
        }
        private string _mutationProbability;
        public string MutationProbability
        {
            get { return _mutationProbability; }
            set
            {
                if (double.TryParse(value, out double res))
                {
                    _mutationProbability = value;
                }
                OnPropertyChanged();
            }
        }
        private string _fitnessScaleRate;
        public string FitnessScaleRate
        {
            get { return _fitnessScaleRate; }
            set
            {
                if (double.TryParse(value, out double res))
                {
                    _fitnessScaleRate = value;
                }
                OnPropertyChanged();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using ktradesystem.Models;

namespace ktradesystem.ViewModels
{
    class ViewModelPageTheeDimensionChart : ViewModelBase
    {
        public ViewModelPageTheeDimensionChart()
        {
            ViewModelPageTestingResult.TestBatchesUpdatePages += UpdatePage;
        }

        private ModelVisual3D _modelVisual3D;
        public ModelVisual3D ModelVisual3D //объект, содержащий группу геометрий
        {
            get { return _modelVisual3D; }
            private set
            {
                _modelVisual3D = value;
                OnPropertyChanged();
            }
        }

        public void UpdatePage() //обновляет страницу
        {
            if(ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox != null)
            {
                TestBatch testBatch = ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox.TestBatch;
                if(testBatch != null)
                {

                }
            }
        }
    }
}

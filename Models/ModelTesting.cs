using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ktradesystem.Models.Datatables;
using ktradesystem.CommunicationChannel;

namespace ktradesystem.Models
{
    class ModelTesting : ModelBase
    {
        private static ModelTesting _instance;

        public static ModelTesting getInstance()
        {
            if (_instance == null)
            {
                _instance = new ModelTesting();
            }
            return _instance;
        }

        private ModelTesting()
        {
            _modelData = ModelData.getInstance();
            _database = Database.getInstance();
            _mainCommunicationChannel = MainCommunicationChannel.getInstance();
        }

        private ModelData _modelData;
        private Database _database;
        private MainCommunicationChannel _mainCommunicationChannel;

        public CancellationTokenSource CancellationTokenTesting; //токен отмены тестирования
        public void CancellationTokenTestingCancel()
        {
            if (CancellationTokenTesting != null)
            {
                CancellationTokenTesting.Cancel();
            }
        }

        public void IndicatorInsertUpdate(string name, string description, List<IndicatorParameterTemplate> indicatorParameterTemplates, string script, int id = -1) //если прислан id, отправляет запрос update, иначе insert
        {
            if(id == -1)
            {
                //добавляет индикатор, и после шаблоны параметров для него
                _database.InsertIndicator(new Indicator { Name = name, Description = description, Script = script });
                _modelData.ReadIndicators();

                int newIndicatorId = _modelData.Indicators[_modelData.Indicators.Count - 1].Id;
                foreach(IndicatorParameterTemplate parameterTemplate in indicatorParameterTemplates)
                {
                    parameterTemplate.IdIndicator = newIndicatorId;
                    _database.InsertIndicatorParameterTemplate(parameterTemplate);
                }
            }
            else
            {
                //обновляет имеющиеся в БД шаблоны параметров данного индикатора в соответствии с IndicatorParameterTemplates, и обновляет индикатор
                List<IndicatorParameterTemplate> updateParTemp = new List<IndicatorParameterTemplate>(); //список с id параметра который нужно обновить и имя и описание на которые нужно обновить
                List<IndicatorParameterTemplate> addParTemp = new List<IndicatorParameterTemplate>(); //список с шаблонами параметров которые нужно добавить
                List<int> deleteParTemp = new List<int>(); //список с id которые нужно удалить

                Indicator oldIndicator = new Indicator();
                foreach(Indicator indicator in _modelData.Indicators)
                {
                    if(indicator.Id == id)
                    {
                        oldIndicator = indicator;
                    }
                }

                int length = indicatorParameterTemplates.Count;
                if(oldIndicator.IndicatorParameterTemplates.Count > length)
                {
                    length = oldIndicator.IndicatorParameterTemplates.Count;
                }
                //проходит по всем элементам которые есть в старом и новом списке шаблонов параметров, и определяет какие обновить, какие добавить, а какие удалить
                for(int i = 0; i < length; i++)
                {
                    if(indicatorParameterTemplates.Count > i && oldIndicator.IndicatorParameterTemplates.Count > i)
                    {
                        if(indicatorParameterTemplates[i].Name != oldIndicator.IndicatorParameterTemplates[i].Name || indicatorParameterTemplates[i].Description != oldIndicator.IndicatorParameterTemplates[i].Description || indicatorParameterTemplates[i].ParameterValueType != oldIndicator.IndicatorParameterTemplates[i].ParameterValueType)
                        {
                            indicatorParameterTemplates[i].Id = oldIndicator.IndicatorParameterTemplates[i].Id;
                            indicatorParameterTemplates[i].IdIndicator = oldIndicator.IndicatorParameterTemplates[i].IdIndicator;
                            updateParTemp.Add(indicatorParameterTemplates[i]);
                        }
                        
                    }
                    else if(indicatorParameterTemplates.Count > i && oldIndicator.IndicatorParameterTemplates.Count <= i)
                    {
                        indicatorParameterTemplates[i].IdIndicator = oldIndicator.Id;
                        addParTemp.Add(indicatorParameterTemplates[i]);
                    }
                    else if(indicatorParameterTemplates.Count <= i && oldIndicator.IndicatorParameterTemplates.Count > i)
                    {
                        deleteParTemp.Add(oldIndicator.IndicatorParameterTemplates[i].Id);
                    }
                }
                //обновляем шаблоны параметров
                foreach(IndicatorParameterTemplate item in updateParTemp)
                {
                    _database.UpdateIndicatorParameterTemplate(item);
                }
                //добавляем шаблоны параметров
                foreach(IndicatorParameterTemplate item in addParTemp)
                {
                    _database.InsertIndicatorParameterTemplate(item);
                }
                //удаляем шаблоны параметров
                foreach(int idDelete in deleteParTemp)
                {
                    _database.DeleteIndicatorParameterTemplate(idDelete);
                }

                //обновляем индикатор
                _database.UpdateIndicator(new Indicator { Id = id, Name = name, Description = description, Script = script });
            }

            _modelData.ReadIndicators();
        }

        public void IndicatorDelete(int id)
        {
            _database.DeleteIndicator(id);

            _modelData.ReadIndicators();
        }

        public void AlgorithmInsertUpdate(string name, string description, List<DataSourceTemplate> dataSourceTemplates, List<IndicatorParameterRange> indicatorParameterRanges, List<AlgorithmParameter> algorithmParameters, string script, int id = -1) //если прислан id, отправляет запрос update, иначе insert
        {
            if (id == -1)
            {
                //добавляет алгоритм, и после для него: макеты источников данных, диапазоны значений шаблонов параметров, и параметры алгоритма
                _database.InsertAlgorithm(new Algorithm { Name = name, Description = description, Script = script });
                _modelData.ReadAlgorithms();

                int newAlgorithmId = _modelData.Algorithms.Last().Id;

                foreach (DataSourceTemplate dataSourceTemplate in dataSourceTemplates)
                {
                    dataSourceTemplate.IdAlgorithm = newAlgorithmId;
                    _database.InsertDataSourceTemplate(dataSourceTemplate);
                }

                foreach (IndicatorParameterRange indicatorParameterRange in indicatorParameterRanges)
                {
                    indicatorParameterRange.IdAlgorithm = newAlgorithmId;
                    _database.InsertIndicatorParameterRange(indicatorParameterRange);
                }

                foreach (AlgorithmParameter algorithmParameter in algorithmParameters)
                {
                    algorithmParameter.IdAlgorithm = newAlgorithmId;
                    _database.InsertAlgorithmParameter(algorithmParameter);
                }
            }
            else
            {
                //находим обновляемый объект алгоритма по id
                Algorithm oldAlgorithm = new Algorithm();
                foreach(Algorithm algorithm in _modelData.Algorithms)
                {
                    if(algorithm.Id == id)
                    {
                        oldAlgorithm = algorithm;
                    }
                }


                //обновляем макеты источников данных
                List<DataSourceTemplate> updateDatSouTem = new List<DataSourceTemplate>(); //список с id записи которую нужно обновить, и новые данные для этой записи
                List<DataSourceTemplate> addDatSouTem = new List<DataSourceTemplate>(); //список с записями которые нужно добавить
                List<int> deleteDatSouTem = new List<int>(); //список с id записей которые нужно удалить

                int maxLengthDataSourceTemplate = dataSourceTemplates.Count;
                if(oldAlgorithm.DataSourceTemplates.Count > maxLengthDataSourceTemplate)
                {
                    maxLengthDataSourceTemplate = oldAlgorithm.DataSourceTemplates.Count;
                }
                //проходим по всем элементам старого и нового списка, и определяем какие обновить, какие добавить, а какие удалить
                for(int i = 0; i < maxLengthDataSourceTemplate; i++)
                {
                    if(dataSourceTemplates.Count > i && oldAlgorithm.DataSourceTemplates.Count > i)
                    {
                        if(dataSourceTemplates[i].Name != oldAlgorithm.DataSourceTemplates[i].Name || dataSourceTemplates[i].Description != oldAlgorithm.DataSourceTemplates[i].Description)
                        {
                            dataSourceTemplates[i].Id = oldAlgorithm.DataSourceTemplates[i].Id;
                            dataSourceTemplates[i].IdAlgorithm = oldAlgorithm.DataSourceTemplates[i].IdAlgorithm;
                            updateDatSouTem.Add(dataSourceTemplates[i]);
                        }
                    }
                    else if(dataSourceTemplates.Count > i && oldAlgorithm.DataSourceTemplates.Count <= i)
                    {
                        dataSourceTemplates[i].IdAlgorithm = oldAlgorithm.Id;
                        addDatSouTem.Add(dataSourceTemplates[i]);
                    }
                    else if(dataSourceTemplates.Count <= i && oldAlgorithm.DataSourceTemplates.Count > i)
                    {
                        deleteDatSouTem.Add(oldAlgorithm.DataSourceTemplates[i].Id);
                    }
                }
                //обновляем
                foreach (DataSourceTemplate dataSourceTemplate in updateDatSouTem)
                {
                    _database.UpdateDataSourceTemplate(dataSourceTemplate);
                }
                //добавляем
                foreach (DataSourceTemplate dataSourceTemplate1 in addDatSouTem)
                {
                    _database.InsertDataSourceTemplate(dataSourceTemplate1);
                }
                //удаляем
                foreach (int idDatSouTem in deleteDatSouTem)
                {
                    _database.DeleteDataSourceTemplate(idDatSouTem);
                }


                //обновляем диапазоны значений параметров индикаторов
                List<IndicatorParameterRange> updateIndParRan = new List<IndicatorParameterRange>(); //список с id записи которую нужно обновить, и новые данные для этой записи
                List<IndicatorParameterRange> addIndParRan = new List<IndicatorParameterRange>(); //список с записями которые нужно добавить
                List<int> deleteIndParRan = new List<int>(); //список с id записей которые нужно удалить

                int maxLengthIndicatorParameterRange = indicatorParameterRanges.Count;
                if (oldAlgorithm.IndicatorParameterRanges.Count > maxLengthIndicatorParameterRange)
                {
                    maxLengthIndicatorParameterRange = oldAlgorithm.IndicatorParameterRanges.Count;
                }
                //проходим по всем элементам старого и нового списка, и определяем какие обновить, какие добавить, а какие удалить
                for (int i = 0; i < maxLengthIndicatorParameterRange; i++)
                {
                    if (indicatorParameterRanges.Count > i && oldAlgorithm.IndicatorParameterRanges.Count > i)
                    {
                        if (indicatorParameterRanges[i].MinValue != oldAlgorithm.IndicatorParameterRanges[i].MinValue || indicatorParameterRanges[i].MaxValue != oldAlgorithm.IndicatorParameterRanges[i].MaxValue || indicatorParameterRanges[i].Step != oldAlgorithm.IndicatorParameterRanges[i].Step || indicatorParameterRanges[i].IsStepPercent != oldAlgorithm.IndicatorParameterRanges[i].IsStepPercent || indicatorParameterRanges[i].IndicatorParameterTemplate.Id != oldAlgorithm.IndicatorParameterRanges[i].IndicatorParameterTemplate.Id)
                        {
                            indicatorParameterRanges[i].Id = oldAlgorithm.IndicatorParameterRanges[i].Id;
                            indicatorParameterRanges[i].IdAlgorithm = oldAlgorithm.IndicatorParameterRanges[i].IdAlgorithm;
                            updateIndParRan.Add(indicatorParameterRanges[i]);
                        }
                    }
                    else if (indicatorParameterRanges.Count > i && oldAlgorithm.IndicatorParameterRanges.Count <= i)
                    {
                        indicatorParameterRanges[i].IdAlgorithm = oldAlgorithm.Id;
                        addIndParRan.Add(indicatorParameterRanges[i]);
                    }
                    else if (indicatorParameterRanges.Count <= i && oldAlgorithm.IndicatorParameterRanges.Count > i)
                    {
                        deleteIndParRan.Add(oldAlgorithm.IndicatorParameterRanges[i].Id);
                    }
                }
                //обновляем
                foreach (IndicatorParameterRange indicatorParameterRange in updateIndParRan)
                {
                    _database.UpdateIndicatorParameterRange(indicatorParameterRange);
                }
                //добавляем
                foreach (IndicatorParameterRange indicatorParameterRange1 in addIndParRan)
                {
                    _database.InsertIndicatorParameterRange(indicatorParameterRange1);
                }
                //удаляем
                foreach (int idIndParRan in deleteIndParRan)
                {
                    _database.DeleteIndicatorParameterRange(idIndParRan);
                }


                //обновляем параметры алгоритма
                List<AlgorithmParameter> updateAlgPar = new List<AlgorithmParameter>(); //список с id записи которую нужно обновить, и новые данные для этой записи
                List<AlgorithmParameter> addAlgPar = new List<AlgorithmParameter>(); //список с записями которые нужно добавить
                List<int> deleteAlgPar = new List<int>(); //список с id записей которые нужно удалить

                int maxLengthAlgorithmParameter = algorithmParameters.Count;
                if (oldAlgorithm.AlgorithmParameters.Count > maxLengthAlgorithmParameter)
                {
                    maxLengthAlgorithmParameter = oldAlgorithm.AlgorithmParameters.Count;
                }
                //проходим по всем элементам старого и нового списка, и определяем какие обновить, какие добавить, а какие удалить
                for (int i = 0; i < maxLengthAlgorithmParameter; i++)
                {
                    if (algorithmParameters.Count > i && oldAlgorithm.AlgorithmParameters.Count > i)
                    {
                        if (algorithmParameters[i].Name != oldAlgorithm.AlgorithmParameters[i].Name || algorithmParameters[i].Description != oldAlgorithm.AlgorithmParameters[i].Description || algorithmParameters[i].ParameterValueType != oldAlgorithm.AlgorithmParameters[i].ParameterValueType || algorithmParameters[i].MinValue != oldAlgorithm.AlgorithmParameters[i].MinValue || algorithmParameters[i].MaxValue != oldAlgorithm.AlgorithmParameters[i].MaxValue || algorithmParameters[i].Step != oldAlgorithm.AlgorithmParameters[i].Step || algorithmParameters[i].IsStepPercent != oldAlgorithm.AlgorithmParameters[i].IsStepPercent)
                        {
                            algorithmParameters[i].Id = oldAlgorithm.AlgorithmParameters[i].Id;
                            algorithmParameters[i].IdAlgorithm = oldAlgorithm.AlgorithmParameters[i].IdAlgorithm;
                            updateAlgPar.Add(algorithmParameters[i]);
                        }
                    }
                    else if (algorithmParameters.Count > i && oldAlgorithm.AlgorithmParameters.Count <= i)
                    {
                        algorithmParameters[i].IdAlgorithm = oldAlgorithm.Id;
                        addAlgPar.Add(algorithmParameters[i]);
                    }
                    else if (algorithmParameters.Count <= i && oldAlgorithm.AlgorithmParameters.Count > i)
                    {
                        deleteAlgPar.Add(oldAlgorithm.AlgorithmParameters[i].Id);
                    }
                }
                //обновляем
                foreach (AlgorithmParameter algorithmParameter in updateAlgPar)
                {
                    _database.UpdateAlgorithmParameter(algorithmParameter);
                }
                //добавляем
                foreach (AlgorithmParameter algorithmParameter1 in addAlgPar)
                {
                    _database.InsertAlgorithmParameter(algorithmParameter1);
                }
                //удаляем
                foreach (int idAlgPar in deleteAlgPar)
                {
                    _database.DeleteAlgorithmParameter(idAlgPar);
                }


                //обновляем алгоритм
                _database.UpdateAlgorithm(new Algorithm { Id = id, Name = name, Description = description, Script = script });
            }

            _modelData.ReadAlgorithms();
        }

        public void AlgorithmDelete(int id)
        {
            _database.DeleteAlgorithm(id);

            _modelData.ReadAlgorithms();
        }

        public void TestingLaunch(Testing testing)
        {
            CancellationTokenTesting = new CancellationTokenSource();
            DispatcherInvoke((Action)(() => { _mainCommunicationChannel.TestingProgress.Clear(); }));
            testing.LaunchTesting();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

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
        }

        ModelData _modelData;
        Database _database;

        public void IndicatorInsertUpdate(string name, string description, List<ParameterTemplate> parameterTemplates, string script, int id = -1) //если прислан id, отправляет запрос update, иначе insert
        {
            if(id == -1)
            {
                //добавляет индикатор, и после шаблоны параметров для него
                _database.InsertIndicator(new Indicator { Name = name, Description = description, Script = script });
                _modelData.ReadIndicators();

                int newIndicatorId = _modelData.Indicators[_modelData.Indicators.Count - 1].Id;
                foreach(ParameterTemplate parameterTemplate in parameterTemplates)
                {
                    parameterTemplate.IdIndicator = newIndicatorId;
                    _database.InsertParameterTemplate(parameterTemplate);
                }
            }
            else
            {
                //обновляет имеющиеся в БД шаблоны параметров данного индикатора в соответствии с parameterTemplates, и обновляет индикатор
                List<ParameterTemplate> updateParTemp = new List<ParameterTemplate>(); //список с id параметра который нужно обновить и имя и описание на которые нужно обновить
                List<ParameterTemplate> addParTemp = new List<ParameterTemplate>(); //список с шаблонами параметров которые нужно добавить
                List<int> deleteParTemp = new List<int>(); //список с id которые нужно удалить

                Indicator oldIndicator = new Indicator();
                foreach(Indicator indicator in _modelData.Indicators)
                {
                    if(indicator.Id == id)
                    {
                        oldIndicator = indicator;
                    }
                }

                int length = parameterTemplates.Count;
                if(oldIndicator.ParameterTemplates.Count > length)
                {
                    length = oldIndicator.ParameterTemplates.Count;
                }
                //проходит по всем элементам которые есть в старом и новом списке шаблонов параметров, и определяет какие обновить, какие добавить, а какие удалить
                for(int i = 0; i < length; i++)
                {
                    if(parameterTemplates.Count > i && oldIndicator.ParameterTemplates.Count > i)
                    {
                        if(parameterTemplates[i].Name != oldIndicator.ParameterTemplates[i].Name || parameterTemplates[i].Description != oldIndicator.ParameterTemplates[i].Description)
                        {
                            updateParTemp.Add(new ParameterTemplate { Id = oldIndicator.ParameterTemplates[i].Id, Name = parameterTemplates[i].Name, Description = parameterTemplates[i].Description, IdIndicator = oldIndicator.ParameterTemplates[i].IdIndicator });
                        }
                        
                    }
                    else if(parameterTemplates.Count > i && oldIndicator.ParameterTemplates.Count <= i)
                    {
                        parameterTemplates[i].IdIndicator = oldIndicator.Id;
                        addParTemp.Add(parameterTemplates[i]);
                    }
                    else if(parameterTemplates.Count <= i && oldIndicator.ParameterTemplates.Count > i)
                    {
                        deleteParTemp.Add(oldIndicator.ParameterTemplates[i].Id);
                    }
                }
                //обновляем шаблоны параметров
                foreach(ParameterTemplate item in updateParTemp)
                {
                    _database.UpdateParameterTemplate(item);
                }
                //добавляем шаблоны параметров
                foreach(ParameterTemplate item in addParTemp)
                {
                    _database.InsertParameterTemplate(item);
                }
                //удаляем шаблоны параметров
                foreach(int idDelete in deleteParTemp)
                {
                    _database.DeleteParameterTemplate(idDelete);
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
    }
}

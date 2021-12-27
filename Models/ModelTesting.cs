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

        public void IndicatorInsertUpdate(string name, string description, List<IndicatorParameterTemplate> IndicatorParameterTemplates, string script, int id = -1) //если прислан id, отправляет запрос update, иначе insert
        {
            if(id == -1)
            {
                //добавляет индикатор, и после шаблоны параметров для него
                _database.InsertIndicator(new Indicator { Name = name, Description = description, Script = script });
                _modelData.ReadIndicators();

                int newIndicatorId = _modelData.Indicators[_modelData.Indicators.Count - 1].Id;
                foreach(IndicatorParameterTemplate parameterTemplate in IndicatorParameterTemplates)
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

                int length = IndicatorParameterTemplates.Count;
                if(oldIndicator.IndicatorParameterTemplates.Count > length)
                {
                    length = oldIndicator.IndicatorParameterTemplates.Count;
                }
                //проходит по всем элементам которые есть в старом и новом списке шаблонов параметров, и определяет какие обновить, какие добавить, а какие удалить
                for(int i = 0; i < length; i++)
                {
                    if(IndicatorParameterTemplates.Count > i && oldIndicator.IndicatorParameterTemplates.Count > i)
                    {
                        if(IndicatorParameterTemplates[i].Name != oldIndicator.IndicatorParameterTemplates[i].Name || IndicatorParameterTemplates[i].Description != oldIndicator.IndicatorParameterTemplates[i].Description)
                        {
                            updateParTemp.Add(new IndicatorParameterTemplate { Id = oldIndicator.IndicatorParameterTemplates[i].Id, Name = IndicatorParameterTemplates[i].Name, Description = IndicatorParameterTemplates[i].Description, IdIndicator = oldIndicator.IndicatorParameterTemplates[i].IdIndicator });
                        }
                        
                    }
                    else if(IndicatorParameterTemplates.Count > i && oldIndicator.IndicatorParameterTemplates.Count <= i)
                    {
                        IndicatorParameterTemplates[i].IdIndicator = oldIndicator.Id;
                        addParTemp.Add(IndicatorParameterTemplates[i]);
                    }
                    else if(IndicatorParameterTemplates.Count <= i && oldIndicator.IndicatorParameterTemplates.Count > i)
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
    }
}

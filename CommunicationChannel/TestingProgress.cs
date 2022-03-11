using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models;

namespace ktradesystem.CommunicationChannel
{
    class TestingProgress
    {
        public string StepDescription { get; set; } //шаг выполнения тестирования (1/3 считывание источников данных, 2/3 симуляция тестирования, 3/3 запись результатов)
        public int StepTasksCount { get; set; } //количество задач текущего шага выполнения тестирования, подлежащих выполнению
        public int CompletedStepTasksCount { get; set; } //количество выполненных задач текущего шага выполнения тестирования
        public TimeSpan StepElapsedTime { get; set; } //затраченное время на выполненние задач текущего шага выполнения тестирования
        public TimeSpan TotalElapsedTime { get; set; } //общее затраченное время на выполнение тестирования
        public bool CancelPossibility { get; set; } //возможность отмены операции
        public bool IsFinish { get; set; } //завершен процесс или нет
        public bool IsSuccess { get; set; } //успешно ли завершено тестирование. Нужно ли переходить на запись результатов
        public Testing Testing { get; set; } //выполненное тестирование
    }
}

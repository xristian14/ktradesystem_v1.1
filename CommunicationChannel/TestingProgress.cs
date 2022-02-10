using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.CommunicationChannel
{
    class TestingProgress
    {
        public string Header { get; set; } //заголовок
        public int TasksCount { get; set; } //количество задач, подлежащих выполнению
        public int CompletedTasksCount { get; set; } //количество выполненных задач
        public TimeSpan ElapsedTime { get; set; } //общее затраченное время на выполненные задачи
        public bool CancelPossibility { get; set; } //возможность отмены операции
        public bool IsFinish { get; set; } //завершен процесс или нет
        public bool IsSuccess { get; set; } //успешно ли завершено тестирование. Нужно ли переходить на отображение результатов
    }
}

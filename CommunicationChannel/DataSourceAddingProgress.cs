using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.CommunicationChannel
{
    class DataSourceAddingProgress
    {
        public int TasksCount { get; set; } //количество задач, подлежащих выполнению
        public int CompletedTasksCount { get; set; } //количество выполненных задач
        public TimeSpan ElapsedTime { get; set; } //общее затраченное время на выполненные задачи
        public bool IsFinish { get; set; } //завершен процесс или нет
    }
}

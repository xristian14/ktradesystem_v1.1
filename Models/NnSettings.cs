using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    public class NnSettings
    {
        public DateTimeDuration LearningPeriodOffset { get; set; }
        public DateTimeDuration LearningPeriodCount { get; set; }
        public DateTimeDuration LearningPeriodDuration { get; set; }
        public DateTimeDuration LearningPeriodDistance { get; set; }
        public bool IsForwardTesting { get; set; }
        public DateTimeDuration ForwardPeriodOffset { get; set; }
        public DateTimeDuration ForwardPeriodCount { get; set; }
        public DateTimeDuration ForwardPeriodDuration { get; set; }
        public DateTimeDuration ForwardPeriodDistance { get; set; }
    }
}

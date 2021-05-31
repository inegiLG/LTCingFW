using LTCingFW.beans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.thread
{
    public interface ITimer
    {
        String TimerName { get; set; }
        StateLight ThreadLight { get; set; } 
        System.Timers.Timer ThisTimer { get; set; }
        void Execute(object sender, EventArgs e);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW.beans
{

    public class StateLight {

        public LightStateEnum State { get; set; } = LightStateEnum.STOP;

        public string ErrMsg { get; set; }

        public enum LightStateEnum
        {
            NORMAL = 0,
            WARN = 1,
            ERROR = 2,
            STOP = 3
        }

    }


   
}

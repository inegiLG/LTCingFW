using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTCingFW
{
    public enum ValidateEnum
    {
        //非空
        NOT_NULL = 1,
        //最大长度
        MAX_LENGTH = 2,
        //最小长度
        MIN_LENGTH = 4,
        //日期
        IS_DATE = 8,
        //数字
        IS_NUMBER = 16,

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZuggerWpf
{
   public static class CustomEnum
    {
        /// <summary>
        /// 严重程度
        /// </summary>
        public  enum customSeverity
        {
            致命=1,
            严重=2,
            中等=3,
            轻微=4
        }

        public  enum CustomPri
        {
            紧急 = 1,
            高 = 2,
            中 = 3,
            低 = 4
        }
    }
}

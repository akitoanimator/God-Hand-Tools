using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRTool
{
    class Answer
    {
        public static void analyseAnswer(string ans)
        {
            if (ans.Contains("e")) ExtractModel.GUI();
            if (ans.Contains("c")) ConvertModel.GUI();
        }
    }
}

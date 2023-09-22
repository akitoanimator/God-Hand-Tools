using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Answer
{
    public static void analyseAnswer(string ans)
    {
        if (ans.Contains("1")) ExtractModel.GUI();
        if (ans.Contains("2")) ConvertModel.GUI();
    }
}

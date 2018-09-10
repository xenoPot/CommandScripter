using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CommandScripter;
using Scenario.Command;

namespace Scenario.Linter
{
    /// <summary>
    /// logコマンドのLinter
    /// </summary>
    public class Log : CommandScripter.Linter
    {

        public Log()
        {
            SetParamKeys(Command.Log.KEY_TEXT);
        }

        public override LinkedList<ErrorFormat> CommandParamLinting(Dictionary<string, string> param)
        {
            var errList = new LinkedList<ErrorFormat>();
            LinterUtility.ExistRequiredParam(errList, param, Command.Log.KEY_TEXT);
            return errList;
        }
    }
}

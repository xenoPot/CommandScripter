using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CommandScripter;
using Scenario.Command;

namespace Scenario.Linter
{
    /// <summary>
    /// WaitコマンドのLinter
    /// </summary>
    public class Wait : CommandScripter.Linter
    {

        public Wait()
        {
            SetParamKeys(Command.Wait.KEY_TIME);
        }

        public override LinkedList<ErrorFormat> CommandParamLinting(Dictionary<string, string> param)
        {
            var errList = new LinkedList<ErrorFormat>();
            LinterUtility.ExistRequiredParam(errList, param, Command.Wait.KEY_TIME);
            LinterUtility.TypeCheckFloat(errList, param, Command.Wait.KEY_TIME);
            return errList;
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Scenario.Command;
using CommandScripter;

namespace Scenario.Linter
{
    /// <summary>
    /// MsgコマンドのLinter
    /// </summary>
    public class Msg : CommandScripter.Linter
    {
        public Msg()
        {
            SetParamKeys(Command.Msg.KEY_TEXT);
        }

        public override LinkedList<ErrorFormat> CommandParamLinting(Dictionary<string, string> param)
        {
            return null;
        }
    }
}

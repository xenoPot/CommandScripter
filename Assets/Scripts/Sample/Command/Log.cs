using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CommandScripter;

namespace Scenario.Command
{
    /// <summary>
    /// @log text=
    /// text : string : コンソールログに表示するテキスト
    /// 
    /// 指定したテキストをコンソールログに表示する
    /// </summary>
    public class Log : CommandScripter.Command
    {
        public override void CommandStart(CommandInfo commandInfo)
        {
            Debug.Log(commandInfo.commandParams.GetValue(KEY_TEXT));
            this.Finish();
        }

        public override void CommandUpdate()
        {
        }

        #region コマンド引数
        public const string KEY_TEXT = "text";
        #endregion
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CommandScripter;

namespace Scenario.Command
{
    /// <summary>
    /// @wait time=
    /// time : float : 秒（小数可）
    /// 
    /// スクリプトの進行をtime秒待機させます
    /// </summary>
    public class Wait : CommandScripter.Command
    {
        public override void CommandStart(CommandInfo commandInfo)
        {
            StartCoroutine(TimeProcess(float.Parse(commandInfo.commandParams.GetValue(KEY_TIME))));
        }

        public override void CommandUpdate()
        {

        }

        private IEnumerator TimeProcess(float time)
        {
            yield return new WaitForSeconds(time);
            this.Finish();
        }

        #region コマンド引数
        public const string KEY_TIME = "time";
        #endregion
    }
}

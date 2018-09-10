using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CommandScripter;

namespace Scenario.Command
{
    /// <summary>
    /// メッセージ表示用コマンド
    /// コマンド記号（@）を打たずに文章のみ打った場合に、このコマンドのtextパラメータとして変換される
    /// 
    /// text : 文字列 : このコマンドで表示するメッセージデータ
    /// </summary>
    public class Msg : CommandScripter.Command
    {
        public override void CommandStart(CommandInfo commandInfo)
        {
            Debug.Log(commandInfo.commandParams.GetValue(KEY_TEXT));
        }

        public override void CommandUpdate()
        {
            // 左クリックでコマンド終了
            if (Input.GetMouseButtonDown(0))
            {
                this.Finish();
            }
        }

        #region コマンド引数
        public const string KEY_TEXT = "text";
        #endregion
    }
}

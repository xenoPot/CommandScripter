
using UnityEngine;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CommandScripter
{
    /// <summary>
    /// コマンドの抽象クラス
    /// </summary>
    public abstract class Command : MonoBehaviour
    {

        /// <summary>
        /// コマンド実行開始
        /// </summary>
        /// <param name="param">コマンドに渡された引数群</param>
        abstract public void CommandStart(CommandInfo commandInfo);

        /// <summary>
        /// コマンド更新処理
        /// </summary>
        abstract public void CommandUpdate();

        /// <summary>
        /// コマンド実行前の設定処理
        /// </summary>
        /// <param name="controller">コマンドコントローラ</param>
        /// <param name="callback">コマンド実行完了時コールバック</param>
        virtual public void Setup(CommandController controller, System.Action callback)
        {
            _commandController = controller;
            _callback = callback;
        }

        protected CommandController _commandController;
        protected System.Action _callback;

        /// <summary>
        /// コマンドの終了通知
        /// </summary>
        protected void Finish()
        {
            _callback();
            _callback = null;
        }
    }
}

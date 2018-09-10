using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CommandScripter
{
    /// <summary>
    /// コマンドの構文チェッカ用抽象クラス
    /// </summary>
    public abstract class Linter
    {

        /// <summary>
        /// 引数キーリストを返す
        /// </summary>
        public ReadOnlyCollection<string> KeyList
        {
            get
            {
                return _keyList;
            }
        }

        /// <summary>
        /// コマンドの引数キーリストを設定する
        /// </summary>
        /// <param name="keys">引数キーリスト</param>
        protected void SetParamKeys(params string[] keys)
        {
            _keyList = new ReadOnlyCollection<string>(keys);
        }

        /// <summary>
        /// コマンドパラメータの構文チェックを行う
        /// </summary>
        /// <returns>エラーリスト</returns>
        abstract public LinkedList<ErrorFormat> CommandParamLinting(Dictionary<string, string> param);

        /// <summary>
        /// パース中の一時パラメータの設定、使用を行う
        /// </summary>
        /// <param name="tmpController">一時パラメータ制御用インターフェース</param>
        /// <param name="param">コマンドパラメータ</param>
        /// <returns>エラーリスト</returns>
        virtual public LinkedList<ErrorFormat> ParamControlStep(IChunkParamControl paramController, Dictionary<string, string> param)
        {
            return null;
        }

        /// <summary>
        /// コマンドの引数キーリスト
        /// </summary>
        private ReadOnlyCollection<string> _keyList;
    }
}

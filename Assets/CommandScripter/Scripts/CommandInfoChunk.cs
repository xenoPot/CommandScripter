using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Serialize;

namespace CommandScripter
{
    /// <summary>
    /// コマンドのパラメータ構造体
    /// </summary>
    [System.Serializable]
    public class OptionParamPair : KeyAndValue<string, ParamDictionary> { }
    #region コマンドパラメータ保存用の定義
    /// <summary>
    /// チャンク全体に関わるオプションを持たせるための定義
    /// </summary>
    [System.Serializable]
    public class OptionParamDictionary : TableBase<string, ParamDictionary, OptionParamPair> { }

    #endregion
    /// <summary>
    /// スクリプトをパースした結果生成された実行用のコマンド情報群
    /// </summary>
    [System.Serializable]
    public class CommandInfoChunk : ScriptableObject
    {

        public void Setup(string scriptName, List<CommandInfo> commandInfos)
        {
            _scriptName = scriptName;
            _commandInfos = commandInfos;
        }

        public void Setup(CommandInfoChunk src)
        {
            _scriptName = src._scriptName;
            _commandInfos = src._commandInfos;
            _optionDict = src._optionDict;
        }

        /// <summary>
        /// チャンクの生成元になったスクリプト名
        /// </summary>
        public string ScriptName
        {
            get { return _scriptName; }
        }

        /// <summary>
        /// オプションパラメータのディクショナリ
        /// </summary>
        public OptionParamDictionary OptionDict
        {
            get
            {
                return _optionDict;
            }
            set
            {
                _optionDict = value;
            }
        }

        /// <summary>
        /// 指定したインデックスのコマンド情報を取得
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>コマンド情報</returns>
        public CommandInfo GetCommandInfo(int index)
        {
            if (index < 0)
            {
                return null;
            }
            if (index >= _commandInfos.Count)
            {
                return null;
            }

            return _commandInfos[index];
        }

        [SerializeField]
        private string _scriptName;
        /// <summary>
        /// 実行用のコマンド情報リスト
        /// </summary>
        [SerializeField]
        private List<CommandInfo> _commandInfos;

        /// <summary>
        /// スクリプト内のオプションパラメータリスト
        /// </summary>
        [SerializeField]
        private OptionParamDictionary _optionDict;

    }
}

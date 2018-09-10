using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Serialize;

namespace CommandScripter
{
    #region コマンドパラメータ保存用の定義
    /// <summary>
    /// コマンドのパラメータをDictionaryの形で保存するためのもの
    /// </summary>
    [System.Serializable]
    public class ParamDictionary : TableBase<string, string, ParamPair> { }

    /// <summary>
    /// コマンドのパラメータ構造体
    /// </summary>
    [System.Serializable]
    public class ParamPair : KeyAndValue<string, string> { }
    #endregion

    [System.Serializable]
    public class CommandInfo
    {
        /// <summary>行番号</summary>
        public int lineNumber;
        public string commandName;
        /// <summary>コマンドのパラメータ群</summary>
        public ParamDictionary commandParams;
    }
}

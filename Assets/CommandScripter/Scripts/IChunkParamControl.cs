using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CommandScripter
{
    /// <summary>
    /// スクリプトパース中のチャンクパラメータ操作用インターフェース
    /// </summary>
    public interface IChunkParamControl
    {

        #region パース状態取得
        /// <summary>
        /// 現在のコマンドインデックス
        /// 主にジャンプ先の定義に使用される想定
        /// </summary>
        int NowCommandIndex
        {
            get;
        }
        #endregion

        #region Temp操作
        /// <summary>
        /// 指定タグのStackにパラメータをpushする
        /// </summary>
        /// <param name="tag">stack特定タグ</param>
        /// <param name="key">パラメータkey</param>
        /// <param name="value">パラメータvalue</param>
        void PushTempParamForStack(string tag, string key, string value);

        /// <summary>
        /// 指定タグのStackからパラメータをpopする
        /// </summary>
        /// <param name="tag">stack特定タグ</param>
        /// <returns>パラメータ</returns>
        KeyValuePair<string, string> PopTempParamByStack(string tag);

        /// <summary>
        /// 指定タグのDictionaryにパラメータをAddする
        /// </summary>
        /// <param name="tag">dictionary特定用タグ</param>
        /// <param name="key">パラメータkey</param>
        /// <param name="value">パラメータvalue</param>
        void AddTempParamForDict(string tag, string key, string value);

        /// <summary>
        /// 指定タグのDictionaryからパラメータをGetする
        /// </summary>
        /// <param name="tag">dictionary特定用タグ</param>
        /// <param name="key">パラメータkey</param>
        /// <returns>パラメータ</returns>
        string GetTempParamForDict(string tag, string key);
        #endregion

        #region Option操作
        /// <summary>
        /// 指定タグのDictionaryにパラメータをAddする
        /// </summary>
        /// <param name="tag">dictionary特定用タグ</param>
        /// <param name="key">パラメータkey</param>
        /// <param name="value">パラメータvalue</param>
        void AddOptionParamForDict(string tag, string key, string value);

        /// <summary>
        /// 指定タグのDictionaryからパラメータをGetする
        /// </summary>
        /// <param name="tag">dictionary特定用タグ</param>
        /// <param name="key">パラメータkey</param>
        /// <returns>パラメータ</returns>
        string GetOptionParamByDict(string tag, string key);
        #endregion
    }
}

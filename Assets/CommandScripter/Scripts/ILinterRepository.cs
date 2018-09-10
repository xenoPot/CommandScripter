using UnityEngine;
using System.Collections;

namespace CommandScripter
{
    /// <summary>
    /// Linterリポジトリインターフェース
    /// </summary>
    public interface ILinterRepository
    {
        /// <summary>
        /// 指定名のコマンドが存在するかどうかを返す
        /// </summary>
        /// <param name="commandName">コマンド名</param>
        /// <returns>存在すればtrue</returns>
        bool ExistCommandName(string commandName);
        /// <summary>
        /// 指定名のコマンドと紐付いているLinterを返す
        /// </summary>
        /// <param name="commandName">コマンド名</param>
        /// <returns>linter</returns>
        Linter GetLinter(string commandName);
    }
}

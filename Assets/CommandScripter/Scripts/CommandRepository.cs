
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CommandScripter
{
    /// <summary>
    /// コマンドリポジトリ基底クラス
    /// </summary>
    public abstract class CommandRepository : MonoBehaviour, ICommandRepository
    {
        /// <summary>
        /// 指定名に該当するコマンドオブジェクトを返す
        /// </summary>
        /// <param name="commandName">Addコマンドで登録したコマンド名</param>
        /// <returns>コマンド</returns>
        public Command GetCommand(string commandName)
        {
            if (!_commands.ContainsKey(commandName))
            {
                // コマンドが存在しない
                return null;
            }
            return _commands[commandName];
        }

        /// <summary>
        /// Linterなしのコマンド登録
        /// </summary>
        /// <typeparam name="Command">登録コマンドの型</typeparam>
        /// <param name="commandName">コマンド名</param>
        protected void AddCommand<Command>(string commandName) where Command : CommandScripter.Command
        {
            var cmd = this.gameObject.AddComponent<Command>();
            _commands.Add(commandName, cmd);
        }

        /// <summary>
        /// システム内定義のコマンド一覧
        /// </summary>
        private Dictionary<string, Command> _commands = new Dictionary<string, Command>();
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CommandScripter
{
    /// <summary>
    /// Linterリポジトリ基底クラス
    /// </summary>
    public abstract class LinterRepository : ILinterRepository
    {
        #region ICommandLinterFactory メンバー

        public bool ExistCommandName(string commandName)
        {
            return _linters.ContainsKey(commandName);
        }

        public Linter GetLinter(string commandName)
        {
            if (_linters.ContainsKey(commandName))
            {
                return _linters[commandName];
            }
            return null;
        }

        #endregion

        /// <summary>
        /// Linterの追加・登録を行う
        /// </summary>
        /// <param name="commandName">コマンド名</param>
        /// <typeparam name="Linter">コマンドに対応させるlinterの型</typeparam>
        protected void AddLinter<Linter>(string commandName)
            where Linter : CommandScripter.Linter
        {
            var linter = System.Activator.CreateInstance<Linter>();
            _linters.Add(commandName, linter);
        }

        /// <summary>
        /// 実際にはLinterを登録せずコマンド名のみを登録する
        /// コマンドは作ったけどLinterはまだだったり、そもそもLinterの必要なコマンドではない場合
        /// （引数なしのコマンドなど）の場合に使用し、コマンドリスト未存在の例外を回避する
        /// </summary>
        /// <param name="commandName">登録するコマンド名</param>
        protected void AddLinter(string commandName)
        {
            _linters.Add(commandName, null);
        }

        #region private
        private Dictionary<string, Linter> _linters = new Dictionary<string, Linter>();
        #endregion
    }
}

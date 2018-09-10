
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Serialize;

namespace CommandScripter
{
    /// <summary>
    /// コマンドチャンクの実行を行うコントローラのリファレンス実装
    /// </summary>
    public class CommandController : MonoBehaviour
    {

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="repository">コマンドリポジトリのインターフェース</param>
        public void Initialize(ICommandRepository repository)
        {
            _commandRepository = repository;
            _nextPhase = null;
            _nowPhase = this.PNone;
        }

        /// <summary>
        /// チャンクの実行を行う
        /// </summary>
        /// <param name="command"></param>
        public void Execute(CommandInfoChunk chunk, System.Action endCallback = null)
        {
            if (chunk == null)
            {
                Debug.LogError("chunk is null");
                return;
            }
            _nowHandlingChunk = chunk;
            _nextPhase = PStartExecute;
            _endCallback = endCallback;
            _isCommandEnd = false;
            _nowExecutingCommand = null;
            _nextExecutingCommandIndex = 0;
        }

        /// <summary>
        /// チャンクの実行を停止する
        /// </summary>
        public void Stop()
        {
            Debug.Log("Stopped execute chunk");
            // チャンクの参照を解除
            _nowHandlingChunk = null;
            _nextPhase = PExecuteEnd;
        }

        /// <summary>
        /// 現在実行しているコマンドのインデックス
        /// </summary>
        public int NowExecutingCommandIndex
        {
            get
            {
                return _nowExecutingCommandIndex;
            }
        }

        /// <summary>
        /// 次に実行されるコマンドのインデックス
        /// </summary>
        public int NextCommandIndex
        {
            get
            {
                return _nextExecutingCommandIndex;
            }
            set
            {
                _nextExecutingCommandIndex = value;
            }
        }

        /// <summary>
        /// スクリプト実行中かどうかを返す
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                return _nowPhase != PNone || _nextPhase != null;
            }
        }

        /// <summary>
        /// 現在実行中のチャンクからオプションデータを取得する
        /// </summary>
        /// <param name="categoryTag">オプションカテゴリタグ</param>
        /// <param name="key">パラメータのKey</param>
        public string GetOptionParam(string categoryTag, string key)
        {
            return _nowHandlingChunk.OptionDict.GetValue(categoryTag).GetValue(key);
        }

        /// <summary>
        /// 指定した条件でオプションデータを取得できるかを返す
        /// </summary>
        /// <param name="categoryTag">オプションカテゴリタグ</param>
        /// <param name="key">パラメータのKey</param>
        /// <returns>取得できる場合はtrue</returns>
        public bool ExistOptionParam(string categoryTag, string key)
        {
            if (!_nowHandlingChunk.OptionDict.GetTable().ContainsKey(tag))
            {
                return false;
            }
            return _nowHandlingChunk.OptionDict.GetValue(categoryTag).GetTable().ContainsKey(key);
        }

        /// <summary>
        /// 何もしないフェーズ
        /// </summary>
        private void PNone()
        {

        }

        /// <summary>
        /// チャンク実行開始時フェーズ
        /// </summary>
        private void PStartExecute()
        {
            ExecuteCommand();
            if (_nowExecutingCommand == null)
            {
                Debug.Log("Script execute finished");
                _nextPhase = PExecuteEnd;
                return;
            }
        }

        /// <summary>
        /// コマンド実行中フェーズ
        /// </summary>
        private void PCommandExecuting()
        {
            _nowExecutingCommand.CommandUpdate();
            if (_isCommandEnd)
            {
                // コマンド実行完了
                ExecuteCommand();
                if (_nowExecutingCommand == null)
                {
                    Debug.Log("Script execute finished");
                    // チャンクの参照を解除
                    _nowHandlingChunk = null;
                    _nextPhase = PExecuteEnd;
                }
            }
        }

        /// <summary>
        /// 全コマンド終了時フェーズ
        /// </summary>
        private void PExecuteEnd()
        {
            // コールバック
            if (_endCallback != null)
            {
                _endCallback();
                _endCallback = null;
            }
            // 終わったら無動作フェーズへ戻る
            _nextPhase = PNone;
        }

        private void Update()
        {
            if (_nextPhase != null)
            {
                _nowPhase = _nextPhase;
                _nextPhase = null;
            }
            _nowPhase();
        }

        /// <summary>
        /// コマンドの実行を行う
        /// 即時完了のコマンドが続く時はこの関数内で連続実行する
        /// </summary>
        private void ExecuteCommand()
        {
            while (ExecuteNextCommand())
            {
                if (!_isCommandEnd)
                {
                    _nextPhase = PCommandExecuting;
                    break;
                }
            }
        }

        /// <summary>
        /// 次コマンドの実行を行う
        /// </summary>
        /// <param name="commandInfo">コマンド情報</param>
        /// <returns>false = 実行コマンド無し</returns>
        private bool ExecuteNextCommand()
        {
            var commandInfo = _nowHandlingChunk.GetCommandInfo(_nextExecutingCommandIndex);
            if (commandInfo == null)
            {
                // すべてのコマンドを実行し終えた
                _nowExecutingCommand = null;
                return false;
            }
            _nowExecutingCommandIndex = _nextExecutingCommandIndex;
            ++_nextExecutingCommandIndex;
            var command = _commandRepository.GetCommand(commandInfo.commandName);
            _nowExecutingCommand = command;
            _isCommandEnd = false;
            command.Setup(this, () =>
            {
                _isCommandEnd = true;
            });
            // このタイミングで終了する（コールバックが呼ばれる）コマンドもある
            command.CommandStart(commandInfo);

            return true;
        }

        /// <summary>
        /// コマンドオブジェクト生成者
        /// </summary>
        private ICommandRepository _commandRepository;

        /// <summary>
        /// 現在実行中のコマンド
        /// </summary>
        private Command _nowExecutingCommand;

        /// <summary>
        /// コマンド終了したかどうか
        /// </summary>
        private bool _isCommandEnd;

        /// <summary>
        /// 現在実行中のコマンドチャンク
        /// </summary>
        private CommandInfoChunk _nowHandlingChunk;

        /// <summary>
        /// 現在実行しているコマンドのリスト内インデックス
        /// </summary>
        private int _nowExecutingCommandIndex;

        /// <summary>
        /// 次に実行するコマンドのリスト内インデックス
        /// 次の実行コマンドを選ぶ際に使う
        /// </summary>
        private int _nextExecutingCommandIndex;

        private System.Action _nowPhase;
        private System.Action _nextPhase;

        /// <summary>
        /// 全コマンド終了時のコールバック
        /// </summary>
        private System.Action _endCallback;

    }
}

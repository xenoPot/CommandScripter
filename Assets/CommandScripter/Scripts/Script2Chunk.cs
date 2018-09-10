using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Serialize;

namespace CommandScripter
{
    /// <summary>
    /// コマンドスクリプトパーサ
    /// チャンクオブジェクトの出力
    /// 
    /// TODO: Msgコマンドがある前提の箇所を修正し、Msgコマンドがなくても問題が無いような形にもできるようにする
    /// </summary>
    public class Script2Chunk : IChunkParamControl
    {
        public void SetLinterRepository(ILinterRepository linterRepository)
        {
            _linterRepository = linterRepository;
        }

        /// <summary>
        /// スクリプトファイル全体のパースを行い、コマンドチャンクオブジェクトを生成して返す
        /// </summary>
        /// <param name="scriptName">スクリプト名</param>
        /// <param name="scriptText">スクリプト本文</param>
        /// <returns>パース結果のコマンドチャンク</returns>
        public CommandInfoChunk ParseScript(string scriptName, string scriptText)
        {
            _tempParamStacks.Clear();
            _tempParamDicts.Clear();
            _optionParamDict.Reset();
            _nowCommandIndex = 0;
            var commandParamList = new List<CommandInfo>();
            _errList = new LinkedList<ErrorFormat>();

            var lineCount = 0;
            var textReader = new StringReader(scriptText);
            var text = string.Empty;

            while ((text = textReader.ReadLine()) != null)
            {
                ++lineCount;
                // コメント除去
                var formatedText = CommandFormating(text);
                // --- メッセージデータ解析 ---
                // 改行のみの行だった時、
                // textBuilderにデータが入っていたらmsgコマンドに変換して追加
                if (string.IsNullOrEmpty(formatedText) || formatedText.IndexOf("\r\n") == 0 ||
                    formatedText.IndexOf("\n") == 0 ||
                    formatedText.IndexOf("\r") == 0)
                {
                    TryAddMsgCommandInfo(commandParamList);
                    continue;
                }
                // --- メッセージデータ解析 ---

                // コマンド解析
                var commandParam = CommandAnalysis(lineCount, formatedText);
                if (commandParam == null) { continue; }
                // ここまででtextBuilderにデータが入っている場合、文章表示用に今コマンドの前にmsgコマンドを作成して追加する
                TryAddMsgCommandInfo(commandParamList);
                LinterProcess(commandParam);
                commandParamList.Add(commandParam);
                _nowCommandIndex = commandParamList.Count;
            }
            // 最終行がメッセージだった場合のmsgコマンド追加措置
            TryAddMsgCommandInfo(commandParamList);
            // オプションパラメータ確定
            _optionParamDict.Apply();
            foreach (var dict in _optionParamDict.GetTable())
            {
                // それぞれのシリアライズ対象DictionaryのApplyを呼ばなければいけない
                dict.Value.Apply();
            }

            if (_errList.Count > 0)
            {
                // エラーログの吐き出し
                foreach (var err in _errList)
                {
                    Debug.LogError(string.Format(
                        "{0} :\nerror line {1} : {2} ",
                        scriptName, err.lineNumber, err.errorMsg));
                }
                return null;
            }

            var chunk = ScriptableObject.CreateInstance<CommandInfoChunk>();
            chunk.Setup(Path.GetFileNameWithoutExtension(scriptName), commandParamList);
            chunk.OptionDict = _optionParamDict;
            return chunk;
        }

        private void LinterProcess(CommandInfo commandInfo)
        {
            // linter系処理
            var linter = _linterRepository.GetLinter(commandInfo.commandName);
            // パラメータ指定時の構文にミスがないか静的解析
            CommandParamNameLinting(commandInfo);
            // コマンドによるパラメータ内構文解析
            if (linter != null)
            {
                CommandParamLinting(commandInfo, commandInfo.commandParams.GetTable(), linter);
                // Temp、Optionパラメータの制御
                ParamControlStep(commandInfo, commandInfo.commandParams.GetTable(), linter);
            }
            // コマンドパラメータ確定
            commandInfo.commandParams.Apply();
        }

        #region IChunkParamControl

        public int NowCommandIndex
        {
            get
            {
                return _nowCommandIndex;
            }
        }

        /// <summary>
        /// 指定タグのスタックにパラメータをpushする
        /// </summary>
        /// <param name="tag">スタックの分類指定</param>
        /// <param name="key">パラメータキー</param>
        /// <param name="value">パラメータ</param>
        public void PushTempParamForStack(string tag, string key, string value)
        {
            if (!_tempParamStacks.ContainsKey(tag))
            {
                _tempParamStacks.Add(tag, new Stack<KeyValuePair<string, string>>());
            }
            var stack = _tempParamStacks[tag];
            stack.Push(new KeyValuePair<string, string>(key, value));
        }

        /// <summary>
        /// 指定タグのスタックからパラメータをpopする
        /// </summary>
        /// <param name="tag">スタックの分類指定</param>
        /// <returns>パラメータ</returns>
        public KeyValuePair<string, string> PopTempParamByStack(string tag)
        {
            Debug.Assert(_tempParamStacks.ContainsKey(tag), "未登録のtagでスタックを取得しようとしました");
            return _tempParamStacks[tag].Pop();
        }

        /// <summary>
        /// 指定タグのディクショナリにパラメータを追加する
        /// </summary>
        /// <param name="tag">ディクショナリ特定用タグ</param>
        /// <param name="key">key</param>
        /// <param name="value">keyに紐づくvalue</param>
        public void AddTempParamForDict(string tag, string key, string value)
        {
            if (!_tempParamDicts.ContainsKey(tag))
            {
                _tempParamDicts.Add(tag, new Dictionary<string, string>());
            }
            _tempParamDicts[tag].Add(key, value);
        }

        /// <summary>
        /// パラメータ取得
        /// </summary>
        /// <param name="tag">タグ</param>
        /// <param name="key">タグが指すdictにあるデータのkey</param>
        public string GetTempParamForDict(string tag, string key)
        {
            return _tempParamDicts[tag][key];
        }

        /// <summary>
        /// 指定タグのディクショナリにパラメータを追加する
        /// </summary>
        /// <param name="tag">タグ</param>
        /// <param name="key">タグが指すdictに追加するkey</param>
        /// <param name="value">keyと紐付けるvalue</param>
        public void AddOptionParamForDict(string tag, string key, string value)
        {
            if (!_optionParamDict.GetTable().ContainsKey(tag))
            {
                _optionParamDict.SetValue(tag, new ParamDictionary());
            }
            _optionParamDict.GetTable()[tag].GetTable().Add(key, value);
        }

        /// <summary>
        /// パラメータを取得する
        /// </summary>
        /// <param name="tag">タグ</param>
        /// <param name="key">タグが指すdictにあるデータのkey</param>
        public string GetOptionParamByDict(string tag, string key)
        {
            return _optionParamDict.GetTable()[tag].GetTable()[key];
        }

        #endregion

        #region private

        /// <summary>
        /// コマンドオブジェクト生成者
        /// 構文チェックに使用
        /// </summary>
        private ILinterRepository _linterRepository;

        /// <summary>
        /// スクリプトパース中に使用
        /// 複数のコマンド間でパラメータ連携をさせるためのもの
        /// </summary>
        private Dictionary<string, Stack<KeyValuePair<string, string>>> _tempParamStacks
            = new Dictionary<string, Stack<KeyValuePair<string, string>>>();

        /// <summary>
        /// スクリプトパース中に使用
        /// 複数のコマンド間でパラメータ連携をさせるためのもの
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> _tempParamDicts
            = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// スクリプトパース中に使用
        /// オプションパラメータ格納用ディクショナリ
        /// </summary>
        private OptionParamDictionary _optionParamDict =
            new OptionParamDictionary();

        /// <summary>
        /// スクリプトパース中に使用
        /// 現在のコマンドリスト番号
        /// </summary>
        private int _nowCommandIndex;

        /// <summary>
        /// スクリプトパース時のエラーリスト
        /// </summary>
        LinkedList<ErrorFormat> _errList = new LinkedList<ErrorFormat>();

        /// <summary>
        /// msgコマンド生成向け
        /// </summary>
        private int _firstTextLineNumber = 0;
        private StringBuilder _textBuilder = new StringBuilder();

        /// <summary>
        /// textBuilderと付帯情報を使ってmsgコマンドを作成する
        /// 作成後textBuilderと付帯情報はリセットされる
        /// </summary>
        private void TryAddMsgCommandInfo(List<CommandInfo> addTarget)
        {
            if (_textBuilder.Length == 0)
            {
                return;
            }
            CommandInfo info = new CommandInfo();
            info.commandName = "msg";
            info.lineNumber = _firstTextLineNumber;
            info.commandParams = new ParamDictionary();
            var commandParamDic = info.commandParams.GetTable();
            string pattern = "[\r\n]+";
            var textData = Regex.Replace(_textBuilder.ToString(), pattern, "\\n");
            commandParamDic.Add("text", textData);

            var linter = _linterRepository.GetLinter(info.commandName);
            if (linter != null)
            {
                ParamControlStep(info, commandParamDic, linter);
            }

            info.commandParams.Apply();
            _textBuilder.Length = 0;
            _firstTextLineNumber = 0;
            addTarget.Add(info);
            _nowCommandIndex = addTarget.Count;
        }

        /// <summary>
        /// コマンド情報の解析を行い、コマンドパラメータを生成して返す
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private CommandInfo CommandAnalysis(int lineNumber, string text)
        {
            CommandInfo commandInfo = new CommandInfo();
            commandInfo.commandParams = new ParamDictionary();
            var commandParamDic = commandInfo.commandParams.GetTable();
            // 行番号の付加
            commandInfo.lineNumber = lineNumber;
            // コマンド名の取得
            var tag = Regex.Match(text, "@(\\S+)\\s");
            // コマンド検出されなかった場合は文章としてBuilderに付加
            if (!tag.Success)
            {
                if (_firstTextLineNumber == 0)
                {
                    _firstTextLineNumber = lineNumber;
                }
                _textBuilder.Append(text);
                return null;
            }
            // 検出したコマンドがシステムのコマンドに存在するかどうかチェック
            // 無いと記述ミスとしてエラー登録
            var commandName = tag.Groups[1].ToString();

            // Linterリストに存在していないコマンドは存在しないものとみなす
            if (!_linterRepository.ExistCommandName(commandName))
            {
                var errData = new ErrorFormat("Command not found: " + commandName, lineNumber);
                _errList.AddLast(errData);
                commandInfo.commandName = commandName;
                return commandInfo;
            }

            // コマンド名登録
            commandInfo.commandName = commandName;
            // コマンドパラメータ取得
            var cmdMatchRegex = new Regex("(\\S+)=(\\S+)");
            var cmdMatches = cmdMatchRegex.Matches(text);
            foreach (Match match in cmdMatches)
            {
                var paramTag = match.Groups[1].ToString();
                var paramValue = match.Groups[2].ToString();

                // 同名パラメータ検出時はエラーとして登録
                if (commandParamDic.ContainsKey(paramTag))
                {
                    var errData = new ErrorFormat(
                        string.Format(
                            "Parameter name duplicate: {0}={1}, {0}={3}",
                            paramTag, paramValue, commandParamDic[paramTag]),
                        lineNumber);
                    _errList.AddLast(errData);
                    continue;
                }

                commandParamDic.Add(paramTag, paramValue);
            }
            return commandInfo;
        }

        /// <summary>
        /// Temp、Optionパラメータの制御
        /// </summary>
        /// <param name="commandInfo">コマンド情報</param>
        /// <param name="commandParamDic">コマンドパラメータ</param>
        /// <param name="linter">commandInfoに対応するlinter</param>
        private void ParamControlStep(CommandInfo commandInfo, Dictionary<string, string> commandParamDic, Linter linter)
        {
            var errList = linter.ParamControlStep(this, commandParamDic);
            if (errList == null) { return; }
            foreach (var err in errList)
            {
                _errList.AddLast(err);
            }
        }

        /// <summary>
        /// 指定した文字列から不要情報を取り除いてコマンドのみ残した状態の文字列を生成して返す
        /// </summary>
        /// <param name="line">整形対象文字列</param>
        /// <returns>整形後文字列</returns>
        private string CommandFormating(string line)
        {
            var lineReader = new StringReader(line);
            var lineBuilder = new StringBuilder();
            var text = string.Empty;
            while ((text = lineReader.ReadLine()) != null)
            {
                // コメント部分の除去
                var commentCharacterCount = text.IndexOf(";");
                if (commentCharacterCount != -1)
                {
                    text = text.Substring(0, commentCharacterCount);
                }
                lineBuilder.AppendLine(text);
            }

            return lineBuilder.ToString();
        }

        /// <summary>
        /// コマンドパラメータ名の指定ミスがあるかどうかのエラー解析
        /// </summary>
        private void CommandParamNameLinting(CommandInfo commandInfo)
        {
            // 引数リストに存在しないパラメータがあるかどうかチェック
            foreach (var cmdParam in commandInfo.commandParams.GetTable())
            {
                var linter = _linterRepository.GetLinter(commandInfo.commandName);
                // linterが存在していない場合はチェックをスキップ
                if (linter == null) { break; }
                bool isExists = linter.KeyList.Any(_ => _.Equals(cmdParam.Key));
                if (!isExists)
                {
                    // コマンドの引数リストに存在しないものが指定されているのでエラー確定
                    var paramStringList = new StringBuilder();
                    foreach (string key in linter.KeyList)
                    {
                        paramStringList.Append(key + ", ");
                    }
                    var err = new ErrorFormat(
                        string.Format(
@"{0} Specified a parameter that does not exist in the command
specified: 
{1}
{0} available parameter:
{2}
",
                                 commandInfo.commandName, cmdParam.Key, paramStringList.ToString()),
                        commandInfo.lineNumber);
                    _errList.AddLast(err);
                }
            }
        }

        /// <summary>
        /// 各コマンドパラメータの内容に指定ミスがあるかどうかのエラー解析
        /// </summary>
        /// <param name="commandInfo"></param>
        /// <param name="commandParamDic"></param>
        private void CommandParamLinting(CommandInfo commandInfo, Dictionary<string, string> commandParamDic, Linter linter)
        {
            var errs = linter.CommandParamLinting(commandParamDic);
            if (errs == null) { return; }
            foreach (var err in errs)
            {
                err.lineNumber = commandInfo.lineNumber;
                _errList.AddLast(err);
            }
        }

        #endregion

    }
}

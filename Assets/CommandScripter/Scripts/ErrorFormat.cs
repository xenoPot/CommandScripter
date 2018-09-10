namespace CommandScripter
{
    /// <summary>
    /// コマンドスクリプトコンパイル時のエラー情報格納用クラス
    /// </summary>
    public class ErrorFormat
    {
        /// <summary>
        /// 行番号
        /// </summary>
        public int lineNumber;
        /// <summary>
        /// エラーメッセージ
        /// </summary>
        public string errorMsg;

        public ErrorFormat(string errorMsg)
        {
            this.lineNumber = 0;
            this.errorMsg = errorMsg;
        }

        public ErrorFormat(string errorMsg, int lineNumber)
        {
            this.lineNumber = lineNumber;
            this.errorMsg = errorMsg;
        }
    }
}

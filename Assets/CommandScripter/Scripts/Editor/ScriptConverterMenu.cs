using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace CommandScripter
{
    /// <summary>
    /// スクリプトコンバート提供するエディタ拡張用クラス
    /// </summary>
    public class ScriptConverterMenu
    {
        // TODO: パス設定系をScriptableObjectに移す
        /// <summary>
        /// コンバート元スクリプトの保存されたフォルダ
        /// </summary>
        private const string SrcFolderPath = "Assets/Resources/CommandScripts/Sources";
        /// <summary>
        /// コンバート後スクリプトチャンクの出力先フォルダ
        /// </summary>
        private const string DstScriptFolderPath = "Assets/Resources/CommandScripts/Converted";

        /// <summary>
        /// SrcFolderPath内にあるスクリプトをすべてコンバートして保存する
        /// </summary>
        [MenuItem("CommandScripter/Convert All")]
        static public void AllConvert()
        {
            // TODO: LinterRepositoryを設定ファイルで選択できるように
            ConvertStarter(new Scenario.LinterRepository(), Directory.GetFiles(SrcFolderPath, "*.txt", SearchOption.AllDirectories));
        }

        /// <summary>
        /// 選択しているファイルのみのコンバートを行う
        /// </summary>
        [MenuItem("Assets/CommandScripter/Selection Convert", false)]
        static public void SelectionConvert()
        {
            string[] paths = new string[Selection.instanceIDs.Length];
            for (int i = 0; i < Selection.instanceIDs.Length; ++i)
            {
                paths[i] = AssetDatabase.GetAssetPath(Selection.instanceIDs[i]);
            }
            // TODO: LinterRepositoryを設定ファイルで選択できるように
            ConvertStarter(new Scenario.LinterRepository(), paths);
        }

        [MenuItem("Assets/CommandScripter/Selection Convert", true)]
        static private bool ValidateSelectionConvert()
        {
            for (int i = 0; i < Selection.instanceIDs.Length; ++i)
            {
                if (Path.GetExtension(AssetDatabase.GetAssetPath(Selection.instanceIDs[i])) != ".txt")
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// プログレスバーの更新
        /// </summary>
        /// <param name="now"></param>
        /// <param name="total"></param>
        static private void UpdateProgress(int now, int total)
        {
            EditorUtility.DisplayProgressBar("Script convert",
                        string.Format("Converting... {0}/{1}", now, total),
                        (float)(now) / total);
        }

        /// <summary>
        /// コンバート処理開始時の共通処理
        /// </summary>
        /// <param name="srcFiles">コンバート元ファイル</param>
        static private void ConvertStarter(LinterRepository linterRepository, params string[] srcFiles)
        {
            Debug.Log("Start script convert");
            // スクリプトコンバータの生成
            var scriptConverter = new Script2Chunk();
            scriptConverter.SetLinterRepository(linterRepository);

            int successCount = 0;
            int failedCount = 0;
            UpdateProgress(successCount + failedCount, srcFiles.Length);
            foreach (var baseFilePath in srcFiles)
            {
                var filePath = baseFilePath.Replace("\\", "/");
                var srcLocalFilePath = filePath.Replace(SrcFolderPath, "");
                var dstLocalFilePath = Path.ChangeExtension(srcLocalFilePath, ".asset");
                if (ConvertAndSave(scriptConverter, dstLocalFilePath, filePath))
                {
                    ++successCount;
                }
                else
                {
                    ++failedCount;
                }
                UpdateProgress(successCount + failedCount, srcFiles.Length);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(string.Format("End convert Total file: {0}, success: {1}, failure: {2}",
                srcFiles.Length, successCount, failedCount));
        }

        /// <summary>
        /// 指定パスのファイルのコンバートを試み、
        /// 成功時にDstFolderPath以下の適切な箇所に生成したファイルを保存します
        /// </summary>
        /// <param name="converter">スクリプトコンバータ</param>
        /// <param name="dstLocalFilePath">出力先パス</param>
        /// <param name="fullSrcFilePath">コンバート元ファイルのフルパス</param>
        /// <returns>コンバートを正常に終えたかどうか</returns>
        static private bool ConvertAndSave(Script2Chunk converter, string dstLocalFilePath, string fullSrcFilePath)
        {
            Debug.Log("Target:" + dstLocalFilePath);
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            var chunk = converter.ParseScript(fullSrcFilePath, LoadTxtFile(fullSrcFilePath));
            if (chunk == null)
            {
                Debug.LogError(fullSrcFilePath + " : Failure create chunk");
                watch.Stop();
                Debug.Log("Parse time:" + watch.ElapsedMilliseconds + "ms");
                return false;
            }
            watch.Stop();
            Debug.Log("Parse time:" + watch.ElapsedMilliseconds + "ms");

            watch.Reset();
            watch.Start();
            // ScriptAssetの生成
            CommandInfoChunk targetChunk = TryGetAsset<CommandInfoChunk>(DstScriptFolderPath, dstLocalFilePath, null);
            if (targetChunk != null)
            {
                targetChunk.Setup(chunk);
                EditorUtility.SetDirty(targetChunk);
            }
            else
            {
                CreateAsset(chunk, DstScriptFolderPath, dstLocalFilePath, "commandScript/script", null, false);
                EditorUtility.SetDirty(chunk);
            }
            watch.Stop();
            Debug.Log("Create script asset:" + watch.ElapsedMilliseconds + "ms");
            return true;
        }

        static private T TryGetAsset<T>(string baseFolderPath, string localFilePath, string variant) where T : UnityEngine.Object
        {
            var variantFolderPath = FilePathCombine(baseFolderPath, variant);
            var filePath = FilePathCombine(variantFolderPath, localFilePath);
            filePath = Path.ChangeExtension(filePath, ".asset");
            if (!File.Exists(filePath))
            {
                return null;
            }
            return AssetDatabase.LoadAssetAtPath<T>(filePath);
        }

        static private void CreateAsset<T>(T data,
            string dstFolderPath, string dstLocalFilePath, string bundleName, string variant, bool overwrite) where T : UnityEngine.Object
        {
            // Assetの生成
            var variantFolderPath = FilePathCombine(dstFolderPath, variant);
            var dstFilePath = FilePathCombine(variantFolderPath, dstLocalFilePath);
            dstFilePath = Path.ChangeExtension(dstFilePath, ".asset");
            // 既にファイルがある場合はフラグに沿って分岐
            if (File.Exists(dstFilePath))
            {
                if (!overwrite)
                {
                    return;
                }
            }
            var dstDirectoryName = Path.GetDirectoryName(dstFilePath);
            if (!Directory.Exists(dstDirectoryName))
            {
                Directory.CreateDirectory(dstDirectoryName);
            }
            data.hideFlags = HideFlags.NotEditable;
            AssetDatabase.CreateAsset(data, dstFilePath);
            // AssetBundleNameの設定
            var importer = AssetImporter.GetAtPath(dstFilePath);
            if (importer == null)
            {
                Debug.LogError(dstFilePath + " : Failure get importer");
                return;
            }
            importer.assetBundleName = bundleName;
        }

        static private string FilePathCombine(string path1, string path2)
        {
            if (path1 == null) { return path2; }
            if (path2 == null) { return path1; }
            return (path1 + "/" + path2).Replace("//", "/");
        }

        /// <summary>
        /// テキストファイルを読み込んで文字列として返す
        /// </summary>
        /// <param name="filePath">フルファイルパス</param>
        /// <returns>テキストファイルの中身</returns>
        static private string LoadTxtFile(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            string text = string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader(fi.OpenRead(), Encoding.UTF8))
                {
                    text = sr.ReadToEnd();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("File read error:" + e);
            }
            return text;
        }

    }
}

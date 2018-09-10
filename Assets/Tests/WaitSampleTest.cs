using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class WaitSampleTest
{

    [Test]
    public void Convert()
    {
        var converter = new CommandScripter.Script2Chunk();
        converter.SetLinterRepository(new Scenario.LinterRepository());
        var scriptText = Resources.Load<TextAsset>("CommandScripts/Sources/WaitSample");
        var chunk = converter.ParseScript("WaitSample", scriptText.text);
        Assert.IsNotNull(chunk);
    }

    /// <summary>
    /// スクリプトテキストからのチャンク生成～実行
    /// </summary>
    [UnityTest]
    public IEnumerator ScenarioPlayByText()
    {
        Debug.Log("スクリプトテキストからのチャンク生成と実行");
        // スクリプトコンバータの生成
        var converter = new CommandScripter.Script2Chunk();
        converter.SetLinterRepository(new Scenario.LinterRepository());
        // スクリプトテキストの読み込み・チャンク生成
        var scriptText = Resources.Load<TextAsset>("CommandScripts/Sources/WaitSample");
        var chunk = converter.ParseScript("WaitSample", scriptText.text);
        Assert.IsNotNull(chunk);
        // コマンドコントローラ生成
        var playerGo = new GameObject();
        var player = playerGo.AddComponent<CommandScripter.CommandController>();
        var comRepo = playerGo.AddComponent<Scenario.CommandRepository>();
        player.Initialize(comRepo);
        // チャンクの実行
        player.Execute(chunk, () =>
        {
            Debug.Log("Play end");
        });
        Assert.IsTrue(player.IsPlaying);
        while (player.IsPlaying)
        {
            yield return null;
        }
    }

    /// <summary>
    /// コンバート済みのScriptableObjectからのチャンク生成～実行
    /// </summary>
    [UnityTest]
    public IEnumerator ScenarioPlayByScriptableObject()
    {
        Debug.Log("コンバート済みのScriptableObjectからのチャンク生成～実行");
        // スクリプトチャンク生成
        var scriptChunk = Resources.Load<CommandScripter.CommandInfoChunk>("CommandScripts/Converted/WaitSample");
        Assert.IsNotNull(scriptChunk);
        // コマンドコントローラ生成
        var playerGo = new GameObject();
        var player = playerGo.AddComponent<CommandScripter.CommandController>();
        var comRepo = playerGo.AddComponent<Scenario.CommandRepository>();
        player.Initialize(comRepo);
        // チャンクの実行
        player.Execute(scriptChunk, () =>
        {
            Debug.Log("Play end");
        });
        Assert.IsTrue(player.IsPlaying);
        while (player.IsPlaying)
        {
            yield return null;
        }
    }
}

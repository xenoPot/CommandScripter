using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scenario.Command;
using System;

namespace Scenario
{
    public enum CommandList
    {
        [CommandName("log")]
        log,
        [CommandName("msg")]
        msg,
        [CommandName("wait")]
        wait,
    }

    public class CommandNameAttribute : Attribute
    {
        public CommandNameAttribute(string name) { this.name = name; }
        public string Name { get { return this.name; } }

        private string name;
    }
    public static class ExtCommandName
    {
        public static string GetCommandName(this CommandList command)
        {
            var member = command.GetType().GetMember(command.ToString());
            var att = member[0].GetCustomAttributes(typeof(CommandNameAttribute), false);
            var name = ((CommandNameAttribute)att[0]).Name;
            return name;
        }
    }

    public class CommandRepository : CommandScripter.CommandRepository
    {
        private void Awake()
        {
            // スクリプトで使用できるコマンド群の定義
            Add<Log>(CommandList.log);
            Add<Msg>(CommandList.msg);
            Add<Wait>(CommandList.wait);
        }
        private void Add<T>(CommandList commandName) where T : CommandScripter.Command
        {
            AddCommand<T>(commandName.GetCommandName());
        }
    }


}


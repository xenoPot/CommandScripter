using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scenario.Linter;

namespace Scenario
{
    public class LinterRepository : CommandScripter.LinterRepository
    {

        public LinterRepository()
        {
            // 各CommandとLinterの紐付け
            Add<Msg>(CommandList.msg);
            Add<Wait>(CommandList.wait);
            Add<Log>(CommandList.log);
        }

        private void Add<T>(CommandList commandName) where T : CommandScripter.Linter
        {
            AddLinter<T>(commandName.GetCommandName());
        }

        private void Add(CommandList commandName)
        {
            AddLinter(commandName.GetCommandName());
        }

    }
}


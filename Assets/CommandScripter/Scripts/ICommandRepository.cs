using UnityEngine;
using System.Collections;

namespace CommandScripter
{
    public interface ICommandRepository
    {
        Command GetCommand(string commandName);
    }
}

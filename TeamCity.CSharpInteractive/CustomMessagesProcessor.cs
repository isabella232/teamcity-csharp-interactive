// ReSharper disable ClassNeverInstantiated.Global
namespace TeamCity.CSharpInteractive;

using Script.Cmd;
using Script.DotNet;

internal class CustomMessagesProcessor : IBuildMessagesProcessor
{
    public void ProcessMessages(in Output output, IEnumerable<BuildMessage> messages, Action<BuildMessage> nextHandler)
    {
        foreach (var buildMessage in messages)
        {
            nextHandler(buildMessage);
        }
    }
}
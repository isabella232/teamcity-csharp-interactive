namespace TeamCity.CSharpInteractive;

using System.Collections.Generic;
using Cmd;
using Dotnet;

internal interface IBuildOutputProcessor
{
    IEnumerable<BuildMessage> Convert(in Output output, IBuildResult result);
}
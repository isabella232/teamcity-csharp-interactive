// ReSharper disable UnusedMember.Global
namespace TeamCity.CSharpInteractive
{
    internal interface IStdErr
    {
        void WriteLine(params Text[] errorLine);
    }
}
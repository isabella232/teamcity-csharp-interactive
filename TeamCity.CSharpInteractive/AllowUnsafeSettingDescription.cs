// ReSharper disable ClassNeverInstantiated.Global
namespace TeamCity.CSharpInteractive
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    internal class AllowUnsafeSettingDescription : ISettingDescription
    {
        public bool IsVisible => true;

        public Type SettingType => typeof(AllowUnsafe);

        public string Key => "au";

        public string Description => "Allow or do not allow unsafe code";
    }
}
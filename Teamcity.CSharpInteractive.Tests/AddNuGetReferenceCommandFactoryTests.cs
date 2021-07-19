namespace Teamcity.CSharpInteractive.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Moq;
    using NuGet.Versioning;
    using Shouldly;
    using Xunit;

    public class AddNuGetReferenceCommandFactoryTests
    {
        private readonly Mock<ILog<AddNuGetReferenceCommandFactory>> _log;
        private readonly List<Text> _errors = new();

        public AddNuGetReferenceCommandFactoryTests()
        {
            _log = new Mock<ILog<AddNuGetReferenceCommandFactory>>();
            _log.Setup(i => i.Error(It.IsAny<ErrorId>(),It.IsAny<Text[]>())).Callback<ErrorId, Text[]>((_, text) => _errors.AddRange(text));
        }

        [Fact]
        public void ShouldProvideOrder()
        {
            // Given
            var factory = CreateInstance();
            
            // When
            
            // Then
            factory.Order.ShouldBe(0);
        }

        [Theory]
        [MemberData(nameof(Data))]
        internal void ShouldCreateCommands(string replCommand, ICommand[] expectedCommands, bool hasErrors)
        {
            // Given
            var factory = CreateInstance();

            // When
            var actualCommands = factory.Create(replCommand).ToArray();

            // Then
            actualCommands.ShouldBe(expectedCommands);
            _errors.Any().ShouldBe(hasErrors);
        }

        public static IEnumerable<object?[]> Data => new List<object?[]>
        {
            new object[]
            {
                "#r \"Abc.dll\"",
                Array.Empty<ICommand>(),
                false
            },
            new object[]
            {
                "#",
                Array.Empty<ICommand>(),
                false
            },
            new object[]
            {
                "#   ",
                Array.Empty<ICommand>(),
                false
            },
            new object[]
            {
                "",
                Array.Empty<ICommand>(),
                false
            },
            new object[]
            {
                "   ",
                Array.Empty<ICommand>(),
                false
            },
            new object[]
            {
                "#r \"nuget:Abc, 1.2.3\"",
                new [] {new AddNuGetReferenceCommand("Abc", new NuGetVersion(1,2,3))},
                false
            },
            new object[]
            {
                "  #r  \"NuGet:  Abc,    1.2.3 \"",
                new [] {new AddNuGetReferenceCommand("Abc", new NuGetVersion(1,2,3))},
                false
            },
            new object[]
            {
                "#r \"nuget:Abc, 1.2.3-beta1\"",
                new [] {new AddNuGetReferenceCommand("Abc", new NuGetVersion(new Version(1,2,3), "beta1"))},
                false
            },
            new object[]
            {
                "#r \"nuget:Abc\"",
                new [] {new AddNuGetReferenceCommand("Abc", default)},
                false
            },
            // Errors
            new object[]
            {
                "#r \"nuget:Abc, 1.2.3 xyz\"",
                Array.Empty<ICommand>(),
                false
            },
            new object[]
            {
                "#r \":nuget:Abc, xyz\"",
                Array.Empty<ICommand>(),
                false
            },
            new object[]
            {
                "#r   ",
                Array.Empty<ICommand>(),
                false
            },
            new object[]
            {
                "#r",
                Array.Empty<ICommand>(),
                false
            },
        };

        private AddNuGetReferenceCommandFactory CreateInstance() =>
            new(_log.Object);
    }
}
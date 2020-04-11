using System.Collections.Generic;
using System.Linq;
using CommandDotNet.Tests.ScenarioFramework;
using CommandDotNet.TestTools;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CommandDotNet.Tests.FeatureTests.ClassCommands
{
    public class SubCommandTests
    {
        private readonly ITestOutputHelper _output;

        public SubCommandTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanDiscoverAllTests()
        {
            var classNames = new AppRunner<ThreeLevelsApp>()
                .GetCommandClassTypes()
                .Select(t => t.Name)
                .ToList();
            classNames.Count.Should().Be(3);
            classNames.Should().ContainEquivalentOf(nameof(ThreeLevelsApp), nameof(Second), nameof(Third));

            classNames = new AppRunner<NestedThreeLevelsApp>()
                .GetCommandClassTypes()
                .Select(t => t.Name)
                .ToList();
            classNames.Count.Should().Be(3);
            classNames.Should().ContainEquivalentOf(nameof(NestedThreeLevelsApp), nameof(Second), nameof(Third));
        }

        [Theory]
        [MemberData(nameof(Scenarios))]
        public void Active(IScenarioForApp scenario)
        {
            new AppRunner(scenario.AppType).Verify(_output, scenario);
        }

        public static IEnumerable<object[]> Scenarios => 
            BuildScenarios<ThreeLevelsApp>("via property")
            .Union(BuildScenarios<NestedThreeLevelsApp>("via nested class"))
            .ToObjectArrays();

        public static Scenarios BuildScenarios<T>(string name) =>
            new Scenarios
            {
                new Scenario<T>($"{name} - no args (implicit help) includes 1st level commands and 2nd level app")
                {
                    WhenArgs = null,
                    Then =
                    {
                        Output = @"Usage: dotnet testhost.dll [command]

Commands:

  Do1
  Second

Use ""dotnet testhost.dll [command] --help"" for more information about a command."
                    }
                },
                new Scenario<T>($"{name} - help includes 1st level commands and 2nd level app")
                {
                    WhenArgs = "-h",
                    Then =
                    {
                        Output = @"Usage: dotnet testhost.dll [command]

Commands:

  Do1
  Second

Use ""dotnet testhost.dll [command] --help"" for more information about a command."
                    }
                },
                new Scenario<T>($"{name} - help for 2nd level app includes 2nd level commands and 3rd level app")
                {
                    WhenArgs = "Second -h",
                    Then =
                    {
                        Output = @"Usage: dotnet testhost.dll Second [command]

Commands:

  Do2
  Third

Use ""dotnet testhost.dll Second [command] --help"" for more information about a command."
                    }
                },
                new Scenario<T>($"{name} - help for 3rd level app includes 3rd level commands")
                {
                    WhenArgs = "Second Third -h",
                    Then =
                    {
                        Output = @"Usage: dotnet testhost.dll Second Third [command]

Commands:

  Do3

Use ""dotnet testhost.dll Second Third [command] --help"" for more information about a command."
                    }
                },
                new Scenario<T>($"{name} - can execute 1st level local command")
                {
                    WhenArgs = "Do1 --Opt1 1111 somearg",
                    Then = {Captured = {new ArgModel1 {Opt1 = "1111", Arg1 = "somearg"}}}
                },
                new Scenario<T>($"{name} - can execute 2nd level local command")
                {
                    WhenArgs = "Second Do2 --Opt2 1111 somearg",
                    Then = {Captured = {new ArgModel2 {Opt2 = "1111", Arg2 = "somearg"}}}
                },
                new Scenario<T>($"{name} - can execute 3rd level local command")
                {
                    WhenArgs = "Second Third Do3 --Opt3 1111 somearg",
                    Then = {Captured = {new ArgModel3 {Opt3 = "1111", Arg3 = "somearg"}}}
                }
            };

        private class ThreeLevelsApp
        {
            private TestCaptures TestCaptures { get; set; }

            [SubCommand]
            public Second Second { get; set; }

            public void Do1(ArgModel1 model)
            {
                TestCaptures.Capture(model);
            }
        }

        private class Second
        {
            private TestCaptures TestCaptures { get; set; }

            [SubCommand]
            public Third Third { get; set; }

            public void Do2(ArgModel2 model)
            {
                TestCaptures.Capture(model);
            }
        }

        private class Third
        {
            private TestCaptures TestCaptures { get; set; }

            public void Do3(ArgModel3 model)
            {
                TestCaptures.Capture(model);
            }
        }

        private class NestedThreeLevelsApp
        {
            private TestCaptures TestCaptures { get; set; }


            public void Do1(ArgModel1 model)
            {
                TestCaptures.Capture(model);
            }

            [SubCommand]
            public class Second
            {
                private TestCaptures TestCaptures { get; set; }

                public void Do2(ArgModel2 model)
                {
                    TestCaptures.Capture(model);
                }

                [SubCommand]
                public class Third
                {
                    private TestCaptures TestCaptures { get; set; }

                    public void Do3(ArgModel3 model)
                    {
                        TestCaptures.Capture(model);
                    }
                }
            }
        }

        private class ArgModel1 : IArgumentModel
        {
            [Option]
            public string Opt1 { get; set; }
            [Operand]
            public string Arg1 { get; set; }
        }

        private class ArgModel2 : IArgumentModel
        {
            [Option]
            public string Opt2 { get; set; }
            [Operand]
            public string Arg2 { get; set; }
        }

        private class ArgModel3 : IArgumentModel
        {
            [Option]
            public string Opt3 { get; set; }
            [Operand]
            public string Arg3 { get; set; }
        }
    }
}
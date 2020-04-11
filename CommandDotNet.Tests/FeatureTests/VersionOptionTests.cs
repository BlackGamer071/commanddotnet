using CommandDotNet.TestTools.Scenarios;
using Xunit;
using Xunit.Abstractions;

namespace CommandDotNet.Tests.FeatureTests
{
    public class VersionOptionTests
    {
        private readonly ITestOutputHelper _output;

        public VersionOptionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void WhenVersionEnabled_BasicHelp_IncludesVersionOption()
        {
            var scenario = new Scenario
            {
                WhenArgs = "-h",
                Then = {OutputContainsTexts = {"-v | --version  Show version information"}}
            };

            new AppRunner<App>(TestAppSettings.BasicHelp)
                .UseVersionMiddleware()
                .Verify(_output, scenario);
        }

        [Fact]
        public void WhenVersionEnabled_DetailedHelp_IncludesVersionOption()
        {
            var scenario = new Scenario
            {
                WhenArgs = "-h",
                Then = {
                    OutputContainsTexts = { @"  -v | --version          
  Show version information" }
                }
            };

            new AppRunner<App>(TestAppSettings.DetailedHelp)
                .UseVersionMiddleware()
                .Verify(_output, scenario);
        }

        [Fact]
        public void WhenVersionDisabled_BasicHelp_DoesNotIncludeVersionOption()
        {
            new AppRunner<App>(TestAppSettings.BasicHelp)
                .Verify(_output, new Scenario
                {
                    WhenArgs = "-h",
                    Then = { OutputNotContainsTexts = { "-v | --version" } }
                });
        }

        [Fact]
        public void WhenVersionDisabled_DetailedHelp_DoesNotIncludeVersionOption()
        {
            new AppRunner<App>(TestAppSettings.DetailedHelp)
                .Verify(_output, new Scenario
                {
                    WhenArgs = "-h",
                    Then = {OutputNotContainsTexts = {"-v | --version"}}
                });
        }

        [Fact]
        public void WhenVersionEnabled_Version_LongName_OutputsVersion()
        {
            var scenario = new Scenario
            {
                WhenArgs = "--version",
                Then =
                {
                    Output = @"testhost.dll
16.2.0"
                }
            };

            new AppRunner<App>()
                .UseVersionMiddleware()
                .Verify(_output, scenario);
        }

        [Fact]
        public void WhenVersionEnabled_Version_ShortName_OutputsVersion()
        {
            var scenario = new Scenario
            {
                WhenArgs = "-v",
                Then =
                {
                    Output = @"testhost.dll
16.2.0"
                }
            };
            
            new AppRunner<App>()
                .UseVersionMiddleware()
                .Verify(_output, scenario);
        }

        [Fact]
        public void WhenVersionDisabled_Version_LongName_NotRecognized()
        {
            new AppRunner<App>()
                .Verify(_output, new Scenario
                {
                    WhenArgs = "--version",
                    Then =
                    {
                        ExitCode = 1,
                        OutputContainsTexts = { "Unrecognized option '--version'" }
                    }
                });
        }

        [Fact]
        public void WhenVersionDisabled_Version_ShortName_NotRecognized()
        {
            new AppRunner<App>()
                .Verify(_output, new Scenario
                {
                    WhenArgs = "-v",
                    Then =
                    {
                        ExitCode = 1,
                        OutputContainsTexts = {"Unrecognized option '-v'"}
                    }
                });
        }

        public class App
        {

        }
    }
}
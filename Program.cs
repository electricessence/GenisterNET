using GenisterNET;
using Spectre.Console.Cli;

var app = new CommandApp<GenerateCommand>();
app.Configure(config =>
{
#if DEBUG
	config.PropagateExceptions();
	config.ValidateExamples();
#endif
});

await app.RunAsync(args);
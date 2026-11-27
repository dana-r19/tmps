// Entry point: picks which client to run.
//
//   dotnet run --project src/MusicPlayer                       Lab 3 demo: Command + State + Observer + Strategy
//   dotnet run --project src/MusicPlayer -- --interactive      type commands yourself (behavior changes at runtime)
//   dotnet run --project src/MusicPlayer -- --structural       Lab 2 demo: Composite + Decorator + Facade
//   dotnet run --project src/MusicPlayer -- --play [folder]    play real files (Windows; default folder: songs)
using MusicPlayer.Client;
using MusicPlayer.Creational;
using MusicPlayer.Domain.Audio;
using MusicPlayer.Structural.Facade;

if (args.Contains("--interactive"))
{
    IMusicPlayer player = new MusicPlayerFacade(
        new ConsoleAudioOutput(PlayerSettings.Instance.DefaultVolume), SongLoaderRegistry.CreateDefault());
    new InteractiveShell(player.OpenSession(DemoLibrary.Create(player)), Console.In, Console.Out).Run();
}
else if (args.Contains("--structural"))
{
    StructuralDemo.Run();
}
else if (args.Contains("--play"))
{
    int playIndex = Array.IndexOf(args, "--play");
    FolderPlayer.Run(playIndex + 1 < args.Length ? args[playIndex + 1] : "songs");
}
else
{
    BehavioralDemo.Run();
}

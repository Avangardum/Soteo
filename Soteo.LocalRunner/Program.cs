using System.Diagnostics;

List<Process> processes = [];

BuildSolution();
// StartAuthServer();
// StartCampaignServer();
// StartShardServers();
StartClients();
Console.WriteLine("Press any key to exit");
Console.ReadKey(true);
processes.ForEach(it => it.Kill());

void BuildSolution()
{
    var process = new Process();
    process.StartInfo.FileName = "dotnet";
    process.StartInfo.Arguments = "build Godot/Soteo.sln";
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    process.WaitForExit();
    if (process.ExitCode != 0) throw new Exception("Build failed. Build the solution manually for details.");
    Console.WriteLine("Build succeeded");
}

void StartAuthServer()
{
    var process = new Process();
    process.StartInfo.FileName = "dotnet";
    process.StartInfo.Arguments = "run --no-build --project Soteo.AuthServer --launch-profile https";
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    //process.OutputDataReceived += (_, e) => Console.WriteLine("\nAuth server message:\n" + e.Data);
    process.ErrorDataReceived += (_, e) => Console.WriteLine("\nAuth server error:\n" + e.Data);
    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    Console.WriteLine("Auth server started");
    processes.Add(process);
}

void StartCampaignServer()
{
    var process = new Process();
    process.StartInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Godot");
    process.StartInfo.FileName = "godot3.6.2.exe";
    process.StartInfo.Arguments = "--no-window Scenes/CampaignServer.tscn --external-shard-server 00000000-0000-0000-0000-0000000deb09";
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    //process.OutputDataReceived += (_, e) => Console.WriteLine("\nCampaign server message:\n" + e.Data);
    process.ErrorDataReceived += (_, e) => Console.WriteLine("\nCampaign server error:\n" + e.Data);
    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    Console.WriteLine("Campaign server started");
    processes.Add(process);
}

void StartShardServers()
{
    
}

void StartClients()
{
    var process = new Process();
    process.StartInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Godot");
    process.StartInfo.FileName = "godot3.6.2.exe";
    process.StartInfo.Arguments = "--no-scroll --position 10,10 --resolution 1000x500";
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    //process.OutputDataReceived += (_, e) => Console.WriteLine("\nClient message:\n" + e.Data);
    process.ErrorDataReceived += (_, e) => Console.WriteLine("\nClient error:\n" + e.Data);
    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    Console.WriteLine("Client started");
    processes.Add(process);
}

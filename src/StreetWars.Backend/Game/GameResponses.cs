using System;

namespace StreetWars.Backend.Game;

public sealed class BackendGameProject
{
    public BackendGameProject() 
        => (Name, Transport, Auth) 
        = (string.Empty, string.Empty, string.Empty);
    public BackendGameProject(string name, string transport = "SignalR", string auth = "/api/auth") 
        => (Name, Transport, Auth) 
        = (name, transport, auth);
    public string Name { get; set; }
    public string Transport { get; set; } 
    public string Auth { get; set; } 
}

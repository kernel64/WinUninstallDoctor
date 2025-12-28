public class ProgramEntry
{
    public string DisplayName { get; set; }
    public string DisplayVersion { get; set; }
    public string Publisher { get; set; }
    public string UninstallString { get; set; }
    public string DisplayIcon { get; set; }
    public string RegistryKeyPath { get; set; }
    public bool UninstallExists { get; set; }
    public bool HasProblem { get; set; }
    public bool IsSystemComponent { get; set; }
    public bool IsSelectable { get; set; }
}

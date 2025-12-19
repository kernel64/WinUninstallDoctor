using System.Threading;

public static class SingleInstanceHelper
{
    private static Mutex mutex;

    public static bool EnsureSingleInstance()
    {
        mutex = new Mutex(true, "WinUninstallDoctor_Mutex", out bool created);
        return created;
    }
}

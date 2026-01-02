using System.Drawing;
using System.IO;
using System.Reflection;

namespace WinUninstallDoctor
{

    public static class Utils
    {
        public static Image ByteArrayToImage(byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                return Image.FromStream(ms);
            }
        }

        public static Icon LoadIconFromResource(byte[] iconBytes)
        {
            using (var ms = new MemoryStream(iconBytes))
            using (var icon = new Icon(ms))
            {
                return (Icon)icon.Clone();
            }
        }

        public static string GetVersion()
        {
            return Assembly
                .GetExecutingAssembly()
                .GetName()
                .Version?
                .ToString() ?? "1.0.0";
        }
    }

}

using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionReport
{
    internal class LOG
    {
        StreamWriter sw;

        public LOG()
        {
#if DEBUG
            if(!Directory.Exists(@"AppData%\Roaming\XIVLauncher\pluiginConfigs\FashionReport"))
                Directory.CreateDirectory(@"AppData%\Roaming\XIVLauncher\pluiginConfigs\FashionReport");
            sw = new StreamWriter(@"D:\Log3.txt", false);
            sw.AutoFlush = true;
#endif
        }

        public void Write(string sMessage)
        {
#if DEBUG
            sw.Write(sMessage);
#endif
        }

        public void WriteLine(string sMessage)
        {
#if DEBUG
            sw.WriteLine(sMessage);
#endif
        }

        public void Dispose()
        {
#if DEBUG
            sw.Close();
#endif
        }
    }
}

using System;
using System.IO;
using System.Text;

namespace SignalMeter.Models
{
    public static class Settings
    {

        private static string com;

        public static string Com { get => com; set { com = value; SaveChanges(); } }

        static Settings()
        {
            try
            {
                using (FileStream fstream = new FileStream($"settings.dat", FileMode.CreateNew)) { }
                com = "COM1";
                SaveChanges();
            }
            catch (IOException)
            {
                using (StreamReader sr = new StreamReader($"settings.dat", Encoding.Default))
                {
                    com = sr.ReadLine();
                }
            }
        }

        private static void SaveChanges()
        {
            using (StreamWriter sw = new StreamWriter($"settings.dat", false, Encoding.Default))
            {
                sw.WriteLine(Com);
            }
        }
    }
}

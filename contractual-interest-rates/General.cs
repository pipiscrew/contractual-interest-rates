using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace WindowsFormsApplication2
{
    public static class General
    {
        public static string appTitle = "Εξωτραπεζικά επιτόκια - υπολογισμός τόκων";
        public static string datFile;

        public static void Serializea<T>(List<T> data, string filepath)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(filepath, FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, data);
            stream.Close();
        }

        public static T DeSerialize<T>(string filepath)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                return (T)formatter.Deserialize(stream);
            }
            catch
            {
                return default(T);
            }
        }

        public static string GetLastAppTitle()
        {
            FileInfo fi = new FileInfo(datFile);
            return appTitle + " - Τελευταία ενημέρωση επιτοκίων " + fi.LastWriteTime.ToString("dd-MM-yyyy HH:mm:ss");
        }

    }
}

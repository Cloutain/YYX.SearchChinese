using System.IO;

namespace YYX.SearchChinese
{
    class TextFile
    {
        public static void Read(string filePath, out string text)
        {
            using (var file = new FileStream(filePath, FileMode.Open))
            using (var reader = new StreamReader(file))
            {
                text = reader.ReadToEnd();
            }
        }

        public static void Write(string filePath, string text)
        {
            using (var file = new FileStream(filePath, FileMode.Create))
            using (var writer = new StreamWriter(file))
            {
                writer.Write(text);
            }
        }
    }
}

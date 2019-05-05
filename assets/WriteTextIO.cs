using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;


public class WriteTextIO : IWriteTextIO
{
    public static int counter;
    public string fileName;
    private FileStream fs;
    private byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);

    // Creates a textfile and writes the title in it
    public void CreateFile()
    {
        try
        {
            string path = Directory.GetCurrentDirectory();
            fileName = path + "-IOData.txt";
            while (File.Exists(fileName))
            {
                fileName = path + "-IOData" + ++counter + ".txt";
            }
            File.WriteAllText(fileName, "UDP Server received data: \n\n");
        }
        catch (Exception err)
        {
            Console.WriteLine(err);
        }

    }

    // Writes to the textfile
    public void WriteTextFile(byte[] data)
    {
        // Add some text to file    
        string time = DateTime.Now.ToString("T",
                  CultureInfo.CreateSpecificCulture("es-ES"));
        string text = time + "\n" + Convert2String(data) + "\n";

        File.AppendAllText(fileName, text);
    }

    private string Convert2String(byte[] data)
    {
        char[] characters = data.Select(b => (char)b).ToArray();
        return new string(characters);
    }
}

using System.IO;

namespace Achieve.Encrypt
{
    public static class EncryptAsset
    {
        
        public static void Encrypt(string filePath, string newFilePath)
        {
            string key = "Guofw";
            var keyChar = System.Text.Encoding.ASCII.GetBytes(key);
            var bytes = File.ReadAllBytes(filePath);
            
            byte[] contentAfterChar = new byte[bytes.Length];  //加密后的字符数组
            for (int i = 0; i < bytes.Length; i++)
            {
                contentAfterChar[i] = (byte)(bytes[i] ^ keyChar[i%5]);
            }
            File.WriteAllBytes(newFilePath, contentAfterChar);
        }
    }
}

using System.IO;

namespace Achieve.Encrypt
{
    public static class DecryptAsset
    {
        public static void Decrypt(string filePath, string newFilePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }
            string key = "Guofw";
            var keyChar = System.Text.Encoding.ASCII.GetBytes(key);
            var contentChar = File.ReadAllBytes(filePath);
            //解密
            byte[] contentBeforeChar = new byte[contentChar.Length];  //解密后的字符数组
            for (int i = 0; i < contentChar.Length; i++)
            {
                contentBeforeChar[i] = (byte)(contentChar[i] ^ keyChar[i%5]);
            }
            File.WriteAllBytes(newFilePath,contentBeforeChar);
        }

    }
}

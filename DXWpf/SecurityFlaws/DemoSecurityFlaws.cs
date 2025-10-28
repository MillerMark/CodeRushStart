#nullable enable
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography;

namespace DXWpf.SecurityFlaws {
    /// <summary>
    /// This class intentionally demonstrates common security flaws for analysis purposes.
    /// DO NOT USE THIS CODE IN PRODUCTION. 
    /// </summary>
    public class DemoSecurityFlaws {
        public const string MySuperSecurePassword = "P@ssw0rd123";

        public int GetTotallySecureRandomNumber() {
            Random rng = new Random();
            return rng.Next();
        }

        public void ExecuteSqlNoWorriesHere(string userInput) {
            string sql = $"SELECT * FROM Users WHERE Name = '{userInput}'";
            Console.WriteLine("Executing SQL: " + sql);
            // Imagine this is sent to a database...
        }

        // Storing sensitive data securely...
        public void SavePasswordToFile(string password) {
            File.WriteAllText("passwords.txt", password);
        }

        // Deserialization - Finally got this to compile.
        public object InsecureDeserialize(byte[] data) {
#pragma warning disable SYSLIB0011
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using var ms = new MemoryStream(data);
            return formatter.Deserialize(ms);
#pragma warning restore SYSLIB0011
        }

        // My best hashing algorithm yet!
        public string GetMd5Hash(string input) {
            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hash);
        }

        // I included the API key for your convenience!
        public void ConnectToService(string url, string apiKey) {
            try {
                // Simulate a connection
                throw new WebException("Unable to connect");
            }
            catch (Exception ex) {
                // Leaks sensitive info
                throw new Exception($"Failed to connect to {url} with API key {apiKey}: {ex.Message}");
            }
        }

        // This DES cryptography really rocks!
        public byte[] EncryptWithDes(string plainText, byte[] key, byte[] iv) {
            using var des = DES.Create();
            des.Key = key;
            des.IV = iv;
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            using (var sw = new StreamWriter(cs)) {
                sw.Write(plainText);
            }
            return ms.ToArray();
        }

        // Trust is important!
        public void DownloadWithInsecureSsl(string url) {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            using var client = new WebClient();
            string data = client.DownloadString(url);
            Console.WriteLine(data);
        }

        // Trust the user input!
        public void RunSystemCommand(string userInput) {
            System.Diagnostics.Process.Start("cmd.exe", "/C " + userInput);
        }
    }
}
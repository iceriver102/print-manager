using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Alta.Class
{
    public static class FileExtensions
    {
        public static string Md5(this FileInfo file)
        {
            if (!file.Exists)
                return string.Empty;
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file.FullName))
                {
                    var hash= md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", String.Empty).Trim().ToLower();
                }
            }
        }

        public static Uri toUri(this FileInfo file,UriKind kind= UriKind.RelativeOrAbsolute)
        {
            if (!file.Exists)
                throw new Exception("File Khong ton tai");
            return new Uri(file.FullName, kind);
        }

        public static void deleteAll(this DirectoryInfo Dir)
        {
            if (!Dir.Exists)
                return;
            foreach (FileInfo file in Dir.GetFiles())
            {
                if (IsFileLocked(file) == false)
                    file.Delete();
            }
            foreach (DirectoryInfo dir in Dir.GetDirectories())
            {
                dir.Delete(true);
            }

        }

        public static bool Copy(this FileInfo file, String toFile)
        {
            if(string.IsNullOrEmpty(toFile))
                    throw new Exception("File chuyen toi khong dc de trong");
            
            if (file.Exists)
            {
                File.Copy(file.FullName, toFile);
                return true;
            }
            return false;
        }
        public static bool IsFileLocked(this FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }
        public static string ConvertImageToBase64(this FileInfo inf)
        {
            if (!inf.Exists)
            {
                return string.Empty;
            }
            using (Image image = Image.FromFile(inf.FullName))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
        }

        public static string toBase64(this FileInfo file)
        {
            byte[] bytes = File.ReadAllBytes(file.FullName);
            return Convert.ToBase64String(bytes);
        }
        public static IEnumerable<FileInfo> getAllFiles(this DirectoryInfo dir, params string[] extensions)
        {
            if (!dir.Exists)
                return null;
            try {
                IEnumerable<FileInfo> files = dir.EnumerateFiles();
                if (extensions == null)
                    return files;
                return files.Where(f => extensions.Contains(f.Extension.ToLower()));
            }catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}

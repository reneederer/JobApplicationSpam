using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace JobApplicationSpam.Models
{
    public class FileConverter
    {
        public static bool IsUploadFileTypeValid(string fileType)
        {
            switch(fileType.ToLower())
            {
                case ".pdf":
                case ".odt":
                case ".docx":
                case ".doc":
                    return true;
                default:
                    return false;
            }
        }

        public static string FindUnusedFileName(string fileName, IEnumerable<string> existingFiles)
        {
            string GetFileName(int index)
            {
                if(index == 0) { return fileName; }
                else return $"{Path.GetFileNameWithoutExtension(fileName)} ( {index}){Path.GetExtension(fileName)}";
            }
            for(int i = 0; i < 1000000; ++i)
            {
                var currentFileName = GetFileName(i);
                if (!existingFiles.Contains(currentFileName))
                {
                    return currentFileName;
                }
            }
            return Guid.NewGuid().ToString() + Path.GetExtension(fileName);
        }

        public static bool MergePdfs(IEnumerable<string> pdfFilePaths, string mergedPath)
        {
            try
            {
                using (var outputDocument = new PdfDocument())
                {
                    foreach (var pdfFilePath in pdfFilePaths)
                    {
                        var inputDocument = PdfReader.Open(pdfFilePath, PdfDocumentOpenMode.Import);
                        for (int i = 0; i < inputDocument.Pages.Count; ++i)
                        {
                            outputDocument.AddPage(inputDocument.Pages[i]);
                        }
                    }
                    if (outputDocument.PageCount >= 1)
                    {
                        outputDocument.Save(mergedPath);
                    }
                }
                return true;
            }
            catch(Exception err)
            {
                return false;
            }
        }

        public static bool ConvertTo(string targetFormat, string path, string outputPath)
        {
            if(targetFormat.Length < 2 || ! targetFormat.StartsWith('.')) { return false; }
            var fileName = Path.GetFileName(path);
            var tmpPaths = new UnzipPaths();
            var extension = Path.GetExtension(path).ToLower();
            if(targetFormat == extension)
            {
                return true;
            }
            System.IO.File.Delete(outputPath);
            using (var process1 = new System.Diagnostics.Process())
            {
                process1.StartInfo.FileName = "C:/Program Files/LibreOffice 5/program/python.exe";
                process1.StartInfo.UseShellExecute = false;
                process1.StartInfo.Arguments =
                String.Format
                    (@" ""{0}"" --format {1} --output=""{2}"" ""{3}"" ",
                     "c:/Program Files/unoconv/unoconv",
                     targetFormat.Substring(1),
                     outputPath,
                     path);
                process1.StartInfo.CreateNoWindow = true;
                process1.Start();
                process1.WaitForExit();
                process1.Close();
            }
            return System.IO.File.Exists(outputPath);

                            /*
                            ZipFile.ExtractToDirectory(path, tmpPaths.UnzipTo);
                            ReplaceInDirectory(tmpPaths.UnzipTo, dict);
                            ZipFile.CreateFromDirectory(tmpPaths.UnzipTo, Path.Combine(tmpPaths.ZipTo, fileName));

                            var pdfOutputPath = Path.ChangeExtension(path, ".pdf");
                            System.IO.File.Delete(pdfOutputPath);
                            using (var process1 = new System.Diagnostics.Process())
                            {
                                process1.StartInfo.FileName = "C:/Program Files/LibreOffice 5/program/python.exe";
                                process1.StartInfo.UseShellExecute = false;
                                process1.StartInfo.Arguments =
                                String.Format(@" ""{0}"" --format pdf --output=""{1}"" ""{2}"" ", "c:/Program Files/unoconv/unoconv", pdfOutputPath, path);
                                process1.StartInfo.CreateNoWindow = true;
                                process1.Start();
                                process1.WaitForExit();
                            }
                            if (System.IO.File.Exists(pdfOutputPath))
                            {
                                return pdfOutputPath;
                            }
                            else
                            {
                                throw new Exception("File was not converted");
                            }
                            */
        }


        public static UploadedFileData GetUploadedFileData
        (string userId,
         string filePath,
         IEnumerable<string> dbFileNames,
         IEnumerable<string> diskFileNames)
        {
            try
            {
                UserPath userPath = new UserPath(userId);
                var fileName = Path.GetFileName(filePath);
                var extension = Path.GetExtension(filePath).ToLower();
                switch (extension)
                {
                    case ".pdf":
                        {
                            var unusedDiskFileName = FindUnusedFileName(fileName, diskFileNames);
                            var savePath = Path.Combine(Path.GetDirectoryName(filePath), unusedDiskFileName);
                            var unusedDbFileName = FindUnusedFileName(fileName, dbFileNames);
                            return
                                new UploadedFileData
                                {
                                    OriginalFilePath = filePath,
                                    SavedFileName = unusedDiskFileName,
                                    DisplayedFileName = unusedDbFileName,
                                    ConvertAndSave =
                                        () =>
                                        {
                                            if (MergePdfs(new[] { filePath }, Path.Combine(new TmpPath().Path, "mypdf.pdf")))
                                            {
                                                if (new FileInfo(filePath).FullName.ToLower() != new FileInfo(filePath).FullName.ToLower())
                                                {
                                                    File.Copy(filePath, savePath, true);
                                                }
                                                return true;
                                            }
                                            else { return false; }
                                        }
                                };
                        }
                    case ".odt":
                        {
                            var unusedDiskFileName = FindUnusedFileName(fileName, diskFileNames);
                            var savePath = Path.Combine(Path.GetDirectoryName(filePath), unusedDiskFileName);
                            var unusedDbFileName = FindUnusedFileName(fileName, dbFileNames);
                            return
                                new UploadedFileData
                                {
                                    OriginalFilePath = filePath,
                                    SavedFileName = unusedDiskFileName,
                                    DisplayedFileName = unusedDbFileName,
                                    ConvertAndSave =
                                        () =>
                                        {
                                            var tmpPath = new TmpPath();
                                            var path = Path.Combine(tmpPath.Path, "mypdf.pdf");
                                            if (ConvertTo(".pdf", filePath, path))
                                            {
                                                if (new FileInfo(filePath).FullName.ToLower() != new FileInfo(filePath).FullName.ToLower())
                                                {
                                                    File.Copy(filePath, savePath, true);
                                                }
                                                return true;
                                            }
                                            else { return false; }
                                        }
                                };
                        }
                    case ".doc":
                    case ".docx":
                        {
                            var unusedDiskFileName = FindUnusedFileName(Path.ChangeExtension(fileName, ".odt"), diskFileNames);
                            var savePath = Path.Combine(Path.GetDirectoryName(filePath), unusedDiskFileName);
                            var unusedDbFileName = FindUnusedFileName(Path.ChangeExtension(fileName, ".odt"), dbFileNames);
                            return
                                new UploadedFileData
                                {
                                    OriginalFilePath = filePath,
                                    SavedFileName = unusedDiskFileName,
                                    DisplayedFileName = unusedDbFileName,
                                    ConvertAndSave =
                                  () =>
                                  {
                                      var tmpPath = new TmpPath();
                                      var outputPath = Path.Combine(tmpPath.Path, "myodt.odt");
                                      if (ConvertTo(".odt", filePath, outputPath))
                                      {
                                          if (new FileInfo(filePath).FullName.ToLower() != new FileInfo(filePath).FullName.ToLower())
                                          {
                                              File.Copy(outputPath, savePath, true);
                                          }
                                          return true;
                                      }
                                      else { return false; }
                                  }

                                };
                        }
                    default:
                        throw new Exception("Filetype not found " + extension);
                }
            }
            catch(Exception e)
            {
                throw;
            }
        }

        public static string ReplaceInString(string s, IDictionary<string, string> dict)
        {
            return "";
        }

        public static string ReplaceInOdt(string filePath, IDictionary<string, string> dict)
        {
            return "";
        }

        public static bool AreFilesEqual(string filePath1, string filePath2)
        {
            if(filePath1 == null || filePath2 == null) { return false; }

            string fileType1 = Path.GetExtension(filePath1).ToLower();
            string fileType2 = Path.GetExtension(filePath2).ToLower();
            if(fileType1 != fileType2) { return false; }

            if(fileType1 == ".odt")
            {
                var extractPath1 = new TmpPath().Path;
                var extractPath2 = new TmpPath().Path;
                ZipFile.ExtractToDirectory(filePath1, extractPath1);
                ZipFile.ExtractToDirectory(filePath2, extractPath2);
                File.Delete(Path.Combine(extractPath1, "meta.xml"));
                File.Delete(Path.Combine(extractPath2, "meta.xml"));
                return AreDirectoriesEqual(extractPath1, extractPath2);
            }
            else
            {
                if(filePath1.Length != filePath2.Length) { return false; }
                var fs1 = new FileStream(filePath1, FileMode.Open);
                var fs2 = new FileStream(filePath2, FileMode.Open);
                var bytes1 = new byte[2048];
                var bytes2 = new byte[2048];
                for(int offset = 0; offset < fs1.Length ; offset += 2048)
                {
                    int readCount1 = fs1.Read(bytes1, 0, (int)Math.Min(2048, fs1.Length - offset));
                    int readCount2 = fs2.Read(bytes2, 0, (int)Math.Min(2048, fs1.Length - offset));
                    for(int i = 0; i < bytes1.Length;++i)
                    {
                        if(bytes1[i] != bytes2[i])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static bool AreDirectoriesEqual(string directory1, string directory2)
        {
            var files1 = Directory.EnumerateFiles(directory1);
            var files2 = Directory.EnumerateFiles(directory2);
            if(files1.Count() != files2.Count()) { return false; }
            for(int i = 0; i < files1.Count(); ++i)
            {
                if(!AreFilesEqual(files1.ElementAt(i), files2.ElementAt(i))) { return false; }
            }
            var directories1 = Directory.EnumerateDirectories(directory1);
            var directories2 = Directory.EnumerateDirectories(directory2);
            if(directory1.Count() != directory2.Count()) { return false; }
            for(int i = 0; i < directories1.Count(); ++i)
            {
                return AreDirectoriesEqual(directories1.ElementAt(i), directories2.ElementAt(i));
            }
            return true;
        }

        public static int GetMaxUploadSizeInBytes()
        {
            return 5_000_000;
        }

        public static string GetMaxUploadSizeAsString()
        {
            return $"{(((int)(GetMaxUploadSizeInBytes() / 10_000)) / 100.0)} MB";
        }

        public static void UnzipTo(string zipPath, string extractPath)
        {
        }

        public void ReplaceInDirectory(string path, IDictionary<string, string> dict)
        {
            foreach(var currentDir in Directory.EnumerateDirectories(path))
            {
                ReplaceInDirectory(Path.Combine(path, currentDir), dict);
            }
            foreach(var currentFile in Directory.EnumerateFiles(path))
            {
                var fullFilePath = Path.Combine(path, currentFile);
                if(Path.GetExtension(fullFilePath).ToLower() == ".xml")
                {
                    var content = System.IO.File.ReadAllText(fullFilePath);
                    content = ReplaceInString(content, dict);
                    System.IO.File.WriteAllText(fullFilePath, content);
                }
            }
        }

    }
}

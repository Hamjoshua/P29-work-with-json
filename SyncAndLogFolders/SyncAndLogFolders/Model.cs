using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncAndLogFolders
{
    public class SyncDirectory
    {
        public DirectoryInfo Directory;
        public FileInfo[] OldFiles;
        public List<string> ChangedFiles;
        public FileInfo[] NewFiles
        {
            get
            {
                return Directory.GetFiles();
            }
        }

        public void ApplyChanges()
        {
            OldFiles = NewFiles;
            ChangedFiles = new List<string>();
        }

        public SyncDirectory(string path)
        {
            Directory = new DirectoryInfo(path);
            ApplyChanges();

        }

        public void SyncWith(SyncDirectory sourceDirectory, bool removeMissingItems, bool overwriteDuplicates)
        {
            StartLog();
            CopySourceFiles(sourceDirectory, overwriteDuplicates);

            if (removeMissingItems)
            {
                RemoveMissingFromSource(sourceDirectory);
            }
            LogManager.Shutdown();
        }

        private void RemoveMissingFromSource(SyncDirectory sourceDirectory)
        {
            foreach (FileInfo originalFile in OldFiles)
            {
                int counterOfSameFiles = sourceDirectory.OldFiles.Count(d => d.Name == originalFile.Name && d.Length == originalFile.Length);

                if (counterOfSameFiles == 0 && !ChangedFiles.Contains(originalFile.Name))
                {
                    File.Delete(originalFile.FullName);
                    WriteToLog("Removed file", originalFile.Name);
                }
            }
        }

        private void CopySourceFiles(SyncDirectory sourceDirectory, bool overwriteDuplicates)
        {
            foreach (FileInfo sourceFile in sourceDirectory.OldFiles)
            {
                FileInfo sameFile = OldFiles.FirstOrDefault(d => d.Name == sourceFile.Name);

                if (!overwriteDuplicates)
                {                    
                    if (sameFile != null)
                    {
                        if (sameFile.Length == sourceFile.Length)
                        {
                            continue;
                        }
                        var partsOfName = sourceFile.Name.Split('.');
                        string newFileName = $"{partsOfName[0]} (1).{partsOfName[1]}";
                        string newDuplicateFilePath = Path.Combine(Directory.FullName, newFileName);

                        if (sameFile.Length < sourceFile.Length)
                        {
                            sameFile.MoveTo(newDuplicateFilePath);
                            newDuplicateFilePath = Path.Combine(Directory.FullName, sourceFile.Name);
                        }
                        else if (sameFile.Length > sourceFile.Length)
                        {
                            string otherDuplicateFilePath = Path.Combine(sourceDirectory.Directory.FullName, newFileName);
                            sourceFile.MoveTo(otherDuplicateFilePath);
                        }
                        WriteToLog("Added file", sourceFile.Name);
                        sourceFile.CopyTo(newDuplicateFilePath);
                        continue;
                    }
                }

                string newFilePath = Path.Combine(Directory.FullName, sourceFile.Name);
                sourceFile.CopyTo(newFilePath, true);
                if (sameFile != null)
                {
                    if (sameFile.Length != sourceFile.Length)
                    {
                        WriteToLog("Changed file", sameFile.Name);
                        ChangedFiles.Add(sourceFile.Name);
                    }
                }
                else
                {
                    WriteToLog("Added file", sourceFile.Name);
                }
            }
        }

        private void StartLog()
        {
            LogManager.GetCurrentClassLogger().Info(Directory.Name);
        }

        private void WriteToLog(string operationText, string fileName)
        {
            Operation operation = new Operation(operationText, fileName);
            LogManager.GetCurrentClassLogger().WithProperty("operations", operation);
        }
    }
}

class Operation{
    public string type;
    public string file;

    public Operation(string someType, string someFile)
    {
        type = someType;
        file = someFile;
    }
}
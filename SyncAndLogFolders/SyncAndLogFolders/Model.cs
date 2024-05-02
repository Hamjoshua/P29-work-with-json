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
            CopySourceFiles(sourceDirectory, overwriteDuplicates);

            if (removeMissingItems)
            {
                RemoveMissingFromSource(sourceDirectory);
            }
        }

        private void RemoveMissingFromSource(SyncDirectory sourceDirectory)
        {
            foreach (FileInfo originalFile in OldFiles)
            {
                int counterOfSameFiles = sourceDirectory.OldFiles.Count(d => d.Name == originalFile.Name && d.Length == originalFile.Length);

                if (counterOfSameFiles == 0 && !ChangedFiles.Contains(originalFile.Name))
                {
                    File.Delete(originalFile.FullName);
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
                        ChangedFiles.Add(sourceFile.Name);
                    }
                }
            }
        }
    }
}

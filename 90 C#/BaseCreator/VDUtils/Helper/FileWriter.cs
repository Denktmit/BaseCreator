using System;
using System.Collections.Generic;
using System.Security;

namespace VDUtils.Helper {

  public class FileWriter {

    public string FolderPath { get; set; }
    public string FileName { get; set; }
    public string FileExtension { get; set; }
    public List<string> FileContent { get; set; }

    public FileWriter(string inFolderPath, string inFileName, string inFileExtension) {
      FolderPath = inFolderPath;
      FileName = inFileName;
      FileExtension = inFileExtension;
      FileContent = new List<string>();
    }

    // LongName: AddLineToFile
    public void Altf(string inLine) {
      if (FileContent == null) {
        throw new Exception("ERROR@FileWriter.Altf: FileContent was null!");
      }
      if (inLine == null) {
        throw new Exception("ERROR@FileWriter.Altf: Parameter inLine was null!");
      }
      FileContent.Add(inLine);
    }

    // Longname: AddMultipleLinesToFile
    public void Amltf(List<string> inLines) {
      if (FileContent == null) {
        throw new Exception("ERROR@FileWriter.Amltf: TempFileContent was null!");
      }
      if (inLines == null) {
        throw new Exception("ERROR@FileWriter.Amltf: Parameter inLines was null!");
      }
      FileContent.AddRange(inLines);
    }

    public void CreateFile() {
      FileManager.writeNewFileWithContent(FolderPath, FileName, FileExtension, FileContent, true);
    }

  }

}

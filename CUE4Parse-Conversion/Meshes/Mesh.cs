using System;
using System.Collections.Generic;
using System.IO;
using CUE4Parse_Conversion.Materials;

namespace CUE4Parse_Conversion.Meshes
{
    public class Mesh : ExporterBase
    {
        public readonly string FileName;
        public readonly byte[] FileData;
        public readonly List<MaterialExporter> Materials;

        public Mesh(string fileName, byte[] fileData, List<MaterialExporter> materials)
        {
            FileName = fileName;
            FileData = fileData;
            Materials = materials;
        }

        public override bool TryWriteToDir(DirectoryInfo baseDirectory, out string savedFilePath)
        {
            savedFilePath = string.Empty;
            if (!baseDirectory.Exists || FileData.Length <= 0) return false;

            var filePath = Path.Combine(baseDirectory.FullName, Path.GetFileName(FileName));
            File.WriteAllBytes(filePath, FileData);
            savedFilePath = filePath;
            return File.Exists(filePath);
        }

        public override bool TryWriteToZip(out byte[] zipFile)
        {
            throw new NotImplementedException();
        }

        public override void AppendToZip()
        {
            throw new NotImplementedException();
        }
    }
}
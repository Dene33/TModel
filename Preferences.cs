using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TModel
{
    public static class Preferences
    {
        public static Action Changed;

        // Folder for storing user settings, Fortnite mappings and other data for the software.
        public static string StorageFolder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TModel");

        public static string ExportsPath { get; } = Path.Combine(StorageFolder, "Exports");

        public static string SettingsFile { get; } = Path.Combine(StorageFolder, "Settings.settings");

        public static string MappingsFile { set; get; } = @"https://benbot.app/api/v1/mappings/++Fortnite+Release-19.01-CL-18489740-Windows_oo.usmap";

        public static string? GameDirectory { get; set; }

        public static bool? AutoLoad { set; get; }

        public static void Save()
        {
            CBinaryWriter Writer = new CBinaryWriter(File.Open(SettingsFile, FileMode.OpenOrCreate, FileAccess.ReadWrite));

            Writer.Write(GameDirectory);
            Writer.Write(AutoLoad ?? false);
            Writer.Write(MappingsFile);

            Writer.Close();
            if (Changed != null)
                Changed();
        }

        public static void Read()
        {
            if (File.Exists(SettingsFile))
            {
                CBinaryReader Reader = new CBinaryReader(File.Open(SettingsFile, FileMode.Open, FileAccess.Read));
                if (Reader.BaseStream.Length != 0)
                {
                    GameDirectory = Reader.ReadString();
                    AutoLoad = Reader.ReadBoolean();
                    MappingsFile = Reader.ReadString();
                }
                Reader.Close();
            }
        }
    }

    public class CBinaryWriter : BinaryWriter
    {
        public CBinaryWriter(Stream output) : base(output)
        {

        }

        public override void Write(string value)
        {
            Write((byte)value.Length);
            Write(value.ToCharArray());
        }
    }

    public class CBinaryReader : BinaryReader
    {
        public CBinaryReader(Stream input) : base(input)
        {

        }

        public override string ReadString()
        {
            int length = ReadByte();
            return new string(ReadChars(length));
        }
    }
}

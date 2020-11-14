using System.Collections.Generic;

namespace ArcExplorer
{
    public static class FileFormatInfo
    {
        public static IEnumerable<string> Extensions => iconKeyByExtension.Keys;

        private static readonly Dictionary<string, ApplicationStyles.Icon> iconKeyByExtension = new Dictionary<string, ApplicationStyles.Icon>()
        {
            { ".arc", ApplicationStyles.Icon.Document },
            { ".bfotf", ApplicationStyles.Icon.Document },
            { ".bfttf", ApplicationStyles.Icon.Document },
            { ".bin", ApplicationStyles.Icon.Document },
            { ".bntx", ApplicationStyles.Icon.Document },
            { ".csb", ApplicationStyles.Icon.Document },
            { ".eff", ApplicationStyles.Icon.Document },
            { ".fnv", ApplicationStyles.Icon.Document },
            { ".h264", ApplicationStyles.Icon.Document },
            { ".lc", ApplicationStyles.Icon.Document },
            { ".lvd", ApplicationStyles.Icon.Lvd },
            { ".msbt", ApplicationStyles.Icon.Document },
            { ".nro", ApplicationStyles.Icon.Document },
            { ".nuanmb", ApplicationStyles.Icon.Document },
            { ".nufxlb", ApplicationStyles.Icon.Document },
            { ".nuhlpb", ApplicationStyles.Icon.Document },
            { ".numatb", ApplicationStyles.Icon.MaterialSpecular },
            { ".numdlb", ApplicationStyles.Icon.Document },
            { ".numshb", ApplicationStyles.Icon.Document },
            { ".numshexb", ApplicationStyles.Icon.Document },
            { ".nus3audio", ApplicationStyles.Icon.Document },
            { ".nus3bank", ApplicationStyles.Icon.Document },
            { ".nus3conf", ApplicationStyles.Icon.Document },
            { ".nushdb", ApplicationStyles.Icon.Document },
            { ".nusktb", ApplicationStyles.Icon.Document },
            { ".nusrcmdlb", ApplicationStyles.Icon.Document },
            { ".nutexb", ApplicationStyles.Icon.Bitmap },
            { ".prc", ApplicationStyles.Icon.Document },
            { ".shpc", ApplicationStyles.Icon.Document },
            { ".shpcanim", ApplicationStyles.Icon.Document },
            { ".sli", ApplicationStyles.Icon.Document },
            { ".spt", ApplicationStyles.Icon.Document },
            { ".sqb", ApplicationStyles.Icon.Document },
            { ".stdat", ApplicationStyles.Icon.Document },
            { ".stprm", ApplicationStyles.Icon.Document },
            { ".svt", ApplicationStyles.Icon.Document },
            { ".tonelabel", ApplicationStyles.Icon.Document },
            { ".webm", ApplicationStyles.Icon.Document },
            { ".xmb", ApplicationStyles.Icon.Document },
        };

        private static readonly Dictionary<string, string> descriptionByExtension = new Dictionary<string, string>()
        {
            { ".arc", "SARC Archive" },
            { ".bfotf", "OpenType Font" },
            { ".bfttf", "TrueType Font" },
            { ".bin", "Binary Data" },
            { ".bntx", "Texture" },
            { ".csb", "Common Sound Table" },
            { ".eff", "Effects Container" },
            { ".fnv", "Fighter Number Volume" },
            { ".h264", "MPEG-4 Video" },
            { ".lc", "Compiled Lua Script" },
            { ".lvd", "Stage Level Data" },
            { ".msbt", "Message Text" },
            { ".nro", "Executable/lua2cpp Moveset" },
            { ".nuanmb", "Skeletal and Material Animation Data" },
            { ".nufxlb", "" },
            { ".nuhlpb", "Helper Bone Data" },
            { ".numatb", "Material Container" },
            { ".numdlb", "Model Data" },
            { ".numshb", "Mesh Data" },
            { ".numshexb", "Mesh Extension Data" },
            { ".nus3audio", "Sound Banks" },
            { ".nus3bank", "Sound Banks" },
            { ".nus3conf", "" },
            { ".nushdb", "Compiled Shader" },
            { ".nusktb", "Model Skeleton" },
            { ".nusrcmdlb", "" },
            { ".nutexb", "Texture" },
            { ".prc", "Parameters" },
            { ".shpc", "" },
            { ".shpcanim", "" },
            { ".sli", "Sound Label Info" },
            { ".spt", "Sound Priority Table" },
            { ".sqb", "Sequence Bank" },
            { ".stdat", "Stage Data" },
            { ".stprm", "Stage Parameters" },
            { ".svt", "Sound Volume Table" },
            { ".tonelabel", "Sound Entry Lookup Table" },
            { ".webm", "Video" },
            { ".xmb", "Rendering Data" },
        };

        public static string GetDescription(string extension)
        {
            if (descriptionByExtension.ContainsKey(extension))
                return descriptionByExtension[extension];

            return "";
        }

        public static ApplicationStyles.Icon GetFileIconKey(string extension)
        {
            if (iconKeyByExtension.ContainsKey(extension))
                return iconKeyByExtension[extension];
            else
                return ApplicationStyles.Icon.Document;
        }
    }
}

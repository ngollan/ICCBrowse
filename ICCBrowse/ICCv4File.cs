using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace ICCBrowse {
    struct ICCVersion {
        public uint Major { get; }
        public uint Minor { get; }
        public uint Patch { get; }
        uint Reserved { get; }

        public ICCVersion(byte[] raw) {
            Major = raw[0];
            Minor = (uint)(raw[1] & 0xf0) >> 4;
            Patch = (uint)(raw[1] & 0x0f);
            Reserved = 0;
        }

        public ICCVersion(uint maj, uint min, uint patch) {
            Major = maj;
            Minor = min;
            Patch = patch;
            Reserved = 0;
        }

        public new String ToString {
            get {
                return $"{Major}.{Minor}.{Patch}.{Reserved}";
            }
        }
    }

    class ICCv4File {
        public static HashSet<string> KnownProfileClasses { get; } = new HashSet<string> {
          "scnr", "mntr", "prtr", "link", "spac", "abst", "nmcl"
        };

        // Table 19 from ICCv4
        public static Dictionary<string, string> ColourSpaces = new Dictionary<string, string> {
            { "XYZ ", "nCIEXYZ or PCSXYZa" },
            { "Lab ", "CIELAB or PCSLABb" },
            { "Luv ", "CIELUV" },
            { "YCbr", "YCbCr" },
            { "Yxy ", "CIExy" },
            { "RGB ", "RGB" },
            { "GRAY", "Gray" },
            { "HSV ", "HSV" },
            { "HLS ", "HLS" },
            { "CMYK", "CMYK" },
            { "CMY ", "CMY" },
            { "2CLR", "2 colour" },
            { "3CLR", "3 colour" },
            { "4CLR", "4 colour" },
            { "5CLR", "5 colour" },
            { "6CLR", "6 colour" },
            { "7CLR", "7 colour" },
            { "8CLR", "8 colour" },
            { "9CLR", "9 colour" },
            { "ACLR", "10 colour" },
            { "BCLR", "11 colour" },
            { "CCLR", "12 colour" },
            { "DCLR", "13 colour" },
            { "ECLR", "14 colour" },
            { "FCLR", "15 colour" }
        };

        public static string ProfileSignature = "acsp";

        public ICCVersion Version { get; }
        List<ICCTag> tags;
        uint DataSize;
        string PreferredCMM;
        string ProfileClass;
        string DataColourSpace;
        string PCS;
        DateTimeOffset CreatedDate;
        string HexProfileID;

        public ICCv4File(string fileName) {
            FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read);
            Byte[] header = new Byte[128];
            int n = fs.Read(header, 0, 128);

            if (128 == n) {
                DataSize = ConsumeUInt32(new ArraySegment<byte>(header, 0, 4));

                PreferredCMM = Encoding.ASCII.GetString(header, 4, 4);
                Version = new ICCVersion(new ArraySegment<byte>(header, 8, 4).ToArray());

                if (Version.Major != 2 && Version.Major != 4) {
                    throw new ArgumentException($"Cannot understand a profile with version {Version.ToString}");
                }

                ProfileClass = Encoding.ASCII.GetString(header, 12, 4);

                if (!KnownProfileClasses.Contains(ProfileClass)) {
                    throw new ArgumentException($"Unknown profile class {ProfileClass}");
                }

                DataColourSpace = Encoding.ASCII.GetString(header, 16, 4);
                PCS = Encoding.ASCII.GetString(header, 20, 4);
                CreatedDate = ConsumeDateTimeNumber(new ArraySegment<byte>(header, 24, 12));

                var signature = Encoding.ASCII.GetString(header, 36, 4);
                if (signature != ProfileSignature) {
                    throw new ArgumentException($"Invalid signature \"{signature}\"");
                }

                HexProfileID = BitConverter.ToString(header, 84, 16).Replace("-", "").ToLower();
                if ("00000000000000000000000000000000" == HexProfileID) {
                    HexProfileID = null;
                }
            } else {
                Debug.WriteLine($"Expected at least 128 bytes, got {n}");
            }

            fs.Close();
        }

        private string CMMString {
            get {
                if (PreferredCMM.Length > 0 && PreferredCMM[0] != '\0') {
                    return PreferredCMM;
                } else {
                    return "(blank)";
                }
            }
        }

        public new string ToString {
            get {
                return $"Version {Version.ToString} {ReadableProfileClass} profile ({DataSize} bytes), created {CreatedDate}, checksum {HexProfileID}; CMM {CMMString}; A: {ColourSpaces[DataColourSpace]}, B: {ColourSpaces[PCS]}";
            }
        }

        public string ReadableProfileClass {
            get {
                switch (ProfileClass) {
                    case "scnr":
                        return "input device";
                    case "mntr":
                        return "display device";
                    case "prtr":
                        return "output device";
                    case "link":
                        return "device link";
                    case "spac":
                        return "color space";
                    case "abst":
                        return "abstract";
                    case "nmcl":
                        return "named color";
                    default:
                        return $"unknown class \"{ProfileClass}\"";
                }
            }
        }

        private uint ConsumeUInt32(IEnumerable<byte> _input) {
            var input = _input.ToArray();

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(input);
            }

            return BitConverter.ToUInt32(input, 0);
        }

        private int ConsumeUInt16(IEnumerable<byte> _input) {
            var input = _input.ToArray();

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(input);
            }

            return BitConverter.ToUInt16(input, 0);
        }

        private DateTimeOffset ConsumeDateTimeNumber(IEnumerable<byte> _input) {
            var input = _input.ToArray();

            var yy = ConsumeUInt16(new ArraySegment<byte>(input, 0, 2));
            var mm = ConsumeUInt16(new ArraySegment<byte>(input, 2, 2));
            var dd = ConsumeUInt16(new ArraySegment<byte>(input, 4, 2));
            var h = ConsumeUInt16(new ArraySegment<byte>(input, 6, 2));
            var m = ConsumeUInt16(new ArraySegment<byte>(input, 8, 2));
            var s = ConsumeUInt16(new ArraySegment<byte>(input, 10, 2));

            return new DateTimeOffset(yy, mm, dd, h, m, s, new TimeSpan(0));
        }
    }
}

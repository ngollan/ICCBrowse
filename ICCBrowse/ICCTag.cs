using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICCBrowse {
    class ICCTagStub {
        public string Signature { get; }
        public uint Offset { get; }
        public uint Size { get; }

        public ICCTagStub(IEnumerable<byte> _input) {
            var input = _input.ToArray();

            Signature = Encoding.ASCII.GetString(input, 0, 4);
            Offset = ICCv4File.ConsumeUInt32(new ArraySegment<byte>(input, 4, 4));
            Size = ICCv4File.ConsumeUInt32(new ArraySegment<byte>(input, 8, 4));
        }

        public new string ToString {
            get {
                return $"\"{Signature}\": {Size} bytes from {Offset}";
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib.Id3v2;

namespace PlayGround.Properties
{
    internal record NcmFileInfo
    {
        public Tag Tags { get; set; }

        public MemoryStream DataStream { get; set; }

        public string Ext { get; set; }
    }
}

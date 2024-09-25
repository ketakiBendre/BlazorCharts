using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Quantaflare.Data
{
    public class EnergyStream
    {
        public string? Streams { get; set; } = string.Empty;
        public string? Fields { get; set; } = string.Empty;

        public string? agr { get; set; } = string.Empty;

        public override bool Equals(object obj)
        {
            if (obj is EnergyStream eStream)
            {
                return (Streams == eStream.Streams && Fields == eStream.Fields && agr == eStream.agr);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (Streams?.GetHashCode() ?? 0);
            hash = hash * 31 + (Fields?.GetHashCode() ?? 0);
            hash = hash * 31 + (agr?.GetHashCode() ?? 0);
            return hash;
        }
    }
}

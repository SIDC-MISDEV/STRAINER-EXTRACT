using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STRAINER_EXTRACT.Model
{
    public class Prefix
    {
        public string ObjectType { get; set; }
        public string ObjectPrefix { get; set; }
    }

    public class Branch
    {
        public string BranchName { get; set; }
        public string WarehouseCode { get; set; }
        public string BranchCodeNumber { get; set; }
    }
}

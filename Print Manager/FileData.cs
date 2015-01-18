using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Print_Manager
{
    [Serializable]
    public class FileData
    {
        public string Name { get; set; }
        public bool isPrint { get; set; }
        public string FullName { get; set; }
        public string Status
        {
            get
            {
                if (this.isPrint)
                {
                    return "Đã In";
                }else
                {
                    return "Chưa In";
                }
            }
        }
    }
}

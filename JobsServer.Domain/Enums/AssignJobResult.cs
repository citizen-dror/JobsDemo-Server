using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsServer.Domain.Enums
{
    public enum AssignJobResult
    {
        Success,
        NotFound,
        Offline,
        AtCapacity
    }
}

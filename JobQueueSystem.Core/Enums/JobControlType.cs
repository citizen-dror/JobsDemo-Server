﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobQueueSystem.Core.Enums
{
    public enum JobControlType
    {
        AssignJobToWorker,
        StopJob,
        RestartJob
    }
}

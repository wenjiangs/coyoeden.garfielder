﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garfielder.ViewModels
{
    public class VMHome:VMBase
    {
        public int TopicNum { get; set; }

        public List<VMTopic> Topics { get; set; }
    }
}
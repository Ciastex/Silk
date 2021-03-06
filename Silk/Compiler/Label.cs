﻿using System.Collections.Generic;

namespace Silk.Compiler
{
    internal class Label
    {
        public string Name { get; set; }
        public int? IP { get; set; }
        public List<int> FixUpIPs { get; private set; }

        public Label(string name, int? ip = null)
        {
            Name = name;
            IP = ip;
            FixUpIPs = new List<int>();
        }
    }
}

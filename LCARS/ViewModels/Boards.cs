﻿using System.ComponentModel;

namespace LCARS.ViewModels
{
    public enum Boards
    {
        [Description("Environments")] Environment = 1,
        [Description("Builds")] Build = 2,
        Issues = 3,
        Deployments = 4,
        Backlog = 5,
        Git = 6,
        External = 7,
        [Description("Git/Shipped")] GitShipped = 8
    }
}
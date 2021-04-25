﻿using Unity.Entities;

namespace DeepMiners.Data
{
    public struct DrillPower : IComponentData
    {
        public float Amount;
        public float Rate;
        public int Hits;

    }
}
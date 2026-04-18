using System;

namespace Project.Scripts.BetManagement.Chip
{
    [Serializable]
    public struct Chip : IEquatable<Chip>
    {
        public string Id;
        public int Value;

        public bool Equals(Chip other)
        {
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
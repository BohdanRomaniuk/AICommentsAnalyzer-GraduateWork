using System;

namespace parser.Models
{
    [Serializable]
    public class Country : IEquatable<Country>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Country()
        {
        }

        public Country(int _id, string _name)
        {
            Id = _id;
            Name = _name;
        }

        public bool Equals(Country other)
        {
            return Name.Equals(other.Name);
        }
    }
}

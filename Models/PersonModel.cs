using System.Diagnostics.CodeAnalysis;

namespace  Task_5.Models
{
    public class PersonModel
    {
        public int Num { get; set; }
        public string Id { get; set; }
        public string Fullname { get; set; }
        public string FullAddress { get; set; }
        public string PhoneNumber { get; set; }

        public override string? ToString()
        {
            return string.Join(", ", Id, Fullname, FullAddress, PhoneNumber);
        }
    }

    
}

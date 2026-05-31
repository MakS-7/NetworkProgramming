using System;
using System.Collections.Generic;
using System.Text;

namespace ClientProgram
{
    public class Person
    {
        public required int Id { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public int Age { get; set; }
        public required string Gender { get; init; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Part1.Models
{
    public class Volunteer
    {
            public int Id { get; set; }
            public string Fullname {
            get => Fullname;
            set => Fullname = value; 
            }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Skills { get; set; }
            public string Availability { get; set; }
            public DateTime ApplicationDate { get; set; } = DateTime.Now;
        }
    }
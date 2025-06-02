using System.ComponentModel.DataAnnotations;

namespace SportsCenterReservation.Models
{
    // Reprezinta un utilizator al aplicatiei
    public class User
    {
        public int Id { get; set; } // Identificator unic

        // Lista de rezervari asociate utilizatorului
        public ICollection<Rezervare> Rezervari { get; set; }

        // Credentiale de autentificare
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } 

        // Rolul utilizatorului (ex: Admin, User)
        [Required]
        public string Role { get; set; }
    }
}
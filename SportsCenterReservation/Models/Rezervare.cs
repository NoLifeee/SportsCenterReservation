using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace SportsCenterReservation.Models
{
    public class Rezervare
    {
        public int Id { get; set; }

        // Numele clientului (validat pentru lungime minima)
        [Required(ErrorMessage = "Numele clientului este obligatoriu.")]
        [MinLength(3, ErrorMessage = "Numele clientului trebuie să aibă cel puțin 3 caractere.")]
        public string Client { get; set; }

        // Data rezervarii (validata sa nu fie in trecut)
        [Required(ErrorMessage = "Data este obligatorie.")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(Rezervare), "ValidateData")]
        public DateTime Data { get; set; }

        // Ora rezervarii (format HH:mm)
        [Required(ErrorMessage = "Ora este obligatorie.")]
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Ora trebuie să fie în format HH:mm.")]
        public string Ora { get; set; }

        // Durata rezervarii in ore (1-4 ore)
        [Required(ErrorMessage = "Durata este obligatorie.")]
        [Range(1, 4, ErrorMessage = "Durata trebuie să fie între 1 și 4 ore.")]
        public int DurataOre { get; set; }

        // Legatura cu Serviciu (cheie straina)
        [Required(ErrorMessage = "Trebuie să selectați un serviciu.")]
        public int ServiciuId { get; set; }

        [ValidateNever] // Ignora validarea la binding
        public Serviciu Serviciu { get; set; }

        // Legatura cu User (cheie straina)
        [Required]
        public int UserId { get; set; }

        [ValidateNever]
        public User User { get; set; }

        // Ordinea de afisare in interfata
        [Required]
        public int Ordine { get; set; }

        // Verifica daca data este cel putin ziua de maine
        public static ValidationResult ValidateData(DateTime data, ValidationContext context)
        {
            if (data < DateTime.Today)
            {
                return new ValidationResult("Data trebuie să fie cel puțin ziua de mâine.");
            }
            return ValidationResult.Success;
        }
    }
}
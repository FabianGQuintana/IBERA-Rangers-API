using IberaReclamos.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace IberaReclamos.Domain.Entities
{
    public class Turist : BaseEntity
    {
        public required string Menssage {  get; set; }

        public required int Evaluation { get; set; }

        public string? Email { get; set; }

        public required IberaPortales Portal {  get; set; }

        //Foto. Preguntar a la IA como trabajarlo desde aqui si vamos a utilizar CloudFlare.

    }
}

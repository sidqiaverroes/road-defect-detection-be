using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace rdds.api.Dtos.AccessType
{
    public class UpdateAccessTypeDto : IValidatableObject
    {
        public string Name { get; set; }
        public List<string> Accesses { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Accesses == null || !Accesses.Contains("read"))
            {
                yield return new ValidationResult("The AccessType must contain at least a 'read' access.",
                                                new[] { nameof(Accesses) });
            }
        }
    }
}
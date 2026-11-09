using PetMate.Application.Validation;
using PetMate.Domain.Pets;

namespace PetMate.Application.Pets;

/// <summary>
/// Validación de los datos de una mascota (pm-039#CA-01), compartida por los validadores de crear y actualizar
/// (pm-040, pm-043). Devuelve un error por campo, en español y sin repetir el valor recibido.
/// </summary>
public static class PetValidation
{
    public static IReadOnlyList<ValidationFailure> Check(PetDetails? details, DateOnly today)
    {
        var failures = new List<ValidationFailure>();
        if (details is null)
        {
            failures.Add(new ValidationFailure("name", "Los datos de la mascota son obligatorios."));
            return failures;
        }

        if (!Pet.IsValidName(details.Name))
        {
            failures.Add(new ValidationFailure("name", $"El nombre es obligatorio y debe tener de 1 a {Pet.MaxNameLength} caracteres."));
        }

        if (!Pet.IsValidSpecies(details.Species))
        {
            failures.Add(new ValidationFailure("species", "La especie debe ser Perro o Gato."));
        }

        if (!Pet.IsValidBreed(details.Breed))
        {
            failures.Add(new ValidationFailure("breed", $"La raza no debe exceder {Pet.MaxBreedLength} caracteres."));
        }

        if (!Pet.IsValidBirthDate(details.BirthDate, today))
        {
            failures.Add(new ValidationFailure("birthDate", $"La fecha de nacimiento no puede ser futura ni de hace más de {Pet.MaxAgeInYears} años."));
        }

        if (!Pet.IsValidSex(details.Sex))
        {
            failures.Add(new ValidationFailure("sex", "El sexo debe ser Male, Female o Unknown."));
        }

        if (!Pet.IsValidPhotoUrl(details.PhotoUrl))
        {
            failures.Add(new ValidationFailure("photoUrl", "La fotografía debe ser una URL HTTPS válida."));
        }

        return failures;
    }
}

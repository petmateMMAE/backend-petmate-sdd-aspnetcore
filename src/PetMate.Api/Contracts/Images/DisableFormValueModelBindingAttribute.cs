using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PetMate.Api.Contracts.Images;

/// <summary>
/// Quita los proveedores de valores de formulario en las rutas de carga de imágenes (pm-080).
/// <para>
/// MVC lee el formulario completo antes de entrar al controlador en cuanto el tipo de contenido es
/// <c>multipart/form-data</c>, aunque la acción no reciba ningún campo. Eso deja el cuerpo ya consumido para
/// <see cref="PhotoUploadReader"/> y, peor, **escribe los archivos grandes en un temporal del disco**, que es
/// justo lo que prohíbe pm-079#CA-02. Con este filtro el cuerpo llega intacto y la imagen va directa al Blob.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var factories = context.ValueProviderFactories;
        for (var i = factories.Count - 1; i >= 0; i--)
        {
            if (factories[i] is FormValueProviderFactory or FormFileValueProviderFactory or JQueryFormValueProviderFactory)
            {
                factories.RemoveAt(i);
            }
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        // No hay nada que restaurar: la colección de proveedores vive solo durante esta petición.
    }
}

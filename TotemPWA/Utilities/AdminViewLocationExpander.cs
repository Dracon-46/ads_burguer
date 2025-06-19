using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor;

namespace TotemPWA.Utilities // Certifique-se que o namespace corresponde à sua pasta Utilities
{
    public class AdminViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // Não precisamos de valores personalizados para este expander
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            // Verifica se o controlador é um controlador de MVC
            if (context.ActionContext.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                // Verifica se o controlador está na namespace de Admin
                if (controllerActionDescriptor.ControllerTypeInfo.Namespace != null &&
                    controllerActionDescriptor.ControllerTypeInfo.Namespace.StartsWith("TotemPWA.Controllers.Admin"))
                {
                    // Adiciona os caminhos personalizados para as Views de Admin
                    // {0} é o nome da Ação (ex: Create), {1} é o nome do Controlador (ex: Category)
                    yield return "/Views/Admin/{1}/{0}.cshtml";      // Ex: /Views/Admin/Category/Create.cshtml
                    yield return "/Views/Admin/Shared/{0}.cshtml";   // Para Views compartilhadas dentro de Admin
                }
            }

            // Retorna os caminhos de busca padrão do View Engine (importante para Views não-Admin)
            foreach (var location in viewLocations)
            {
                yield return location;
            }
        }
    }
}
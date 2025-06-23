using Microsoft.AspNetCore.Mvc.Rendering; // Necessário para SelectListItem
using TotemPWA.Models; // Necessário para a classe Employee e Client

namespace TotemPWA.ViewModels
{
    public class EmployeeViewModel
    {
        // A instância do funcionário que será criada ou editada
        public Employee Employee { get; set; }

        // Lista de clientes para popular o dropdown no formulário (para associar o funcionário a um cliente)
        public List<SelectListItem> Clients { get; set; } = new List<SelectListItem>();

        public EmployeeViewModel()
        {
            // Inicializa Employee para evitar NullReferenceException em novos formulários
            Employee = new Employee();
        }
    }
}
using System.Linq;
using System.Web.Mvc;
using Practica3.EF;

namespace Practica3.Controllers
{
    public class ConsultaController : Controller
    {
        private readonly PracticaS12Entities db = new PracticaS12Entities();

        // GET: Consulta
        public ActionResult Index()
        {
            // Orden: primero los pendientes Pendiente
            var lista = db.Principal
                          .OrderBy(p => p.Estado != "Pendiente")
                          .ThenBy(p => p.Id_Compra)
                          .ToList();

            return View("~/Views/Sistema/Consulta.cshtml", lista);
        }
    }
}

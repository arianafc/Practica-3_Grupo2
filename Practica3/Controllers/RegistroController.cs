using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Practica3.EF;

namespace Practica3.Controllers
{
    public class RegistroController : Controller
    {
        private readonly PracticaS12Entities db = new PracticaS12Entities();

        // GET: Registro
        public ActionResult Index()
        {
            CargarComprasPendientes();
            return View("~/Views/Sistema/Registro.cshtml");
        }

        // POST: Registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(long Id_Compra, decimal Monto)
        {
            CargarComprasPendientes(Id_Compra);

            var compra = db.Principal.FirstOrDefault(p => p.Id_Compra == Id_Compra);
            if (compra == null)
            {
                ModelState.AddModelError("", "Compra no encontrada.");
                return View("~/Views/Sistema/Registro.cshtml");
            }

            if (Monto <= 0)
            {
                ModelState.AddModelError("Monto", "El abono debe ser mayor a 0.");
                return View("~/Views/Sistema/Registro.cshtml");
            }

            if (Monto > compra.Saldo)
            {
                ModelState.AddModelError("Monto", "El abono no puede ser mayor al saldo.");
                return View("~/Views/Sistema/Registro.cshtml");
            }

            // Registrar abonos realizados
            var abono = new Abonos
            {
                Id_Compra = compra.Id_Compra,
                Monto = Monto,
                Fecha = DateTime.Now
            };
            db.Abonos.Add(abono);

            // Actualizar saldo y estado cada vez que se realiza un abono
            compra.Saldo = Math.Round(compra.Saldo - Monto, 5);
            if (compra.Saldo == 0m)
                compra.Estado = "Cancelado";

            db.SaveChanges();

            return RedirectToAction("Index", "Consulta");
        }

        // Devuelve saldo y precio con punto decimal y 2 decimales, AJAX
        [HttpGet]
        public JsonResult GetSaldo(long id)
        {
            var data = db.Principal
                         .Where(p => p.Id_Compra == id)
                         .Select(p => new { p.Saldo, p.Precio })
                         .FirstOrDefault();

            if (data == null) return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                saldo = data.Saldo.ToString("F2", CultureInfo.InvariantCulture),
                precio = data.Precio.ToString("F2", CultureInfo.InvariantCulture)
            }, JsonRequestBehavior.AllowGet);
        }

        private void CargarComprasPendientes(long? seleccion = null)
        {
            var items = db.Principal
                          .Where(p => p.Estado == "Pendiente")
                          .OrderBy(p => p.Id_Compra)
                          .Select(p => new
                          {
                              p.Id_Compra,
                              Texto = p.Descripcion
                          })
                          .ToList();

            ViewBag.ComprasPendientes = new SelectList(items, "Id_Compra", "Texto", seleccion);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}

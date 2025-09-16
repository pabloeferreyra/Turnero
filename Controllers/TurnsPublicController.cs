namespace Turnero.Controllers
{
    [AllowAnonymous]
    public class TurnsPublicController(IInsertTurnsServices insertTurns,
                           IGetMedicsServices getMedics,
                           IGetTimeTurnsServices getTimeTurns,
                           IHubContext<TurnsTableHub> hubContext,
                           IMemoryCache cache) : Controller
    {

        public async Task<ActionResult> Index()
        {
            List<MedicDto> medics = null;
            List<TimeTurn> time = null;

            medics = cache.Get<List<MedicDto>>("medics");
            time = cache.Get<List<TimeTurn>>("timeTurns");
            if (medics == null)
            {
                Task medicsTask = Task.Run(() =>
                {
                    medics = getMedics.GetCachedMedics().Result;
                });
                await medicsTask;
            }
            if (time == null)
            {

                Task timeTask = Task.Run(() =>
                {
                    time = getTimeTurns.GetCachedTimes().Result;
                });

                await timeTask;
            }
            ViewBag.Medics = new SelectList(medics, "Id", "Name");
            ViewBag.Time = new SelectList(time, "Id", "Time");
            return View();
        }

        [HttpPost]
        public async Task<StatusCodeResult> Create(TurnDTO turn)
        {
            if (!ModelState.IsValid) return this.BadRequest();
            try
            {
                turn.Date = DateTime.Today.ToString("dd/MM/yyyy");
                turn.TimeId = Guid.Parse("78496444-276d-4389-8b2f-f668c5350e3f");
                turn.Reason = "Turno espontáneo";
                var t = turn.Adapt<Turn>();
                await insertTurns.CreateTurnAsync(t);
                var medic = await getMedics.GetMedicById(turn.MedicId);
                await hubContext.Clients.User(medic.UserGuid).SendAsync("UpdateTableDirected", "La tabla se ha actualizado");

                return Ok();
            }
            catch
            {
                return Conflict();
            }
        }
    }
}

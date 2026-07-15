namespace Turnero.Controllers
{
    [AllowAnonymous]
    public class TurnsPublicController(IInsertTurnsServices insertTurns,
                           IGetMedicsServices getMedics,
                           IGetTimeTurnsServices getTimeTurns,
                           IHubContext<TurnsTableHub> hubContext,
                           IMemoryCache cache) : TurneroBaseController
    {

        public async Task<ActionResult> Index()
        {
            var medics = await GetCachedMedicsAsync();
            var time = await GetCachedTimeTurnsAsync();
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
                var timeTurns = await GetCachedTimeTurnsAsync();
                if (timeTurns.Count == 0) return BadRequest();
                turn.TimeId = timeTurns[0].Id;
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

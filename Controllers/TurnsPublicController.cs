using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Turnero.Hubs;
using Turnero.Models;
using Turnero.Services.Interfaces;

namespace Turnero.Controllers
{
    [AllowAnonymous]
    public class TurnsPublicController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        public IInsertTurnsServices _insertTurns;
        public ILogger<TurnsController> _logger;
        public IGetTurnsServices _getTurns;
        public IUpdateTurnsServices _updateTurns;
        public IGetMedicsServices _getMedics;
        public IGetTimeTurnsServices _getTimeTurns;
        private readonly IMapper mapper;
        private readonly IHubContext<TurnsTableHub> _hubContext;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        public IMemoryCache _cache;

        public TurnsPublicController(UserManager<IdentityUser> userManager,
                               ILogger<TurnsController> logger,
                               IInsertTurnsServices insertTurns,
                               IGetTurnsServices getTurns,
                               IUpdateTurnsServices updateTurns,
                               IGetMedicsServices getMedics,
                               IGetTimeTurnsServices getTimeTurns,
                               IMapper mapper,
                               IHttpClientFactory httpClientFactory,
                               IConfiguration config,
                               IHubContext<TurnsTableHub> hubContext,
                               IMemoryCache cache)
        {
            _userManager = userManager;
            _logger = logger;
            _insertTurns = insertTurns;
            _getTurns = getTurns;
            _updateTurns = updateTurns;
            _getMedics = getMedics;
            _getTimeTurns = getTimeTurns;
            this.mapper = mapper;
            _hubContext = hubContext;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }
        // GET: TurnsPublicController
        public async Task<ActionResult> Index()
        {
            List<MedicDto> medics = null;
            List<TimeTurn> time = null;

            medics = _cache.Get<List<MedicDto>>("medics");
            time = _cache.Get<List<TimeTurn>>("timeTurns");
            if (medics.Count == 0)
            {
                Task medicsTask = Task.Run(() =>
                {
                    medics = _getMedics.GetCachedMedics().Result;
                });
                await medicsTask;
            }
            if (time.Count == 0)
            {

                Task timeTask = Task.Run(() =>
                {
                    time = _getTimeTurns.GetCachedTimes().Result;
                });

                await timeTask;
            }
            ViewBag.Medics = new SelectList(medics, "Id", "Name");
            ViewBag.Time = new SelectList(time, "Id", "Time");
            return View();
        }

        // POST: TurnsPublicController/Create
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<StatusCodeResult> Create(TurnDTO turn)
        {
            if (!ModelState.IsValid) return this.BadRequest();
            try
            {
                turn.Date = DateTime.Today.ToString("dd/MM/yyyy");
                turn.TimeId = Guid.Parse("78496444-276d-4389-8b2f-f668c5350e3f");
                turn.Reason = "Turno espontáneo";
                var t = new Turn();
                t = mapper.Map(turn, t);
                await this._insertTurns.CreateTurnAsync(t);
                var medic = await this._getMedics.GetMedicById(turn.MedicId);
                await _hubContext.Clients.User(medic.UserGuid).SendAsync("UpdateTableDirected", "La tabla se ha actualizado"); ;

                return Ok();
            }
            catch
            {
                return Conflict();
            }


        }
    }
}

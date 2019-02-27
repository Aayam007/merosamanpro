using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kachuwa.Log;
using MeroSaman.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeroSaman.Core
{
    public class AddressController : Controller
    {
        private readonly ICounrtyService _counrtyService;
        private readonly IStateService _stateService;
        private readonly IDistrictService _districtService;
        private readonly IMunicipalityService _municipalityService;
        private readonly ILogger _logger;

        public AddressController(ICounrtyService counrtyService, IStateService stateService, 
            IDistrictService districtService, IMunicipalityService municipalityService, ILogger logger)
        {
            _counrtyService = counrtyService;
            _stateService = stateService;
            _districtService = districtService;
            _municipalityService = municipalityService;
            _logger = logger;
        }

        [Route("/state")]
        public async Task<JsonResult> State(int id)
        {
            try
            {
                var data = await _stateService.StateCrudService.GetListAsync("Where CountyId=@CountyId", new { CountyId = id });
                return Json(data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
          
        }
        [Route("/district")]
        public async Task<JsonResult> District(int id)
        {
            try
            {
                var data = await _districtService.DistrictCrudService.GetListAsync("Where CountyId=@CountyId", new { CountyId = id });
                return Json(data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
          
        }
        [Route("/municipality")]
        public async Task<JsonResult> Municipality(int id)
        {
            try
            {
                var data = await _municipalityService.MunicipalityCrudService.GetListAsync("Where CountyId=@CountyId", new { CountyId = id });
                return Json(data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }
        }       
    }
}
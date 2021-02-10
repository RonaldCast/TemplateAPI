using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using APIWithIdentity.DomainModel.Models;
using APIWithIdentity.DTOs;
using APIWithIdentity.Services;
using APIWithIdentity.Validators.ArtistValidator;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace APIWithIdentity.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class ArtistController : Controller
    {
        private readonly IArtistServices _artistService;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<ArtistController> _logger;
        
        public ArtistController(IArtistServices artistService, IMapper mapper, 
            IDistributedCache distributedCache,
                ILogger<ArtistController> logger
            )
        {
            _mapper = mapper;
            _artistService = artistService;
            _distributedCache = distributedCache;
            _logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArtistDTO>>> GetAllArtists()
        {
            var artists = await _artistService.GetAllArtists();
            var artistResources = _mapper.Map<IEnumerable<Artist>, IEnumerable<ArtistDTO>>(artists);
            return Ok(artistResources);
        }
        
        //[ApiVersion("1.0")]
        [MapToApiVersion("2.0")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArtistDTO>>> GetAllArtiistsV2()
        {
            var artists = await _artistService.GetAllArtists();
            var artistResources = _mapper.Map<IEnumerable<Artist>, IEnumerable<ArtistDTO>>(artists);
            return Ok(artistResources);
        }
        
        [HttpPost]
       // [Authorize("OnlyTest")]
        public async Task<ActionResult<ArtistDTO>> CreateArtist([FromBody] SaveArtist saveArtistResource)
       {
           var artistToCreate = _mapper.Map<SaveArtist, Artist>(saveArtistResource);

            var newArtist = await _artistService.CreateArtist(artistToCreate);

            var artist = await _artistService.GetArtistById(newArtist.Id);

            var artistResource = _mapper.Map<Artist, ArtistDTO>(artist);

            return Ok(artistResource);
        }

       [HttpGet("searchForName")]
       public async Task<ActionResult<ResponseMessage<List<Artist>>>> GetArtistByName([FromQuery] string name)
       {
           
           var cacheKey = name.ToLower();

           List<Artist> artists;
           string serializeArtist;

           var encodeArtist = await _distributedCache.GetAsync(cacheKey);

           if (encodeArtist != null)
           {
               serializeArtist = Encoding.UTF8.GetString(encodeArtist);
               artists = JsonConvert.DeserializeObject<List<Artist>>(serializeArtist);
               _logger.LogInformation(LogCustomeMessage.ReadMessage("APIWithIdentity", 
                   "ArtistController", "GetArtistByName","Get data from Redis cache"));
           }
           else
           {
               
               artists = await _artistService.GetArtistsByNameAsync(name);
               serializeArtist = JsonConvert.SerializeObject(artists);
               encodeArtist = Encoding.UTF8.GetBytes(serializeArtist);
               var options = new DistributedCacheEntryOptions()
                   .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                   .SetAbsoluteExpiration(DateTime.Now.AddHours(6));

               await _distributedCache.SetAsync(cacheKey, encodeArtist, options);
           }

           return Ok(new ResponseMessage<List<Artist>> { Response = artists});



       }
       
       

    }
}
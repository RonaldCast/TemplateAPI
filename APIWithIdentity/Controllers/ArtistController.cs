using System.Collections.Generic;
using System.Threading.Tasks;
using APIWithIdentity.DomainModel.Models;
using APIWithIdentity.DTOs;
using APIWithIdentity.Services.ArtistS;
using APIWithIdentity.Validators.ArtistValidator;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        
        public ArtistController(IArtistServices artistService, IMapper mapper)
        {
            this._mapper = mapper;
            this._artistService = artistService;
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
            var validator = new SaveArtistValidator();
            var validationResult = await validator.ValidateAsync(saveArtistResource);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors); // this needs refining, but for demo it is ok

            var artistToCreate = _mapper.Map<SaveArtist, Artist>(saveArtistResource);

            var newArtist = await _artistService.CreateArtist(artistToCreate);

            var artist = await _artistService.GetArtistById(newArtist.Id);

            var artistResource = _mapper.Map<Artist, ArtistDTO>(artist);

            return Ok(artistResource);
        }
       
       

    }
}
using Application.ResponseCode;
using ApplicationLayer.DTO.MovieManagement;
using AutoMapper;
using DomainLayer.Entities;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationLayer.Services.MovieManagement
{
    public interface IActorService
    {
        Task<IActionResult> ViewActors();
        Task<IActionResult> CreateActor(ActorCreateDto dto);
    }

    public class ActorService : IActorService
    {
        private readonly IGenericRepository<Actor> _actorRepo;
        private readonly IMapper _mapper;

        public ActorService(IGenericRepository<Actor> actorRepo, IMapper mapper)
        {
            _actorRepo = actorRepo;
            _mapper = mapper;
        }

        public async Task<IActionResult> ViewActors()
        {
            var actors = await _actorRepo.WhereAsync(a => a.IsActive);
            var result = _mapper.Map<List<ActorDto>>(actors);
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> CreateActor(ActorCreateDto dto)
        {
            if(string.IsNullOrWhiteSpace(dto.Name))
                return ErrorResp.BadRequest("Actor name is required");

            // check duplicate
            var exist = await _actorRepo.FirstOrDefaultAsync(a => a.Name.ToLower() == dto.Name.ToLower());
            if(exist != null)
                return ErrorResp.BadRequest("Actor already exists");

            var actor = _mapper.Map<Actor>(dto);
            actor.IsActive = true;
            await _actorRepo.CreateAsync(actor);
            return SuccessResp.Created(_mapper.Map<ActorDto>(actor));
        }
    }

    public interface IDirectorService
    {
        Task<IActionResult> ViewDirectors();
        Task<IActionResult> CreateDirector(DirectorCreateDto dto);
    }

    public class DirectorService : IDirectorService
    {
        private readonly IGenericRepository<Director> _directorRepo;
        private readonly IMapper _mapper;

        public DirectorService(IGenericRepository<Director> directorRepo, IMapper mapper)
        {
            _directorRepo = directorRepo;
            _mapper = mapper;
        }

        public async Task<IActionResult> ViewDirectors()
        {
            var directors = await _directorRepo.WhereAsync(d => d.IsActive);
            var result = _mapper.Map<List<DirectorDto>>(directors);
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> CreateDirector(DirectorCreateDto dto)
        {
            if(string.IsNullOrWhiteSpace(dto.Name))
                return ErrorResp.BadRequest("Director name is required");

            var exist = await _directorRepo.FirstOrDefaultAsync(d => d.Name.ToLower() == dto.Name.ToLower());
            if(exist != null)
                return ErrorResp.BadRequest("Director already exists");

            var director = _mapper.Map<Director>(dto);
            director.IsActive = true;
            await _directorRepo.CreateAsync(director);
            return SuccessResp.Created(_mapper.Map<DirectorDto>(director));
        }
    }
} 
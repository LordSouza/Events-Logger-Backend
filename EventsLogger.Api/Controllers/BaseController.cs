using AutoMapper;
using EventsLogger.DataService.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventsLogger.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{

    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IMapper _mapper;

    public BaseController(
        IUnitOfWork unitOfWork,
         IMapper mapper)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

}

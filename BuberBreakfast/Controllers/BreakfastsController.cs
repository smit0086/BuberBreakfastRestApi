using System.Net.NetworkInformation;
using BuberBreakfast.Contracts.Breakfast;
using BuberBreakfast.Models;
using BuberBreakfast.ServiceErrros;
using BuberBreakfast.Services.Breakfasts;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
namespace BuberBreakfast.Controllers;

public class BreakfastsController : ApiController {
    private readonly IBreakfastService _breakfastService;

    public BreakfastsController(IBreakfastService breakfastService){
        _breakfastService = breakfastService;
    }

    [HttpPost]
    public IActionResult CreateBreakfast(CreateBreakfastRequest request){
        ErrorOr<Breakfast> requestToBreakfastResult = Breakfast.Create(
            request.Name,
            request.Description,
            request.StartDateTime,
            request.EndDateTime,
            request.Savory,
            request.Sweet
        );
        if(requestToBreakfastResult.IsError){
            return Problem(requestToBreakfastResult.Errors);
        }
        var breakfast = requestToBreakfastResult.Value;
        ErrorOr<Created> createBreakfastResult =  _breakfastService.CreateBreakfast(breakfast);
        return createBreakfastResult.Match(
            created => CreatedAtGetBreakfast(breakfast),
            errors => Problem(errors)
        );
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetBreakfast(Guid id){
        ErrorOr<Breakfast> getBreakfastResult = _breakfastService.GetBreakfast(id);
        return getBreakfastResult.Match(
            breakfast => Ok(MapBreakfastResponse(breakfast)), 
            errors => Problem(errors)
        );
    }

    [HttpPut("{id:guid}")]
    public IActionResult UpsertBreakfaste(Guid id, UpsertBreakfastRequest request){
        var requestToBreakfastResult = Breakfast.Create(
            request.Name,
            request.Description,
            request.StartDateTime,
            request.EndDateTime,
            request.Savory,
            request.Sweet,
            id
        );

        if(requestToBreakfastResult.IsError){
            return Problem(requestToBreakfastResult.Errors);
        }

        var breakfast = requestToBreakfastResult.Value;
        ErrorOr<UpsertedBreakfast> upsertedResult = _breakfastService.UpsertBreakfast(breakfast);
        return upsertedResult.Match(
            upserted => upserted.IsNewlyCreated ? CreatedAtGetBreakfast(breakfast) : NoContent(),
            errors => Problem(errors)
        );
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteBreakfast(Guid id){
        ErrorOr<Deleted> deletedResult = _breakfastService.DeleteBreakfast(id);
        return deletedResult.Match(
            deleted => NoContent(),
            errors => Problem(errors)
        );
    }

     private static BreakfastResponse MapBreakfastResponse(Breakfast breakfast){
        return new BreakfastResponse(
            breakfast.Id,
            breakfast.Name,
            breakfast.Description,
            breakfast.StartDateTime,
            breakfast.EndDateTime,
            breakfast.LastModifiedDateTime,
            breakfast.Savory,
            breakfast.Sweet
        );
    }

    private CreatedAtActionResult CreatedAtGetBreakfast(Breakfast breakfast){
        return CreatedAtAction(
            nameof(GetBreakfast),
            new {id = breakfast.Id},
            value: MapBreakfastResponse(breakfast)
        );
    }
}
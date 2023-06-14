using CRUDDemmo.Controllers;
using CRUDDemmo.Filters.ResourceFilters;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO;

namespace CRUDDemmo.Filters.ActionFilters
{
    public class PersonsCreateAndEditPostActionFilter : IAsyncActionFilter
    {
        private readonly ICountriesGetterService _countriesService;
        private readonly ILogger<PersonsCreateAndEditPostActionFilter> _logger;
        public PersonsCreateAndEditPostActionFilter(ICountriesGetterService countriesService, ILogger<PersonsCreateAndEditPostActionFilter> logger)
        {
            _countriesService = countriesService;
            _logger = logger;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.Controller is PersonsController personsController)
                if (!personsController.ModelState.IsValid)
                {
                    List<CountryResponse> countries = await _countriesService.GetAllCountries();
                    personsController.ViewBag.Countries = countries.Select(temp =>
                 new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

                    var personRequest = context.ActionArguments["personRequest"];
                    personsController.ViewBag.Errors = personsController.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    context.Result = personsController.View(personRequest);
                }
                else { 
                await next();
                }
            else
            {
                await next();
            }

            _logger.LogInformation("In after logic of PersonsCreateAndEdit Action filter");
        }
    }
}

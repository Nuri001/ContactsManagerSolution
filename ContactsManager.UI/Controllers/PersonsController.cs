using CRUDDemmo.Filters;
using CRUDDemmo.Filters.ActionFilters;
using CRUDDemmo.Filters.AuthorizationFilter;
using CRUDDemmo.Filters.ExceptionFilters;
using CRUDDemmo.Filters.ResourceFilters;
using CRUDDemmo.Filters.ResultFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using static CRUDDemmo.Filters.ActionFilters.ResponseHeaderActionFilter;

namespace CRUDDemmo.Controllers
{
    [Route("[controller]")]
    [ResponseHeaderFilterFactory("My-Key-From-Controller", "My-Value-From-Controller", 3)]
    [TypeFilter(typeof(HandleExceptionFilter))]
    [TypeFilter(typeof(PersonAlwaysRunResultFilter))]
    public class PersonsController : Controller
    {
        //private dields
        private readonly IPersonsGetterService _personsGetterService;
        private readonly IPersonsAdderService _personsAdderService;
        private readonly IPersonsSorterService _personsSorterService;
        private readonly IPersonsDeleterService _personsDeleterService;
        private readonly IPersonsUpdaterService _personsUpdaterService;

        private readonly ICountriesGetterService _countriesService;
		private readonly ILogger<PersonsController> _logger;

        //constructor
        public PersonsController(IPersonsGetterService personsGetterService, IPersonsAdderService personsAdderService, IPersonsDeleterService personsDeleterService, IPersonsUpdaterService personsUpdaterService, IPersonsSorterService personsSorterService, ICountriesGetterService countriesService, ILogger<PersonsController> logger)
        {
            _personsGetterService = personsGetterService;
            _personsAdderService = personsAdderService;
            _personsUpdaterService = personsUpdaterService;
            _personsDeleterService = personsDeleterService;
            _personsSorterService = personsSorterService;

            _countriesService = countriesService;
            _logger = logger;
        }

        [Route("[action]")]
        [Route("/")]
		[ServiceFilter(typeof(PersonsListActionFilter),Order =4)]
        //TypeFilter[(typeof(ResponseHeaderActionFilter),Arguments =new object[] {"X-Custom-Key","Custom-Value" ,1}, Order =1)]
        [ResponseHeaderFilterFactory("MyKey-FromAction", "MyValue-From-Action", 1)]
        [TypeFilter(typeof(PersonsListResultFilter))]
        [SkipFilter]
        public async Task<IActionResult> Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
			_logger.LogInformation("Index action methot of personsController");
			_logger.LogDebug($"searchBy: {searchBy}, searchString: {searchString}, sortBy: {sortBy}");

            
            List<PersonResponse> persons = await _personsGetterService.GetFilteredPersons(searchBy, searchString);
           
            //sort
            List<PersonResponse> sortedPersons = await _personsSorterService.GetSortedPersons(persons, sortBy, sortOrder);
           
            return View(sortedPersons);
        }

        //Executes when the user clickss on "Create Person" htperlink
        [Route("[action]")]
        [HttpGet]
        [ResponseHeaderFilterFactory("X-Custom-Key", "Custom-Value", 4)]
        public async Task<IActionResult> Create() {

            List<CountryResponse> countries= await _countriesService.GetAllCountries();   
            ViewBag.Countries = countries.Select(temp =>
                new SelectListItem()
                {
                    Text = temp.CountryName,
                    Value = temp.CountryID.ToString()
                }        
            );

            return View();
        }

        [Route("[action]")]
        [HttpPost]
		[TypeFilter(typeof(PersonsCreateAndEditPostActionFilter))]
        [TypeFilter(typeof(FeatureDisabledResourceFilter), Arguments =new object[] {false })]
        public async Task<IActionResult> Create(PersonAddRequest personRequest)
        {
			//call the service method
			PersonResponse personResponse = await _personsAdderService.AddPerson(personRequest);

			//navigate to Index() action method (it makes another get request to "persons/index"
			return RedirectToAction("Index", "Persons");
		}

		[HttpGet]
		[Route("[action]/{personID}")] //Eg: /persons/edit/1
        [TypeFilter(typeof(TokenResultFilter))]
        public async Task<IActionResult> Edit(Guid personID)
		{
			PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personID);
			if (personResponse == null)
			{
				return RedirectToAction("Index");
			}

			PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

			List<CountryResponse> countries = await _countriesService.GetAllCountries();
			ViewBag.Countries = countries.Select(temp =>
			new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

			return View(personUpdateRequest);
		}

		[HttpPost]
		[Route("[action]/{personID}")]
        [TypeFilter(typeof(PersonsCreateAndEditPostActionFilter))]
        [TypeFilter(typeof(TokenAuthorizationFilter))]
        
        public async Task<IActionResult> Edit(PersonUpdateRequest personRequest)
		{
			PersonResponse? personResponse =await _personsGetterService.GetPersonByPersonID(personRequest.PersonID);

			if (personResponse == null)
			{
				return RedirectToAction("Index");
			}

			
			PersonResponse updatedPerson = await _personsUpdaterService.UpdatePerson(personRequest);
			return RedirectToAction("Index");
			
			
		}


		[HttpGet]
		[Route("[action]/{personID}")]
		public async Task<IActionResult> Delete(Guid? personID)
		{
			PersonResponse? personResponse =await _personsGetterService.GetPersonByPersonID(personID);
			if (personResponse == null)
				return RedirectToAction("Index");

			return View(personResponse);
		}

		[HttpPost]
		[Route("[action]/{personID}")]
		public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateResult)
		{
			PersonResponse? personResponse =await _personsGetterService.GetPersonByPersonID(personUpdateResult.PersonID);
			if (personResponse == null)
				return RedirectToAction("Index");

			await _personsDeleterService.DeletePerson(personUpdateResult.PersonID);
			return RedirectToAction("Index");
		}

		[Route("[action]")]
		public async Task<IActionResult> PersonsPDF() {

            //Get list of Persons
           List<PersonResponse> persons= await _personsGetterService.GetAllPersons();
            //REturn view as PDF
            return new ViewAsPdf("PersonsPDF", persons, ViewData) { 
            PageMargins=new Rotativa.AspNetCore.Options.Margins() {Top=20, Right=20, Bottom=20, Left=20 },
            PageOrientation= Rotativa.AspNetCore.Options.Orientation.Landscape

			};
        }

		[Route("[action]")]
		public async Task<IActionResult> PersonsCSV()
		{
           MemoryStream memoryStream=await _personsGetterService.GetPersonsCSV();
            return File(memoryStream, "application/octet-stream","persons.csv");
			
		}

		[Route("[action]")]
		public async Task<IActionResult> PersonsExcel()
		{
			MemoryStream memoryStream = await _personsGetterService.GetPersonsExcel();
			return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");

		}
	}

}

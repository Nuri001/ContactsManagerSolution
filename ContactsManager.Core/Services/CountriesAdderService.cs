using Entities;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
	public class CountriesAdderService : ICountriesAdderService
    {
		//private field
		private readonly ICountriesRepository _countriesRepository;
		//constractor
		public CountriesAdderService(ICountriesRepository countriesRepository)
		{
			_countriesRepository = countriesRepository;
		}
		public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
		{
			//validation: countryAddRequesr parameter can't be null
			if (countryAddRequest == null) { throw new ArgumentNullException(nameof(countryAddRequest)); }
			//validation: countryName can't be null
			if (countryAddRequest.CountryName == null) { throw new ArgumentException(nameof(countryAddRequest.CountryName)); }
			//validation: CountryName can't be duplicate
			if (await _countriesRepository.GetCountryByCountryName(countryAddRequest.CountryName) != null)
			{
				throw new ArgumentException("Given country name already exists");
			}

			//Convert object from countryAddRequest to Country type
			Country country = countryAddRequest.ToCountry();
			//generate CountryID
			country.CountryID = Guid.NewGuid();

			//Add country object into _countries list
			await _countriesRepository.AddCountry(country);
			return country.ToCountryResponse();

		}

	}
}
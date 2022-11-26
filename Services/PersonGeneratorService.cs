using Bogus;
using Bogus.DataSets;
using Task_5.Models;
using CsvHelper;
using System.Globalization;
using System.Runtime.Serialization;

namespace Task_5.Services
{
    public enum Region
    {
        fr, pl, lv
    }

    public interface IPersonGeneratorService
    {
        public PersonModel GeneratePerson(int seed, Region region);
        public List<PersonModel> GeneratePersons(int seed, Region region, int amountOfPersons, int amountOfMistakes);
        public MemoryStream GetPersonsInCsvFile();
    }

    public class PersonGeneratorService : IPersonGeneratorService
    {
        private Faker _faker = new();
        private int _currentSeed;
        private int _currentAmountOfMistakes;
        private List<PersonModel> _currentListOfPersons = new();

        private const string PL_CODE = "+48";
        private const string FR_CODE = "+33";
        private const string LV_CODE = "+371";

        private const int PL_MIN_MASK = 501111111;
        private const int PL_MAX_MASK = 889999999;
        private const int FR_MIN_MASK = 611111111;
        private const int FR_MAX_MASK = 789999999;
        private const int LV_MIN_MASK = 21111111;
        private const int LV_MAX_MASK = 29999999;
        private const int BUILDING_NUMBER_MIN = 1;
        private const int BUILDING_NUMBER_MAX = 150;
        private const int APARTMENT_NUMBER_MIN = 1;
        private const int APARTMENT_NUMBER_MAX = 255;

        private const int DEFAULT_SWITCH_MISTAKE_PROBABILITY_THERESHOLD = 800;
        private const int DEFAULT_DELETE_MISTAKE_PROBABILITY_THERESHOLD = 850;
        private const int DEFAULT_ADD_MISTAKE_PROBABILITY_THERESHOLD = 999;

        private int _currentSwitchProbabilityThreshold = DEFAULT_SWITCH_MISTAKE_PROBABILITY_THERESHOLD;
        private int _currentDeleteProbabilityThreshold = DEFAULT_SWITCH_MISTAKE_PROBABILITY_THERESHOLD;
        private int _currentAddProbabilityThreshold = DEFAULT_SWITCH_MISTAKE_PROBABILITY_THERESHOLD;

        public PersonModel GeneratePerson(int seed, Region region )
        {
            Randomizer.Seed = new Random(seed);
            _faker.Locale = Enum.GetName(region.GetType(), region);
            PersonModel person = new PersonModel();
            person.Fullname = _faker.Name.FullName();
            var city = _faker.Address.City();
            var street = _faker.Address.StreetName();
            var buildingNumber = _faker.Random.Number(BUILDING_NUMBER_MIN, BUILDING_NUMBER_MAX);
            var apartmentNumber = "";
            int roll = _faker.Random.Number(10);
            if (roll <= 4)
                apartmentNumber = $", {_faker.Random.Number(APARTMENT_NUMBER_MIN,APARTMENT_NUMBER_MAX)}";
            person.FullAddress = $"{city}, {street} {buildingNumber}{apartmentNumber}";
            person.Id = _faker.Random.Uuid().ToString();
            person.Num = _faker.IndexFaker;
            person.PhoneNumber = GeneratePhoneNumber();
            _faker.IndexFaker++;
            return person;
        }

        public List<PersonModel> GeneratePersons(int seed, Region region, int amountOfPersons, int amountOfMistakes)
        {
            ChangeFakerContext(region, seed, amountOfMistakes);
            for (int i = 0; i < amountOfPersons; i++)
            {
                PersonModel person = GeneratePerson(seed, region);
                if (_currentAmountOfMistakes > 0)
                    CorruptData(person);
                _currentListOfPersons.Add(person);
            }
            return _currentListOfPersons;
        }


        private void CorruptData(PersonModel person)
        {
            for (int i = 0; i < _currentAmountOfMistakes; i++)
            {
                int fieldNumber = _faker.Random.Int(0, 3);
                switch (fieldNumber)
                {
                    case 0:
                        person.Id = AddMistakesToPersonData(person.Id); break;
                    case 1:
                        person.Fullname = AddMistakesToPersonData(person.Fullname); break;
                    case 2:
                        person.FullAddress = AddMistakesToPersonData(person.FullAddress); break;
                    case 3:
                        person.PhoneNumber = AddMistakesToPersonData(person.PhoneNumber); break;
                }
            }
            _currentSwitchProbabilityThreshold = DEFAULT_SWITCH_MISTAKE_PROBABILITY_THERESHOLD;
            _currentDeleteProbabilityThreshold = DEFAULT_DELETE_MISTAKE_PROBABILITY_THERESHOLD;
            _currentAddProbabilityThreshold = DEFAULT_ADD_MISTAKE_PROBABILITY_THERESHOLD;
        }

        private string AddMistakesToPersonData(string dataToModify)
        {
            int mistakeNumber = 2;
            int rolledNum = _faker.Random.Int(1, DEFAULT_ADD_MISTAKE_PROBABILITY_THERESHOLD);
            if (rolledNum <= _currentSwitchProbabilityThreshold)
                mistakeNumber = 0;
            if (rolledNum > _currentSwitchProbabilityThreshold && rolledNum <=_currentDeleteProbabilityThreshold)
                mistakeNumber = 1;
            if (rolledNum > _currentDeleteProbabilityThreshold)
                mistakeNumber = 2;
            switch (mistakeNumber)
            {
                case 0:
                    return MakeSwitchedCharsMistake(dataToModify);
                case 1:
                    return MakeDeletedCharMistake(dataToModify);
                case 2:
                    return MakeAdditionalCharMistake(dataToModify);
            }
            return "";
        }

        private string MakeDeletedCharMistake(string data)
        {
            if(_currentDeleteProbabilityThreshold > _currentSwitchProbabilityThreshold + 1)
                _currentDeleteProbabilityThreshold--;
            int indexToDelete = _faker.Random.Int(0, data.Length-1);
            data = data.Remove(indexToDelete,1);
            return data;
        }
        private string MakeAdditionalCharMistake(string data)
        {
            if (_currentDeleteProbabilityThreshold < _currentAddProbabilityThreshold-1)
                _currentDeleteProbabilityThreshold++;
            int indexToAdd = _faker.Random.Int(0, data.Length);
            string characterToAdd = _faker.Random.AlphaNumeric(1);
            data = data.Insert(indexToAdd, characterToAdd);
            return data;
        }
        private string MakeSwitchedCharsMistake(string data)
        {
            int firstIndex = _faker.Random.Int(0, data.Length - 2);
            int secondIndex = firstIndex + 1;
            string switchedPart = String.Join("", data[secondIndex], data[firstIndex]);
            data = data.Remove(firstIndex, 2).Insert(firstIndex, switchedPart);
            return data;
        }

        private void ChangeFakerContext(Region region, int seed, int amountOfMistakes)
        {
            var currentRegion = Enum.GetName(region.GetType(), region);
            if (!_faker.Locale.Equals(currentRegion) || _currentSeed != seed || _currentAmountOfMistakes != amountOfMistakes)
            {
                _faker = new Faker(currentRegion);
                _currentSeed = seed;
                _currentAmountOfMistakes= amountOfMistakes;
                _currentListOfPersons = new();
                _faker.IndexFaker += 2;
            }
        }

        private string GeneratePhoneNumber()
        {
            string phoneNumber = string.Empty;
            Region currentRegion = (Region)Enum.Parse(typeof(Region), _faker.Locale);
            if (currentRegion == Region.pl)
            {
                phoneNumber = $"({PL_CODE}) {_faker.Random.UInt(PL_MIN_MASK, PL_MAX_MASK)}";
            }
            if (currentRegion == Region.fr)
            {
                phoneNumber = $"({FR_CODE}) {_faker.Random.UInt(FR_MIN_MASK, FR_MAX_MASK)}";
            }
            if (currentRegion == Region.lv)
            {
                phoneNumber = $"({LV_CODE}) {_faker.Random.UInt(LV_MIN_MASK, LV_MAX_MASK)}";
            }
            return phoneNumber;
        }

        public MemoryStream GetPersonsInCsvFile()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(_currentListOfPersons);
            stream.Position = 0;
            return stream;
        }
    }
}
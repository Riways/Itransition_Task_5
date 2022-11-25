using CsvHelper;
using System.Globalization;
using Task_5.Models;

namespace Task_5.Services
{
    public interface ICsvExportService
    {
        public void GetCsvFileFromList(List<PersonModel> persons);
    }

    public class CsvExportService : ICsvExportService
    {
        void ICsvExportService.GetCsvFileFromList(List<PersonModel> persons)
        {
            using (var writer = new StreamWriter("Persons.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(persons);
            }

        }
    }
}

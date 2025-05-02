using InspectionSendImagesAndAnnotations.Channel.Services;
using InspectionSendImagesAndAnnotations.Messages;
using InspectionSendImagesAndAnnotations.Messages.Dtos;
using MongoDB.Bson;
using MongoDB.Driver;

namespace InspectionSendImagesAndAnnotations.Channel.Services
{
    public class InspectionService : MongoConnect
    {

        public InspectionService(string ConnectionString) : base(ConnectionString)
        {
        }

        public async Task<string> InsertOnFinishedImageScanning(ImageTrainingResponse request, string inspectionname, string numofimgs, string county)
        {
            var database = dbClient.GetDatabase("InspectionAppDatabase");
            var collection = database.GetCollection<BsonDocument>("InspectionDetails");


            var document = new BsonDocument {
                {"id",  request.id.ToString() },
                { "Inspectionname", inspectionname },
                { "NumberOfImages", numofimgs },
                { "County", county },
                { "Overall Training Loss Rate", request.overallLoss },
                { "Status", request.status }
            };

            try
            {
                await collection.InsertOneAsync(document);
                return "Success";
            }
            catch (Exception e)
            {
                return "Failed" + e.Message;
            }
        }


    }
}
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.Diagnostics;

using System.Runtime.InteropServices.WindowsRuntime;

namespace SmartMirror
{
    public partial class MainPage
    {
        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("0c5c804cfbe345de8a120fe839ea1d9d");
        string personGroup = "mirrorsmart";

        private async void SetupPersonGroup()
        {
            const string lilianImageDir = @"Assets\PersonGroup\Lilian\";

            try
            {
                //if (faceServiceClient.) <-- need to check if it exists first
                    await faceServiceClient.CreatePersonGroupAsync(personGroup, personGroup);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }

            // Define Users
            CreatePersonResult lilian = await faceServiceClient.CreatePersonAsync(
                personGroup,
                "Lilian"
            );

            foreach (string imagePath in Directory.GetFiles(lilianImageDir, "*.jpg"))
            {
                using (Stream s = File.OpenRead(imagePath))
                {
                    // Detect faces in the image and add to Lilian
                    await faceServiceClient.AddPersonFaceAsync(
                        personGroup, lilian.PersonId, s);
                }
            }

            //Train model
            try
            {
                await faceServiceClient.TrainPersonGroupAsync(personGroup);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            TrainingStatus trainingStatus = null;
            while (true)
            {
                trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroup);

                if (trainingStatus.Status.ToString() != "running")
                {
                    break;
                }

                await Task.Delay(1000);
            }
        }

        private async Task<string> IdentifyUser(string imageName)
        {
            StorageFolder appFolder = ApplicationData.Current.LocalCacheFolder;
            StorageFile file = await appFolder.GetFileAsync(imageName);
            using (var randomAccessStream = await file.OpenReadAsync())
            using (Stream s = randomAccessStream.AsStreamForRead())
            {
                var faces = await faceServiceClient.DetectAsync(s);
                var faceIds = faces.Select(face => face.FaceId).ToArray();

                var results = await faceServiceClient.IdentifyAsync(personGroup, faceIds);
                foreach (var identifyResult in results)
                {
                    Debug.WriteLine("Result of face: {0}", identifyResult.FaceId);

                    if (identifyResult.Candidates.Length == 0)
                    {
                        Debug.WriteLine("No one identified");
                    }
                    else
                    {
                        // Get top 1 among all candidates returned
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = await faceServiceClient.GetPersonAsync(personGroup, candidateId);
                        userName = person.Name;
                        Debug.WriteLine("Identified as {0}", userName);
                        IdentityTextBlock.Text = "Hey there, " + userName + "!";
                        await Speak("Hello, " + userName);
                    }
                }
            }

            return userName;
        }
    }
}

using Firebase.Storage;
using System.IO;
using System.Threading.Tasks;

namespace PROJECT.Services
{
    public class FirebaseStorageService
    {
        // REPLACE with your actual bucket name from Firebase Console -> Storage
        // It usually looks like: "your-app-id.appspot.com"
        private const string FirebaseStorageBucket = "mad-mental.firebasestorage.app";

        private readonly FirebaseStorage _firebaseStorage;

        public FirebaseStorageService()
        {
            _firebaseStorage = new FirebaseStorage(FirebaseStorageBucket);
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
        {
            // Uploads to a folder named "journal_images"
            // Returns the public Download URL
            var imageUrl = await _firebaseStorage
                .Child("journal_images")
                .Child(fileName)
                .PutAsync(fileStream);

            return imageUrl;
        }
    }
}
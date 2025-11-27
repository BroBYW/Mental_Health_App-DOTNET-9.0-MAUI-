using Firebase.Storage;
using System.IO;
using System.Threading.Tasks;

namespace PROJECT.Services
{
    public class FirebaseStorageService
    {
        // Keep your existing bucket name
        private const string FirebaseStorageBucket = "mad-mental.firebasestorage.app";

        private readonly FirebaseStorage _firebaseStorage;

        public FirebaseStorageService()
        {
            _firebaseStorage = new FirebaseStorage(FirebaseStorageBucket);
        }

        // UPDATED: Added optional 'folderName' parameter
        public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string folderName = "journal_images")
        {
            var imageUrl = await _firebaseStorage
                .Child(folderName)
                .Child(fileName)
                .PutAsync(fileStream);

            return imageUrl;
        }
    }
}
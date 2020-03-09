using Amazon;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace S3Browser.Handlers
{
    class S3ClientHandler
    {
        private CredentialProfile profile;
        private RegionEndpoint region;
        private AWSCredentials awsCredentials;
        private AmazonS3Client amazonS3Client;
        private List<S3Bucket> buckets;

        internal S3ClientHandler()
        {
        }

        internal void SetProfile(CredentialProfile profile)
        {
            this.profile = profile;
            this.InitialS3Client();
        }

        internal void SetRegionEndpoint(RegionEndpoint region)
        {
            this.region = region;
            this.InitialS3Client();
        }

        private void InitialS3Client()
        {
            if(this.profile != null && this.region != null)
            {
                this.awsCredentials = this.profile.GetAWSCredentials(profile.CredentialProfileStore);
                this.amazonS3Client = new AmazonS3Client(this.awsCredentials, this.region);
            }
        }
        internal List<CredentialProfile> ListProfiles()
        {
            CredentialProfileStoreChain configFile = new CredentialProfileStoreChain();
            return configFile.ListProfiles();
        }

        internal async Task<List<S3Bucket>> ListBuckets()
        {
            ListBucketsResponse response = await this.amazonS3Client.ListBucketsAsync();
            this.buckets = response.Buckets;
            return this.buckets;
        }

        internal S3Bucket GetBucket(int selectedIndex)
        {
            return this.buckets[selectedIndex];
        }

        internal async Task<ListObjectsV2Response> ListObjectsV2Async(ListObjectsV2Request request)
        {
            return await this.amazonS3Client.ListObjectsV2Async(request);
        }

        internal bool GetObject(GetObjectRequest request, string downloadFilePath)
        {
            bool isSuccess = false;

            GetObjectResponse response = this.amazonS3Client.GetObject(request);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                using (Stream inputStream = response.ResponseStream)
                using (Stream outputStream = File.Create(downloadFilePath))
                {
                    byte[] buffer = new byte[8 * 1024];
                    int len;
                    while ((len = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outputStream.Write(buffer, 0, len);
                    }

                    inputStream.Close();
                    outputStream.Close();
                }
                isSuccess = true;
            }
            return isSuccess;
        }

        internal bool Encrypt(FileStream inputStream, out MemoryStream encStream)
        {
            bool isSuccess = false;

            var memoryStream = new MemoryStream();
            inputStream.CopyTo(memoryStream);

            var kmsClient = new AmazonKeyManagementServiceClient(this.awsCredentials, this.region);

            var encRequest = new EncryptRequest
            {
                KeyId = "arn:aws:kms:ap-northeast-2:677963422510:key/4fcb8b76-e1fe-4b5d-90b0-37c3e6dd5e4c"
                ,
                Plaintext = memoryStream
            };

            var encResponse = kmsClient.Encrypt(encRequest);
            if (encResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                isSuccess = true;
            }
            encStream = encResponse.CiphertextBlob;
            return isSuccess;
        }

        internal bool PutObject(PutObjectRequest request)
        {
            bool isSuccess = false;

            var response = this.amazonS3Client.PutObject(request);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        internal void DeleteObject(string bucketName, string key)
        {
            this.amazonS3Client.DeleteObject(bucketName, key);
        }

    }
}

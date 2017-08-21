using System;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using System.Collections.Generic;
using System.IO;

namespace StorageAccess
{
    public class AccessStorageS3
    {
        IAmazonS3 client;
        public AccessStorageS3()
        {
            client = new Amazon.S3.AmazonS3Client("",
                "", Amazon.RegionEndpoint.APSoutheast2);
        }

        public bool CreateBucket(string bucketName)
        {

            if (!(AmazonS3Util.DoesS3BucketExist(client, bucketName)))
            {
                return CreateABucket(bucketName);
            }
            // Retrieve bucket location.
            return false;
        }

        public string FindBucketLocation( string bucketName)
        {
            string bucketLocation;
            GetBucketLocationRequest request = new GetBucketLocationRequest()
            {
                BucketName = bucketName
            };
            GetBucketLocationResponse response = client.GetBucketLocation(request);
            bucketLocation = response.Location.ToString();
            return bucketLocation;
        }

        public bool CreateABucket(string bucketName)
        {
            try
            {
                PutBucketRequest putRequest1 = new PutBucketRequest
                {
                    BucketName = bucketName,
                    UseClientRegion = true
                };

                PutBucketResponse response1 = client.PutBucket(putRequest1);

            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Check the provided AWS Credentials.");
                }
                else
                {
                    Console.WriteLine(
                        "Error occurred. Message:'{0}' when writing an object"
                        , amazonS3Exception.Message);
                }
                return false;
            }
            return true;
        }
        public bool uploadObject(System.IO.Stream file,
            string existingBucketName, string fileName)
        {

            // List to store upload part responses.
            List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();

            // 1. Initialize.
            InitiateMultipartUploadRequest initiateRequest = new InitiateMultipartUploadRequest
            {
                BucketName = existingBucketName,
                Key = fileName
            };

            InitiateMultipartUploadResponse initResponse =
                client.InitiateMultipartUpload(initiateRequest);

            // 2. Upload Parts.
            long contentLength = file.Length;
            long partSize = 5 * (long)Math.Pow(2, 20); // 5 MB

            try
            {
                long filePosition = 0;
                for (int i = 1; filePosition < contentLength; i++)
                {
                    UploadPartRequest uploadRequest = new UploadPartRequest
                    {
                        BucketName = existingBucketName,
                        Key = fileName,
                        UploadId = initResponse.UploadId,
                        PartNumber = i,
                        PartSize = partSize,
                        FilePosition = filePosition,
                        InputStream = file
                    };

                    // Upload part and add response to our list.
                    uploadResponses.Add(client.UploadPart(uploadRequest));

                    filePosition += partSize;
                }

                // Step 3: complete.
                CompleteMultipartUploadRequest completeRequest = new CompleteMultipartUploadRequest
                {
                    BucketName = existingBucketName,
                    Key = fileName,
                    UploadId = initResponse.UploadId,
                    
                };
                completeRequest.AddPartETags(uploadResponses);

                CompleteMultipartUploadResponse completeUploadResponse =
                   client.CompleteMultipartUpload(completeRequest);

            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception occurred: {0}", exception.Message);
                AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
                {
                    BucketName = existingBucketName,
                    Key = fileName,
                    UploadId = initResponse.UploadId
                };
               client.AbortMultipartUpload(abortMPURequest);
                return false;
            }
            return true;
        }
        public List<string> ListObjects(string bucketName)
        {
            List<string> listOfObjects = new List<string>();

            try
            {
                ListObjectsV2Request request = new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    MaxKeys = 10
                };
                ListObjectsV2Response response;
                do
                {
                    response = client.ListObjectsV2(request);

                    // Process response.
                    foreach (S3Object entry in response.S3Objects)
                    {

                        listOfObjects.Add(entry.Key);
                    }
                    request.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated == true);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return listOfObjects;

        }
        public System.IO.Stream GetContent (string bucketName, string keyName)
        {
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = keyName
            };

            using (GetObjectResponse response = client.GetObject(request))
            {
                 return response.ResponseStream;
            }
        }

    }
}
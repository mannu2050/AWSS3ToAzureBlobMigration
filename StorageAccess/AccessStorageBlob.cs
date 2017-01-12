using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using System.IO;
using Microsoft.ServiceBus.Messaging;

namespace StorageAccess
{
    public class AccessStorageBlob
    {
        CloudStorageAccount storageAccount;
        public AccessStorageBlob()
        {
            // Parse the connection string and return a reference to the storage account.
            storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials("testify", "5nThM3H9qA3JJ65514epeZ0RljkBUEeJ5fpCb9pdFBYfuDfPj3EweIeWr4uG+iBvsI7YzVgAyRMW6VPXZoTJgQ==", "primary")
                , true);
        }
        public bool CreateBucket(string bucketName)
        {
            CloudBlobClient cb = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer cbc = cb.GetContainerReference(bucketName);
            return cbc.CreateIfNotExists();
        }
        public bool uploadObject(System.IO.Stream file,
            string existingBucketName, string fileName)
        {
            CloudBlobClient cb = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer cbc = cb.GetContainerReference(existingBucketName);
            CloudBlockBlob cbb = cbc.GetBlockBlobReference(fileName);

            cbb.UploadFromStream(file);
            return true;
        }

        public List<string> ListObjects(string bucketName)
        {
            CloudBlobClient cb = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer cbc = cb.GetContainerReference(bucketName);
            List<string> listOfBlobs = new List<string>();
            foreach (var blob in cbc.ListBlobs(null, false))
            {
                if (blob.GetType() == typeof(CloudBlockBlob))
                {
                    listOfBlobs.Add(((CloudBlockBlob)blob).Name);

                }
            }
            return listOfBlobs;
        }

        public string ConvertS3ToBlobURI(string S3Uri)
        {
            return S3Uri.Replace("amazonaws.com", "blob.core.windows.net");
        }

        public System.IO.Stream GetContent(string bucketName, string keyName)
        {
            CloudBlobClient cb = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer cbc = cb.GetContainerReference(bucketName);
            CloudBlockBlob cbb = cbc.GetBlockBlobReference(keyName);
            Stream st = new MemoryStream();
            if (cbb.Exists())
            {
                cbb.DownloadToStream(st);
            }
            else
            {
                //If content does not exists in Blob Storage
                //then fetch the content from S3
                AccessStorageS3 objs3 = new AccessStorageS3();
                st = objs3.GetContent(bucketName, keyName);

                //Now prioritize the migration of accessed content
                var connectionString = "Endpoint=sb://storagemigration.servicebus.windows.net/;SharedAccessKeyName=prioritizationpolicy;SharedAccessKey=JXvT/lf500i5jCifE1poVfqCqpiPt33wzgqeBoU5SRo=";
                var queueName = "contentmigrationprioritizationqueue";

                var client = QueueClient.CreateFromConnectionString(connectionString, queueName);
                var message = new BrokeredMessage(bucketName + "<SplitString>" + keyName);
                client.Send(message);

            }
            return st;
        }
    }
}
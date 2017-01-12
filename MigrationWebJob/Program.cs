using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationWebJob
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "Endpoint=sb://storagemigration.servicebus.windows.net/;SharedAccessKeyName=migrationservicepolicy;SharedAccessKey=zYOzygnwOkFbEdJgQlZiiYvS53Oqrxg3Iw5mw7VyZR4=";
            var queueName = "contentmigrationprioritizationqueue";

            var queueClient = QueueClient.CreateFromConnectionString(connectionString, queueName);
            while (true)
            {
                BrokeredMessage message = queueClient.Receive();

                try
                {
                    ProcessMessage(message);
                    message.Complete();
                }
                catch (Exception e)
                {
                    message.Abandon();
                }
                
            }
        }

        private static void ProcessMessage(BrokeredMessage message)
        {
            AmazonS3Client client = new AmazonS3Client("AKIAIN3OFTQNK5NUJ2KA",
             "pQAJlH21mfzO1tQLw6TpRYKTDEwo5MGtU1VFecRw", Amazon.RegionEndpoint.APSoutheast2);


            string bucketName = string.Empty;
            string keyName = string.Empty;
            string splitString = "<SplitString>";

            string msg = message.GetBody<string>();
            if (!string.IsNullOrEmpty(msg))
            {
                int idx = msg.IndexOf(splitString);

                bucketName = msg.Substring(0, idx);
                keyName = msg.Substring(idx + splitString.Length);

                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };

                using (GetObjectResponse response = client.GetObject(request))
                {
                    response.WriteResponseStreamToFile(keyName);
                }

                CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials("testify", "5nThM3H9qA3JJ65514epeZ0RljkBUEeJ5fpCb9pdFBYfuDfPj3EweIeWr4uG+iBvsI7YzVgAyRMW6VPXZoTJgQ==", "primary")
        , true);
                CloudBlobClient cb = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer cbc = cb.GetContainerReference(bucketName);
                cbc.CreateIfNotExists();
                CloudBlockBlob cbb = cbc.GetBlockBlobReference(keyName);

                using (FileStream fs = new FileStream(keyName, FileMode.Open))
                {
                    cbb.UploadFromStream(fs);
                    fs.Close();
                    File.Delete(keyName);
                }

            }
        }
    }
}
